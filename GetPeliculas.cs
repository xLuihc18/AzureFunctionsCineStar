using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Data;

namespace CinestarFunctions;

public class GetPeliculas
{
    private readonly ILogger<GetPeliculas> _logger;

    public GetPeliculas(ILogger<GetPeliculas> logger)
    {
        _logger = logger;
    }

    [Function("GetPeliculas")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        string id = req.Query["id"];
        if (string.IsNullOrEmpty(id)) id = "1";

        _logger.LogInformation($"Consultando películas para el tipo: {id}");

        string connectionString = Environment.GetEnvironmentVariable("Cn");

        try
        {
            using var db = new SqlConnection(connectionString);
            var peliculas = await db.QueryAsync("EXEC sp_getPeliculas @_id", new { _id = id });

            return new OkObjectResult(peliculas);
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            return new BadRequestObjectResult("Error al obtener películas.");
        }
    }
}