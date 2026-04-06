using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Data;

namespace CinestarFunctions;

public class GetCines
{
    private readonly ILogger<GetCines> _logger;

    public GetCines(ILogger<GetCines> logger)
    {
        _logger = logger;
    }

    [Function("GetCines")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("Consultando la lista completa de cines.");

        string connectionString = Environment.GetEnvironmentVariable("Cn");

        try
        {
            using var db = new SqlConnection(connectionString);

            var cines = await db.QueryAsync("EXEC sp_getCines");

            return new OkObjectResult(cines);
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"Error en GetCines: {ex.Message}");
            return new BadRequestObjectResult("Error al conectar con la base de datos.");
        }
    }
}