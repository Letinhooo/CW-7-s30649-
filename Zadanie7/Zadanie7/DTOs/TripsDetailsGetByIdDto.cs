using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Zadanie7.DTOs;

public class TripsDetailsGetByIdDto
{
    [MaxLength(120)]
    public string Name { get; set; }
    [MaxLength(220)]
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}