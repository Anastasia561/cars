namespace cars.Models;

public class RentalCreationDto
{
    public ClientCreationDto client { get; set; }
    public int CarId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}