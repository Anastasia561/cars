using cars.Models;

namespace cars.Repositories;

public interface IClientRepository
{
    public Task<bool> ClientExistsAsync(int id, CancellationToken token);
    public Task<Client> GetClientAsync(int id, CancellationToken token);
    public Task AddClientWithRentalAsync(RentalCreationDto dto, CancellationToken token);
}