namespace Cms.Admin.Middleware;

public sealed class DemoApiGatewayMiddleware
{
    private static readonly HashSet<string> ContentMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH" };

    private readonly RequestDelegate _next;

    public DemoApiGatewayMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("DemoMode:Enabled")
            || (!context.Request.Path.StartsWithSegments("/api")
                && !context.Request.Path.StartsWithSegments("/swagger")))
        {
            await _next(context);
            return;
        }

        var target = new Uri($"http://127.0.0.1:5101{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}");
        using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), target);
        if (ContentMethods.Contains(context.Request.Method))
        {
            request.Content = new StreamContent(context.Request.Body);
        }

        foreach (var header in context.Request.Headers)
        {
            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        request.Headers.Host = "127.0.0.1:5101";
        request.Headers.Remove("Connection");

        var client = httpClientFactory.CreateClient("DemoApiGateway");
        await WaitForApiAsync(client, context.RequestAborted);
        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            context.RequestAborted);
        context.Response.StatusCode = (int)response.StatusCode;

        foreach (var header in response.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in response.Content.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        context.Response.Headers.Remove("transfer-encoding");
        await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
    }

    private static async Task WaitForApiAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                using var response = await client.GetAsync(
                    "http://127.0.0.1:5101/swagger/v1/swagger.json",
                    cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException) when (attempt < 50)
            {
            }

            if (attempt >= 50)
            {
                throw new HttpRequestException("The internal CMS API did not become ready.");
            }

            await Task.Delay(100, cancellationToken);
        }
    }
}
