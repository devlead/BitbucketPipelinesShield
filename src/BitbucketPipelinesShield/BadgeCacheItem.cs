using System;
using Nancy;

namespace BitbucketPipelinesShield
{
    public class BadgeCacheItem
    {
        public long Expires { get; } = DateTime.UtcNow.AddSeconds(45).Ticks;
        public Response Status { get; }
        public Response Url { get; }

        public BadgeCacheItem(Response status, Response url)
        {
            Status = status;
            Url = url;
        }
    }
}