using System.Text.Json.Serialization;

namespace GridApi.Models
{
    // Response DTOs for Grid - uses AjaxStore pattern (not CrudManager)
    public class ReadResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<Player>? Data { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class CreateRequest
    {
        [JsonPropertyName("data")]
        public List<Player>? Data { get; set; }
    }

    // DTO for updates with nullable fields to support partial updates
    public class PlayerUpdateDto
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(IntOrStringIdConverter))]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("team")]
        public string? Team { get; set; }

        [JsonPropertyName("score")]
        public double? Score { get; set; }

        [JsonPropertyName("percentageWins")]
        public double? PercentageWins { get; set; }
    }

    public class UpdateRequest
    {
        [JsonPropertyName("data")]
        public List<PlayerUpdateDto>? Data { get; set; }
    }

    public class DeleteRequest
    {
        [JsonPropertyName("ids")]
        public List<int>? Ids { get; set; }
    }

    public class DeleteResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
