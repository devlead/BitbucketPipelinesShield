using Microsoft.AspNetCore.Mvc;

namespace BitbucketPipelinesShield;

/// <summary>
/// Cached badge result for a Bitbucket Pipelines build (status SVG + redirect URL).
/// </summary>
public sealed class BadgeCacheItem
{
    /// <summary>
    /// Gets the UTC ticks when this cache entry expires.
    /// </summary>
    public long Expires { get; } = DateTime.UtcNow.AddSeconds(45).Ticks;

    /// <summary>
    /// Gets the status badge SVG result (passing, failing, running, unknown).
    /// </summary>
    public IActionResult Status { get; }

    /// <summary>
    /// Gets the redirect URL to the pipeline build (or pipelines home).
    /// </summary>
    public string RedirectUrl { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadgeCacheItem"/> class.
    /// </summary>
    /// <param name="status">The status badge result.</param>
    /// <param name="redirectUrl">The URL to redirect to for build details.</param>
    public BadgeCacheItem(IActionResult status, string redirectUrl)
    {
        Status = status;
        RedirectUrl = redirectUrl;
    }
}
