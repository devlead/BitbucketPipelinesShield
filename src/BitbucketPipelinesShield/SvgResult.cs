using Microsoft.AspNetCore.Mvc;

namespace BitbucketPipelinesShield;

/// <summary>
/// An action result that returns SVG content with no-cache headers for badge responses.
/// </summary>
public sealed class SvgResult : FileContentResult
{
    private const string SvgContentType = "image/svg+xml";

    /// <summary>
    /// Initializes a new instance of the <see cref="SvgResult"/> class.
    /// </summary>
    /// <param name="contents">The SVG file contents.</param>
    /// <param name="fileDownloadName">Optional filename for Content-Disposition.</param>
    public SvgResult(byte[] contents, string? fileDownloadName = null)
        : base(contents, SvgContentType)
    {
        if (!string.IsNullOrEmpty(fileDownloadName))
        {
            FileDownloadName = fileDownloadName;
        }
    }

    /// <inheritdoc />
    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.Headers.Pragma = "no-cache";
        response.Headers.CacheControl = "no-store, no-cache, must-revalidate, pre-check=0, post-check=0, max-age=0";
        response.Headers.Expires = "-1";
        await base.ExecuteResultAsync(context);
    }
}
