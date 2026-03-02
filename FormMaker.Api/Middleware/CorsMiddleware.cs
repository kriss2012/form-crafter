using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace FormMaker.Api.Middleware;

/// <summary>
/// Middleware to add CORS headers to all HTTP responses
/// </summary>
public class CorsMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<CorsMiddleware> _logger;

    public CorsMiddleware(ILogger<CorsMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        _logger.LogInformation("CORS Middleware: Processing request");

        // Get the HTTP request data
        var requestData = await context.GetHttpRequestDataAsync();

        if (requestData != null)
        {
            _logger.LogInformation("CORS Middleware: Request method = {Method}, Path = {Path}",
                requestData.Method, requestData.Url.PathAndQuery);

            // Handle OPTIONS preflight requests immediately
            if (requestData.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("CORS Middleware: Handling OPTIONS preflight request");
                var preflightResponse = requestData.CreateResponse(System.Net.HttpStatusCode.OK);
                AddCorsHeaders(preflightResponse, requestData);
                context.GetInvocationResult().Value = preflightResponse;
                return;
            }
        }

        // Continue with the function execution
        await next(context);

        // Add CORS headers to the response
        if (requestData != null)
        {
            var invocationResult = context.GetInvocationResult();

            if (invocationResult?.Value is HttpResponseData responseData)
            {
                _logger.LogInformation("CORS Middleware: Adding CORS headers to response");
                AddCorsHeaders(responseData, requestData);
            }
            else
            {
                _logger.LogWarning("CORS Middleware: No HttpResponseData found in invocation result");
            }
        }
    }

    private void AddCorsHeaders(HttpResponseData response, HttpRequestData request)
    {
        try
        {
            // List of allowed origins
            var allowedOrigins = new[]
            {
                "https://zyonify.github.io",
                "http://localhost:5000",
                "http://localhost:5001"
            };

            // Get the origin from the request
            var requestOrigin = request.Headers.TryGetValues("Origin", out var origins)
                ? origins.FirstOrDefault()
                : null;

            _logger.LogInformation("CORS Middleware: Request origin = {Origin}", requestOrigin ?? "none");

            // Determine which origin to allow
            string allowedOrigin;
            bool allowCredentials = false;

            if (!string.IsNullOrEmpty(requestOrigin) && allowedOrigins.Contains(requestOrigin, StringComparer.OrdinalIgnoreCase))
            {
                // Request origin is in allowed list - use it
                allowedOrigin = requestOrigin;
                allowCredentials = true;
                _logger.LogInformation("CORS Middleware: Origin {Origin} is allowed", requestOrigin);
            }
            else if (string.IsNullOrEmpty(requestOrigin))
            {
                // No origin header - allow all
                allowedOrigin = "*";
                _logger.LogInformation("CORS Middleware: No origin header, using wildcard");
            }
            else
            {
                // Origin not in allowed list - still add header but log warning
                allowedOrigin = requestOrigin;
                _logger.LogWarning("CORS Middleware: Origin {Origin} not in allowed list, but allowing anyway", requestOrigin);
            }

            // Add CORS headers - try/catch each one in case headers already exist
            TryAddHeader(response, "Access-Control-Allow-Origin", allowedOrigin);
            TryAddHeader(response, "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            TryAddHeader(response, "Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");

            // Only add credentials header if not using wildcard origin
            if (allowCredentials)
            {
                TryAddHeader(response, "Access-Control-Allow-Credentials", "true");
            }

            TryAddHeader(response, "Access-Control-Max-Age", "86400");

            _logger.LogInformation("CORS Middleware: Successfully added CORS headers");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CORS Middleware: Error adding CORS headers");
        }
    }

    private void TryAddHeader(HttpResponseData response, string name, string value)
    {
        try
        {
            if (!response.Headers.Contains(name))
            {
                response.Headers.Add(name, value);
                _logger.LogDebug("CORS Middleware: Added header {Name} = {Value}", name, value);
            }
            else
            {
                _logger.LogDebug("CORS Middleware: Header {Name} already exists", name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CORS Middleware: Failed to add header {Name}", name);
        }
    }
}
