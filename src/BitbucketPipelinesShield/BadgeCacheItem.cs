using System;
using Nancy;

namespace BitbucketPipelinesShield
{
    public class BadgeCacheItem
    {
        public long Expires { get; } = DateTime.UtcNow.AddSeconds(45).Ticks;
        public Func<Response> Status { get; }
        public Func<Response> Url { get; }

        public BadgeCacheItem(Func<Response> status, Func<Response> url)
        {
            Status = status;
            Url = url;
        }
    }
}