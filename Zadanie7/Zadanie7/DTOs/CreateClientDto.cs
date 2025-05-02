using System.ComponentModel.DataAnnotations;

namespace Zadanie7.DTOs;

public class CreateClientDto
{
    public int Id { get; set; }
    [Required]
    [MaxLength(120)]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(120)]
    public string LastName { get; set; }
    [Required]
    [MaxLength(120)]
    public string email { get; set; }
    [Required]
    [MaxLength(120)]
    public string telephone { get; set; }
    [Required]
    [MaxLength(120)]
    public string pesel { get; set; }
}