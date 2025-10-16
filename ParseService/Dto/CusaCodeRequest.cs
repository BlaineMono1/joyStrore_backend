using System.Text.Json.Serialization;

namespace ParseService.Dto;

public class CusaCodeRequest
{
    [JsonPropertyName("cusaCodeUa")]
    public string сusaCodeUa { get; set; }

    [JsonPropertyName("cusaCodeTr")]
    public string сusaCodeTr { get; set; }
}
