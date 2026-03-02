using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FormMaker.Api.Functions;

/// <summary>
/// Handles CORS preflight OPTIONS requests
/// </summary>
public class CorsFunction
{
    private readonly ILogger<CorsFunction> _logger;

    public CorsFunction(ILogger<CorsFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles OPTIONS requests for all routes (CORS preflight)
    /// </summary>
    [Function("OptionsHandler")]
    public HttpResponseData HandleOptions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "{*path}")] HttpRequestData req)
    {
        _logger.LogInformation("OPTIONS request received for path: {Path}", req.Url.PathAndQuery);

        var response = req.CreateResponse(HttpStatusCode.OK);

        // Get the origin from the request
        var origin = req.Headers.TryGetValues("Origin", out var origins)
            ? origins.FirstOrDefault()
            : "*";

        // Add CORS headers
        response.Headers.Add("Access-Control-Allow-Origin", origin ?? "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
        response.Headers.Add("Access-Control-Allow-Credentials", "true");
        response.Headers.Add("Access-Control-Max-Age", "86400");

        return response;
    }
}
