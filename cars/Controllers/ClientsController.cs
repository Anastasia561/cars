using cars.Models;
using cars.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace cars.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientRepository _clientRepository;
    private readonly ICarRepository _carRepository;

    public ClientsController(IClientRepository clientRepository, ICarRepository carRepository)
    {
        _clientRepository = clientRepository;
        _carRepository = carRepository;
    }

    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetClient(int clientId, CancellationToken token)
    {
        if (!await _clientRepository.ClientExistsAsync(clientId, token))
            return BadRequest($"Client with id {clientId} does not exist");

        var result = await _clientRepository.GetClientAsync(clientId, token);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddClientWithRental(RentalCreationDto dto, CancellationToken token)
    {
        if (!await _carRepository.CarExistsAsync(dto.CarId, token))
            return BadRequest($"Car with id {dto.CarId} does not exist");

        await _clientRepository.AddClientWithRentalAsync(dto, token);
        return Ok();
    }
}