using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace CsvProcessing.Api.Authentication;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiKeyValidator _validator;
    private readonly IOptionsMonitor<ApiKeyOptions> _options;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private static readonly JsonSerializerOptions ProblemJsonOptions = new(JsonSerializerDefaults.Web);

    public ApiKeyMiddleware(
        RequestDelegate next,
        IApiKeyValidator validator,
        IOptionsMonitor<ApiKeyOptions> options,
        ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _validator = validator;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await _next(context);
            return;
        }

        if (endpoint is null)
        {
            await _next(context);
            return;
        }

        var headerName = _options.CurrentValue.HeaderName;

        if (!context.Request.Headers.TryGetValue(headerName, out var presented) ||
            !_validator.TryValidate(presented.ToString(), out var matchedKey))
        {
            _logger.LogWarning(
                "Rejected unauthenticated request {Method} {Path} from {RemoteIp}: missing or invalid {HeaderName} header.",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress,
                headerName);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                context.Response.Headers.WWWAuthenticate =
                    $"ApiKey realm=\"CsvProcessing\", header=\"{headerName}\"";

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = $"A valid API key must be supplied in the '{headerName}' request header.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                    Instance = context.Request.Path
                };

                problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(problem, ProblemJsonOptions),
                    context.RequestAborted);
            }

            return;
        }

        context.SetApiKeyOwner(matchedKey.Owner);

        using (_logger.BeginScope(new Dictionary<string, object?> { ["ApiKeyOwner"] = matchedKey.Owner }))
        {
            _logger.LogDebug(
                "Authenticated request {Method} {Path} for owner {ApiKeyOwner}.",
                context.Request.Method,
                context.Request.Path,
                matchedKey.Owner);

            await _next(context);
        }
    }
}
