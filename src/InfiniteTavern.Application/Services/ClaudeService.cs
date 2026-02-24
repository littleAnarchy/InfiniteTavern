using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using InfiniteTavern.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InfiniteTavern.Application.Services;

public class ClaudeService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClaudeService> _logger;
    private readonly string _apiKey;
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    public ClaudeService(HttpClient httpClient, IConfiguration configuration, ILogger<ClaudeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Anthropic:ApiKey"] ?? throw new InvalidOperationException("Anthropic API key not configured");
    }

    public async Task<AIResponse> GenerateResponseAsync(string systemPrompt, string userPrompt)
    {
        try
        {
            var requestBody = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 2048,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.PostAsync(ApiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var claudeApiResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseContent);

            if (claudeApiResponse?.Content == null || claudeApiResponse.Content.Count == 0)
            {
                throw new InvalidOperationException("Claude API returned empty response");
            }

            var responseText = claudeApiResponse.Content[0].Text;

            // Extract JSON from markdown code blocks if present
            responseText = ExtractJsonFromMarkdown(responseText);

            var gameResponse = JsonSerializer.Deserialize<AIResponse>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return gameResponse ?? throw new InvalidOperationException("Failed to parse Claude response");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Claude response as JSON");

            // Return a default response on parse failure
            return new AIResponse
            {
                Narrative = "The dungeon master seems confused. Nothing happens.",
                Events = new List<GameEvent>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude API");
            throw;
        }
    }

    private string ExtractJsonFromMarkdown(string text)
    {
        // Remove markdown code blocks if present
        text = text.Trim();

        if (text.StartsWith("```json"))
        {
            text = text.Substring(7);
        }
        else if (text.StartsWith("```"))
        {
            text = text.Substring(3);
        }

        if (text.EndsWith("```"))
        {
            text = text.Substring(0, text.Length - 3);
        }

        return text.Trim();
    }

    private class ClaudeApiResponse
    {
        public List<ContentItem> Content { get; set; } = new();
    }

    private class ContentItem
    {
        public string Text { get; set; } = string.Empty;
    }
}
