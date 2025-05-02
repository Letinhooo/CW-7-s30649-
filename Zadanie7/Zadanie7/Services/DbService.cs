using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Zadanie7.DTOs;
using Zadanie7.Exceptions;

namespace Zadanie7.Services;

public interface IDbService
{
    public Task<IEnumerable<TripDetailsGetDto>> GetTripsDetailsAsync();
    public Task<IEnumerable<TripsDetailsGetByIdDto>> GetTripsDetailsByIDAsync(int id);
    public Task<int> CreateClientAsync(CreateClientDto body);
    public Task AddClientToTripAsync(int IdClient, int IdTrip);
    public Task DeleteClientAsync(int IdClient, int IdTrip);
}
public class DbService(IConfiguration config) : IDbService
{
    private async Task<SqlConnection> GetConnectionAsync()
    {
        var connection = new SqlConnection(config.GetConnectionString("Default-db"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        return connection;
    }

    public async Task<IEnumerable<TripDetailsGetDto>> GetTripsDetailsAsync()
    {
        await using var connection = await GetConnectionAsync();
        var sql = """
                  Select w.IdTrip,w.Name,w.Description,w.DateFrom,w.DateTo,c.Name as CountryName from Trip w 
                  join Country_Trip ct on ct.IdCountry = w.IdTrip
                  join Country c on c.IdCountry = ct.IdCountry
                  """;
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var trips = new List<TripDetailsGetDto>();
        while (await reader.ReadAsync())
        {
            var trip = new TripDetailsGetDto();
            {
                trip.IdTrip = (int)reader["IdTrip"];
                trip.Name = (string)reader["Name"];
                trip.Description = (string)reader["Description"];
                trip.DateFrom = (DateTime)reader["DateFrom"];
                trip.DateTo = (DateTime)reader["DateTo"];
                trip.Country=(string)reader["CountryName"];
            }
            trips.Add(trip);
        }
        return trips;
    }

    public async Task<IEnumerable<TripsDetailsGetByIdDto>> GetTripsDetailsByIDAsync(int id)
    {
        await using var connection = await GetConnectionAsync();
        var sql = """
                  Select t.Name,t.Description,t.DateFrom,t.DateTo,ct.RegisteredAt,ct.PaymentDate from Trip t
                  join Client_Trip ct on ct.IdTrip = t.IdTrip
                  join Client c on c.IdClient = ct.IdClient
                  Where c.IdClient = @id
                  """;
        
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();
        var trips = new List<TripsDetailsGetByIdDto>();
        if (!await reader.ReadAsync())
        {
            throw new NotFoundException($"Client with id: {id} does not exist");
        }

        while (await reader.ReadAsync())
        {
            var trip = new TripsDetailsGetByIdDto()
            {
                Name = (string)reader["Name"],
                Description = (string)reader["Description"],
                DateFrom = (DateTime)reader["DateFrom"],
                DateTo = (DateTime)reader["DateTo"],
                RegisteredAt = (int)reader["RegisteredAt"],
                PaymentDate = reader["PaymentDate"] == DBNull.Value ? (int?)null : (int)reader["PaymentDate"]
            };
            trips.Add(trip);
        }
        if (!trips.Any())
        {
            throw new NotFoundException($"Klient o ID {id} nie jest zapisany na żadną wycieczkę.");
        }
        return trips;
    }

    public async Task<int> CreateClientAsync(CreateClientDto body)
    {
        await using var connection = await GetConnectionAsync();
        var sql = """
                  Insert into Client (FirstName, LastName, Email, Telephone, Pesel) VALUES (@FirstName, @LastName, @email, @telephone, @pesel);
                  Select scope_identity()
                  """;
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", body.FirstName);
        command.Parameters.AddWithValue("@LastName", body.LastName);
        command.Parameters.AddWithValue("@email", body.email);
        command.Parameters.AddWithValue("@telephone", body.telephone);
        command.Parameters.AddWithValue("@pesel", body.pesel);
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return id;
    }

    public async Task AddClientToTripAsync(int IdClient, int IdTrip)
    {
        await using var connection = await GetConnectionAsync();
        var clientExist = """
                            Select 1 From Client Where IdClient = @IdClient
                          """;
        await using var command = new SqlCommand(clientExist, connection);
        command.Parameters.AddWithValue("@IdClient", IdClient);
        if (await command.ExecuteScalarAsync() == DBNull.Value)
        {
            throw new NotFoundException($"Client with id: {IdClient} does not exist");
        }

        var tripExists = """
                         Select MaxPeople From Trip Where IdTrip = @IdTrip
                         """;
        await using var command2 = new SqlCommand(tripExists, connection);
        command2.Parameters.AddWithValue("@IdTrip", IdTrip);
        var maxPeople = await command2.ExecuteScalarAsync();
        if (maxPeople == DBNull.Value)
        {
            throw new NotFoundException($"Trip with id: {IdTrip} does not exist");
        }
        int Max=Convert.ToInt32(maxPeople);
        var countCmd = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @Idtrip", connection);
        countCmd.Parameters.AddWithValue("@IdTrip", IdTrip);
        int registeredCount = (int)await countCmd.ExecuteScalarAsync();
        if (Max < registeredCount)
        {
            throw new NotFoundException($"No more space in this trip");
        }
        
        var checkDupCmd = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @clientId AND IdTrip = @tripId", connection);
        checkDupCmd.Parameters.AddWithValue("@IdClient", IdClient);
        checkDupCmd.Parameters.AddWithValue("@IdTrip", IdTrip);
        var alreadyRegistered = await checkDupCmd.ExecuteScalarAsync();
        if (alreadyRegistered is not null)
            throw new InvalidOperationException("Klient już jest zapisany na tę wycieczkę.");
        
        
        var now = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
        await using var command3 = new SqlCommand("Insert into Client_Trip (IdClient, IdTrip,RegisteredAt) Values (@IdClient, @IdTrip,@now)", connection);
        command3.Parameters.AddWithValue("@IdClient", IdClient);
        command3.Parameters.AddWithValue("@IdTrip", IdTrip);
        command3.Parameters.AddWithValue("@now", now);
        await command3.ExecuteNonQueryAsync();
    }

    
    public async Task DeleteClientAsync(int IdClient, int IdTrip)
    {
        await using var connection = await GetConnectionAsync();
        await using var check=new SqlCommand("Select COUNT(1) From Client_Trip Where IdClient =@IdClient And IdTrip=@IdTrip", connection);
        check.Parameters.AddWithValue("@IdClient", IdClient);
        check.Parameters.AddWithValue("@IdTrip", IdTrip);
        if (await check.ExecuteScalarAsync() == DBNull.Value)
        {
            throw new NotFoundException($"Client with id: {IdClient} and trip{IdTrip} does not exist");
        }
        
        var sql = """
                    Delete From Client_Trip Where IdClient = @IdClient And IdTrip = @IdTrip
                  """;
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdClient", IdClient);
        command.Parameters.AddWithValue("@IdTrip", IdTrip);
        await command.ExecuteNonQueryAsync();
    }
}