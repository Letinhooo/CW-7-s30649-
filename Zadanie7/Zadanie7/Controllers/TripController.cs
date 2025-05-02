using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zadanie7.DTOs;
using Zadanie7.Exceptions;
using Zadanie7.Services;

namespace Zadanie7.Controllers;

[ApiController]
[Route("api")]
public class TripsController(IDbService service) : ControllerBase
{
    //wyciaganie wszystkich wycieczek
    [HttpGet("trips")]
    public async Task<IActionResult> GetTripDetails()
    {
        return Ok(await service.GetTripsDetailsAsync());
    }
    //wyciaganie wycieczek danego klienta
    [HttpGet("clients/{id}/trips")]
    public async Task<IActionResult> GetTripDetailsByID([FromRoute]int id)
    {
        try
        {
            return Ok(await service.GetTripsDetailsByIDAsync(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    //tworzenie klienta
    [HttpPost("clients")]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientDto body)
    {
        var newClientId= await service.CreateClientAsync(body);
        return Created("id",newClientId);
    }
    //dodawanie nowej wycieczki dla klienta
    [HttpPut("clients/{id}/trips/{tripId}")]
    public async Task<IActionResult> UpdateClient([FromRoute] int id, [FromRoute] int tripId)
    {
        try
        {
            await service.AddClientToTripAsync(id, tripId);
            return Ok("Client updated");
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    //Usuwanie powiązania klient wycieczka
    [HttpDelete("clients/{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteClientAndTrip([FromRoute] int IdClient, [FromRoute] int IdTrip)
    {
        try
        {
            await service.DeleteClientAsync(IdClient, IdTrip);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}