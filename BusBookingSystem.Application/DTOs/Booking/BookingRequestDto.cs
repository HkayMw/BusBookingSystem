using System.ComponentModel.DataAnnotations;

public class BookingRequestDto
{
    [Required]
    public Guid TripId { get; set; }

    [Range(0, int.MaxValue)]
    public int NumberOfSeats { get; set; }

    [Range(0, int.MaxValue)]
    public int CargoWeight { get; set; }
}