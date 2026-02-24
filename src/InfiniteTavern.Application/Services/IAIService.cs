using InfiniteTavern.Application.Models;

namespace InfiniteTavern.Application.Services;

/// <summary>
/// Abstraction for AI service providers (Claude, GPT, etc.)
/// </summary>
public interface IAIService
{
    Task<ClaudeResponse> GenerateResponseAsync(string systemPrompt, string userPrompt);
}
