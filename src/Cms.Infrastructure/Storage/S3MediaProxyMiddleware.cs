using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cms.Infrastructure.Storage;

/// <summary>
/// Serves uploaded media out of S3 without the bucket being public.
///
/// The obvious way to show an uploaded photograph is to link straight at
/// <c>bucket.s3.region.amazonaws.com/key</c>, but that only works if anyone on the internet may
/// read the bucket. Turning that on is a decision about someone else's AWS account, and it is
/// the one thing a customer is most likely to refuse or get wrong — so the site would ship with
/// broken images through no fault of its own.
///
/// Reading the object here instead keeps the bucket closed: only the credentials in this
/// deployment can reach it, and the browser only ever sees an address on the school's own
/// domain. It also means images survive a bucket that is later locked down again.
///
/// The path is the same <c>/uploads/...</c> that local storage uses, so nothing downstream —
/// stored URLs, templates, editors — has to know which provider is in use.
/// </summary>
public sealed class S3MediaProxyMiddleware
{
    public const string PathPrefix = "/uploads";

    private readonly RequestDelegate _next;
    private readonly string _bucket;
    private readonly int _cacheSeconds;
    private readonly ILogger<S3MediaProxyMiddleware> _logger;

    public S3MediaProxyMiddleware(
        RequestDelegate next,
        IOptions<AwsOptions> options,
        ILogger<S3MediaProxyMiddleware> logger)
    {
        _next = next;
        _bucket = options.Value.BucketName;
        // Uploaded files are immutable — the name carries a fresh GUID — so they can be cached
        // hard. Without this every page view would fetch every photograph from S3 again.
        _cacheSeconds = 31536000;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAmazonS3 client)
    {
        if (!context.Request.Path.StartsWithSegments(PathPrefix, out var remainder)
            || (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method)))
        {
            await _next(context);
            return;
        }

        var key = Key(remainder.Value);
        if (key is null)
        {
            await _next(context);
            return;
        }

        try
        {
            using var response = await client.GetObjectAsync(
                new GetObjectRequest { BucketName = _bucket, Key = key }, context.RequestAborted);

            context.Response.ContentType = response.Headers.ContentType ?? "application/octet-stream";
            context.Response.Headers.CacheControl = $"public, max-age={_cacheSeconds}, immutable";
            if (!string.IsNullOrEmpty(response.ETag))
            {
                context.Response.Headers.ETag = response.ETag;
            }

            if (HttpMethods.IsHead(context.Request.Method))
            {
                context.Response.ContentLength = response.ContentLength;
                return;
            }

            await response.ResponseStream.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
        catch (AmazonS3Exception ex)
        {
            // A misconfigured bucket or expired key must not read as "this photo does not exist":
            // that sends whoever is looking after the wrong thing entirely.
            _logger.LogError(ex, "Could not read {Key} from bucket {Bucket}", key, _bucket);
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
        }
    }

    /// <summary>
    /// Turns the request path into an object key, refusing anything that could escape the
    /// prefix the application itself writes under.
    /// </summary>
    public static string? Key(string? remainder)
    {
        var trimmed = remainder?.Trim('/');
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 1024)
        {
            return null;
        }

        return trimmed.Contains("..", StringComparison.Ordinal)
            || trimmed.Contains("//", StringComparison.Ordinal)
            || trimmed.Contains('\\', StringComparison.Ordinal)
            ? null
            : trimmed;
    }
}
