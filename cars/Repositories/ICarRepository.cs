namespace cars.Repositories;

public interface ICarRepository
{
    public Task<bool> CarExistsAsync(int id, CancellationToken token);
}