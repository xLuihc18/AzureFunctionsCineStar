using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Data;

namespace CinestarFunctions;

public class GetCineDetalle
{
    private readonly ILogger<GetCineDetalle> _logger;

    public GetCineDetalle(ILogger<GetCineDetalle> logger)
    {
        _logger = logger;
    }

    [Function("GetCineDetalle")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        string id = req.Query["id"];
        if (string.IsNullOrEmpty(id)) return new BadRequestObjectResult("Debes enviar un id de cine. Ejemplo: ?id=1");

        _logger.LogInformation($"Consultando detalle y tarifas del cine: {id}");

        string connectionString = Environment.GetEnvironmentVariable("Cn");

        try
        {
            using var db = new SqlConnection(connectionString);

            var info = await db.QueryFirstOrDefaultAsync("EXEC sp_getCine @_id", new { _id = id });
            var tarifas = await db.QueryAsync("EXEC sp_getCineTarifas @_id", new { _id = id });

            if (info == null) return new NotFoundObjectResult("Cine no encontrado.");

            return new OkObjectResult(new { cine = info, precios = tarifas });
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            return new BadRequestObjectResult("Error al conectar con la base de datos.");
        }
    }
}