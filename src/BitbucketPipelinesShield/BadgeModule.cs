using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses;

namespace BitbucketPipelinesShield
{
    public class BadgeModule : NancyModule
    {
        public BadgeModule()
        {
            Get("/", _=> Shields.BitbucketPipelinesShieldResponse());
            Get("/status/{owner}/{repo}/{node}", async _ => await GetBuildStatus(_.owner, _.repo, _.node));
            Get("/url/{owner}/{repo}/{node}", async _ => await GetBuildUrl(_.owner, _.repo, _.node));
        }

        private static readonly ConcurrentDictionary<Tuple<string, string, string>, string> StatusApiUrls = new ConcurrentDictionary<Tuple<string, string, string>, string>();
        private static readonly ConcurrentDictionary<Tuple<string, string, string>, BadgeCacheItem> StatusCache = new ConcurrentDictionary<Tuple<string, string, string>, BadgeCacheItem>();

        private static async Task<Response> GetBuildStatus(string owner, string repo, string node)
        {
            try
            {
                return await GetBuildResponse(Tuple.Create(owner, repo, node), true);
            }
            catch (Exception)
            {
                return Shields.BuildUnresponsiveResponse();
            }
        }

        private static async Task<Response> GetBuildUrl(string owner, string repo, string node)
        {
            try
            {
                return await GetBuildResponse(Tuple.Create(owner, repo, node), false);
            }
            catch (Exception)
            {
                return new RedirectResponse($"https://bitbucket.org/{owner}/{repo}/addon/pipelines/home#!/results/?error=1");
            }
        }

        private static async Task<Response> GetBuildResponse(Tuple<string, string, string> cacheKey, bool status)
        {
            BadgeCacheItem cacheItem;
            if (StatusCache.TryGetValue(cacheKey, out cacheItem) && cacheItem.Expires > DateTime.UtcNow.Ticks)
            {
                return (status ? cacheItem.Status : cacheItem.Url)();
            }
            var bitbucketStatus = await GetBitbucketStatus(cacheKey);
            var parsedStats = Newtonsoft.Json.Linq.JObject.Parse(bitbucketStatus);
            var build = parsedStats
                .SelectTokens("$.values[?(@.type=='build')]")
                .Where(token => token.Value<string>("url")?.Contains("/pipelines/") ?? false)
                .Select(token => new {State = token.Value<string>("state"), Url = token.Value<string>("url")})
                .FirstOrDefault();

            cacheItem = new BadgeCacheItem(
                GetStatusResponse(build?.State),
                () => new RedirectResponse(build?.Url ?? $"https://bitbucket.org/{cacheKey.Item1}/{cacheKey.Item2}/addon/pipelines/home#!/results/")
                );

            StatusCache.AddOrUpdate(cacheKey, cacheItem, (key, old) => cacheItem);

            return (status ? cacheItem.Status : cacheItem.Url)();
        }

        private static Func<Response> GetStatusResponse(string state)
        {
            Func<Response> statusResponse;
            switch (state)
            {
                case "SUCCESSFUL":
                {
                    statusResponse = Shields.BuildPassingResponse;
                    break;
                }
                case "FAILED":
                {
                    statusResponse = Shields.BuildFailingResponse;
                    break;
                }
                case "INPROGRESS":
                {
                    statusResponse = Shields.BuildRunningResponse;
                    break;
                }
                default:
                {
                    statusResponse = Shields.BuildUnknownResponse;
                    break;
                }
            }
            return statusResponse;
        }

        private static async Task<string> GetBitbucketStatus(Tuple<string, string, string> cacheKey)
        {
            string bitbucketStatus;
            using (
                var client = new HttpClient
                {
                    DefaultRequestHeaders = {CacheControl = new CacheControlHeaderValue {NoCache = true}}
                })
            {
                bitbucketStatus = await client.GetStringAsync(
                    StatusApiUrls.GetOrAdd(
                        cacheKey,
                        key => $"https://api.bitbucket.org/2.0/repositories/{key.Item1}/{key.Item2}/commit/{key.Item3}/statuses"
                        )
                    );
            }
            return bitbucketStatus;
        }
    }
}
