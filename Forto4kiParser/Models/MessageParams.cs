using System.Text.Json.Serialization;

namespace Forto4kiParser.Models
{
    public class MessageParams
    {
        [JsonPropertyName("chat_id")]
        public string ChatId { get; set; }


        [JsonPropertyName("photo")]
        public string Photo { get; set; }


        [JsonPropertyName("caption")]
        public string Description { get; set; }
    }
}
