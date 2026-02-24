using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BitbucketPipelinesShield;

/// <summary>
/// Fetches and parses Bitbucket commit statuses and maps them to badge responses.
/// </summary>
public static class BitbucketStatusService
{
    private static readonly ConcurrentDictionary<(string Owner, string Repo, string Node), BadgeCacheItem> StatusCache = new();

    private static string StatusApiUrl(string owner, string repo, string node) =>
        $"https://api.bitbucket.org/2.0/repositories/{owner}/{repo}/commit/{node}/statuses";

    /// <summary>
    /// Gets the cached or freshly fetched badge result for the given repo and node (branch/commit).
    /// </summary>
    /// <param name="httpClient">HTTP client (no-cache).</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="node">Branch or commit node.</param>
    /// <param name="returnStatus">If true, returns status SVG; if false, returns redirect URL for URL route.</param>
    /// <returns>Status SVG result or redirect result.</returns>
    public static async Task<IActionResult> GetBuildResponseAsync(
        HttpClient httpClient,
        string owner,
        string repo,
        string node,
        bool returnStatus)
    {
        var key = (owner, repo, node);
        if (StatusCache.TryGetValue(key, out var cached) && cached.Expires > DateTime.UtcNow.Ticks)
        {
            return returnStatus ? cached.Status : new RedirectResult(cached.RedirectUrl);
        }

        string bitbucketStatus;
        using (var request = new HttpRequestMessage(HttpMethod.Get, StatusApiUrl(owner, repo, node)))
        {
            request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            bitbucketStatus = await response.Content.ReadAsStringAsync();
        }

        var parsed = JObject.Parse(bitbucketStatus);
        var build = parsed
            .SelectTokens("$.values[?(@.type=='build')]")
            .Where(token => token.Value<string>("url")?.Contains("/pipelines/", StringComparison.Ordinal) ?? false)
            .Select(token => new { State = token.Value<string>("state"), Url = token.Value<string>("url") })
            .FirstOrDefault();

        var statusResult = GetStatusResponse(build?.State);
        var redirectUrl = build?.Url ?? $"https://bitbucket.org/{owner}/{repo}/addon/pipelines/home#!/results/";
        var cacheItem = new BadgeCacheItem(statusResult, redirectUrl);
        StatusCache.AddOrUpdate(key, cacheItem, (_, _) => cacheItem);

        return returnStatus ? cacheItem.Status : new RedirectResult(cacheItem.RedirectUrl);
    }

    private static IActionResult GetStatusResponse(string? state)
    {
        return state switch
        {
            "SUCCESSFUL" => Shields.BuildPassingResponse(),
            "FAILED" => Shields.BuildFailingResponse(),
            "INPROGRESS" => Shields.BuildRunningResponse(),
            _ => Shields.BuildUnknownResponse()
        };
    }
}
