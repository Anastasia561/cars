using cars.Models;
using Microsoft.Data.SqlClient;

namespace cars.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly string _connectionString;

    public ClientRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<bool> ClientExistsAsync(int id, CancellationToken token)
    {
        await using var con = new SqlConnection(_connectionString);
        await using var com = new SqlCommand("SELECT COUNT(*) FROM clients WHERE ID=@id", con);
        com.Parameters.AddWithValue("@id", id);

        await con.OpenAsync(token);
        var result = (int)await com.ExecuteScalarAsync(token);
        return result > 0;
    }

    public async Task<Client> GetClientAsync(int id, CancellationToken token)
    {
        await using var con = new SqlConnection(_connectionString);
        await using var com = new SqlCommand(
            " select c.ID   as clientId, " +
            "FirstName, LastName, Address, DateFrom, DateTo, TotalPrice, VIN, m.Name as modelName, colors.Name as colorName " +
            " from clients c join car_rentals cr on c.ID = cr.ClientID " +
            "join cars on cr.CarID = cars.ID " +
            "join colors on cars.ColorID = colors.ID " +
            "join models m on cars.ModelID = m.ID where c.ID = @id", con);
        com.Parameters.AddWithValue("@id", id);

        await con.OpenAsync(token);
        var reader = await com.ExecuteReaderAsync(token);
        Client dto = null;

        while (await reader.ReadAsync(token))
        {
            if (dto is not null)
            {
                dto.Rentals.Add(new RentalDto()
                {
                    Vin = reader["VIN"].ToString(),
                    Color = reader["colorName"].ToString(),
                    Model = reader["modelName"].ToString(),
                    DateFrom = Convert.ToDateTime(reader["DateFrom"]),
                    DateTo = Convert.ToDateTime(reader["DateTo"]),
                    TotalPrice = (int)reader["TotalPrice"]
                });
            }
            else
            {
                dto = new Client()
                {
                    Id = int.Parse(reader["clientId"].ToString()),
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Address = reader["Address"].ToString(),
                    Rentals = new List<RentalDto>()
                    {
                        new RentalDto()
                        {
                            Vin = reader["VIN"].ToString(),
                            Color = reader["colorName"].ToString(),
                            Model = reader["modelName"].ToString(),
                            DateFrom = Convert.ToDateTime(reader["DateFrom"]),
                            DateTo = Convert.ToDateTime(reader["DateTo"]),
                            TotalPrice = (int)reader["TotalPrice"]
                        }
                    }
                };
            }
        }

        return dto;
    }

    public async Task AddClientWithRentalAsync(RentalCreationDto dto, CancellationToken token)
    {
        await using var con = new SqlConnection(_connectionString);
        await using var com = new SqlCommand("insert into clients " +
                                             "values(@firstName, @lastName, @address); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            con);
        com.Parameters.AddWithValue("@firstName", dto.client.FirstName);
        com.Parameters.AddWithValue("@lastName", dto.client.LastName);
        com.Parameters.AddWithValue("@address", dto.client.Address);

        await con.OpenAsync(token);

        var transaction = await con.BeginTransactionAsync(token);
        com.Transaction = transaction as SqlTransaction;

        try
        {
            var clientId = (int)await com.ExecuteScalarAsync(token);
            com.Parameters.Clear();

            com.CommandText = "select PricePerDay from cars where ID=@carId";
            com.Parameters.AddWithValue("@carId", dto.CarId);
            var dayPrice = (int)await com.ExecuteScalarAsync(token);

            var totalPrice = (dto.DateTo - dto.DateFrom).Days * dayPrice;

            com.Parameters.Clear();

            com.CommandText = "insert into car_rentals(ClientID, CarID, DateFrom, DateTo, TotalPrice) " +
                              "values(@clientId, @carId, @dateFrom, @dateTo, @totalPrice)";
            com.Parameters.AddWithValue("@clientId", clientId);
            com.Parameters.AddWithValue("@carId", dto.CarId);
            com.Parameters.AddWithValue("@dateFrom", dto.DateFrom);
            com.Parameters.AddWithValue("@dateTo", dto.DateTo);
            com.Parameters.AddWithValue("@totalPrice", totalPrice);

            await com.ExecuteNonQueryAsync(token);


            await transaction.CommitAsync(token);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }
}