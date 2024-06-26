namespace EltraMauiCommon.Plugins
{
    public class EltraPluginCacheItem
    {
        public string Name { get; set; }

        public IEltraNavigoPluginService PluginService { get; set; }

        public string PayloadId { get; set; }

        public string FullPath { get; set; }

        public string HashCode { get; set; }

        public string Version { get; set; }
    }
}