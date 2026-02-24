using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BitbucketPipelinesShield;

/// <summary>
/// HTTP-triggered Azure Functions for Bitbucket Pipelines badge (root, status, url).
/// Routes are preserved for backward compatibility with external consumers.
/// </summary>
/// <param name="httpClient">The HTTP client (configured with no-cache).</param>
/// <param name="logger">The logger.</param>
public sealed class BadgeFunctions(HttpClient httpClient, ILogger<BadgeFunctions> logger)
{
    /// <summary>
    /// Serves the default Bitbucket Pipelines shield SVG at GET /.
    /// </summary>
    [Function(nameof(Root))]
    public IActionResult Root(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "")] HttpRequest req)
    {
        return Shields.BitbucketPipelinesShieldResponse();
    }

    /// <summary>
    /// Returns build status badge SVG at GET /status/{owner}/{repo}/{node}.
    /// </summary>
    [Function(nameof(GetBuildStatus))]
    public async Task<IActionResult> GetBuildStatus(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status/{owner}/{repo}/{node}")] HttpRequest req,
        string owner,
        string repo,
        string node)
    {
        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo) || string.IsNullOrEmpty(node))
        {
            return Shields.BuildUnknownResponse();
        }

        try
        {
            return await BitbucketStatusService.GetBuildResponseAsync(httpClient, owner, repo, node, returnStatus: true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "GetBuildStatus failed for {Owner}/{Repo}/{Node}", owner, repo, node);
            return Shields.BuildUnresponsiveResponse();
        }
    }

    /// <summary>
    /// Redirects to the pipeline build (or pipelines home) at GET /url/{owner}/{repo}/{node}.
    /// </summary>
    [Function(nameof(GetBuildUrl))]
    public async Task<IActionResult> GetBuildUrl(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "url/{owner}/{repo}/{node}")] HttpRequest req,
        string owner,
        string repo,
        string node)
    {
        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo) || string.IsNullOrEmpty(node))
        {
            return new RedirectResult("https://bitbucket.org/");
        }

        try
        {
            return await BitbucketStatusService.GetBuildResponseAsync(httpClient, owner, repo, node, returnStatus: false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "GetBuildUrl failed for {Owner}/{Repo}/{Node}", owner, repo, node);
            return new RedirectResult($"https://bitbucket.org/{owner}/{repo}/addon/pipelines/home#!/results/?error=1");
        }
    }
}
