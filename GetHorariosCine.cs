using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Data;

namespace CinestarFunctions;

public class GetHorariosCine
{
    private readonly ILogger<GetHorariosCine> _logger;

    public GetHorariosCine(ILogger<GetHorariosCine> logger)
    {
        _logger = logger;
    }

    [Function("GetHorariosCine")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        string idCine = req.Query["id"];

        if (string.IsNullOrEmpty(idCine))
        {
            return new BadRequestObjectResult(new { mensaje = "Poner el ID del cine en el link. Ejemplo: ?id=1" });
        }

        string connectionString = Environment.GetEnvironmentVariable("Cn");

        try
        {
            using var db = new SqlConnection(connectionString);
            var horarios = await db.QueryAsync("EXEC sp_getCinePeliculas @_idCine", new { _idCine = idCine });

            if (!horarios.Any())
            {
                return new OkObjectResult(new { mensaje = "Este cine no tiene peliculas programadas por ahora." });
            }

            return new OkObjectResult(horarios);
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            return new BadRequestObjectResult("Error al obtener los horarios.");
        }
    }
}