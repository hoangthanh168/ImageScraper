using Newtonsoft.Json;
using WordPressPCL.Models;

namespace ImageScraper
{
    public class CustomPost : Post
    {
        [JsonProperty("fifu_image_url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FifuImageUrl { get; set; }
        [JsonProperty("_inpost_head_script", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public InpostHeadScript InpostHeadScript { get; set; }
    }
    public class InpostHeadScript
    {
        [JsonProperty("synth_header_script")]
        public string SynthHeaderScript { get; set; }
    }
}