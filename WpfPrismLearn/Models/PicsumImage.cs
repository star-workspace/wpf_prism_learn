using System.Text.Json.Serialization;

namespace WpfPrismLearn.Models
{
    public class PicsumImage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = String.Empty;
        [JsonPropertyName("author")]
        public string Author { get; set; } = String.Empty;
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; } = String.Empty;
        [JsonPropertyName("download_url")]
        public string DownloadUrl { get; set; } = String.Empty;
    }
}
