using System.ComponentModel.DataAnnotations;

namespace Zadanie7.DTOs;

public class TripDetailsGetDto
{
    public int IdTrip { get; set; }
    [MaxLength(120)]
    public string Name { get; set; }
    [MaxLength(220)]
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    [MaxLength(120)]
    public string Country { get; set; }
}