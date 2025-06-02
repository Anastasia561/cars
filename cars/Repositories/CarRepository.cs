using Microsoft.Data.SqlClient;

namespace cars.Repositories;

public class CarRepository : ICarRepository
{
    private readonly string _connectionString;

    public CarRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<bool> CarExistsAsync(int id, CancellationToken token)
    {
        await using var con = new SqlConnection(_connectionString);
        await using var com = new SqlCommand("SELECT COUNT(*) FROM cars WHERE ID=@id", con);
        com.Parameters.AddWithValue("@id", id);

        await con.OpenAsync(token);
        var result = (int)await com.ExecuteScalarAsync(token);
        return result > 0;
    }
}