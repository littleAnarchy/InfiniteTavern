using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InfiniteTavern.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ThirdParty.Json.LitJson;

namespace InfiniteTavern.Application.Services;

public class OpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIService> _logger;
    private readonly string _apiKey;
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
    }

    public async Task<AIResponse> GenerateResponseAsync(string systemPrompt, string userPrompt, bool useJsonFormat = true)
    {
        try
        {
            object requestBody;
            
            if (useJsonFormat)
            {
                requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.7,
                    max_tokens = 2048,
                    response_format = new { type = "json_object" }
                };
            }
            else
            {
                requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.7,
                    max_tokens = 2048
                };
            }

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(ApiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var openAiResponse = JsonSerializer.Deserialize<OpenAIApiResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (openAiResponse?.Choices == null || openAiResponse.Choices.Count == 0)
            {
                throw new InvalidOperationException("OpenAI API returned empty response");
            }

            var responseText = openAiResponse.Choices[0].Message.Content;

            // Extract token usage
            var tokenUsage = new TokenUsage
            {
                InputTokens = openAiResponse.Usage?.PromptTokens ?? 0,
                OutputTokens = openAiResponse.Usage?.CompletionTokens ?? 0,
                TotalTokens = openAiResponse.Usage?.TotalTokens ?? 0,
                ModelName = openAiResponse.Model ?? "gpt-4o-mini"
            };

            // If not using JSON format, return plain text as narrative
            if (!useJsonFormat)
            {
                return new AIResponse
                {
                    Narrative = responseText,
                    Events = new List<GameEvent>(),
                    NewNpcs = new List<NewNpc>(),
                    QuestUpdates = new List<QuestUpdate>(),
                    SkillChecks = new List<SkillCheck>(),
                    Usage = tokenUsage
                };
            }

            // Extract JSON from markdown code blocks if present
            responseText = ExtractJsonFromMarkdown(responseText);

            var gameResponse = JsonSerializer.Deserialize<AIResponse>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (gameResponse == null)
            {
                throw new InvalidOperationException("Failed to parse OpenAI response");
            }

            gameResponse.Usage = tokenUsage;
            return gameResponse;
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI response as JSON");

            // Return a default response on parse failure
            return new AIResponse
            {
                Narrative = "The dungeon master seems confused. Nothing happens.",
                Events = new List<GameEvent>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
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

    private class OpenAIApiResponse
    {
        public List<Choice> Choices { get; set; } = new();
        public UsageInfo? Usage { get; set; }
        public string? Model { get; set; }
    }

    private class Choice
    {
        public Message Message { get; set; } = new();
    }

    private class Message
    {
        public string Content { get; set; } = string.Empty;
    }

    private class UsageInfo
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
