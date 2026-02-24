# AI Provider Configuration Guide

## 🤖 Supported AI Providers

Infinite Tavern now supports multiple AI providers:
- **OpenAI GPT-4o mini** (default) - Fast, cost-effective
- **Anthropic Claude Sonnet 3.5** - More creative narratives

## 🔄 How to Switch Between Providers

### Using OpenAI (GPT-4o mini) - DEFAULT

**1. Edit `appsettings.json`:**
```json
{
  "AI": {
    "Provider": "OpenAI"
  },
  "OpenAI": {
    "ApiKey": "sk-proj-your-openai-key-here"
  }
}
```

**2. Get API Key:**
- Visit: https://platform.openai.com/api-keys
- Create new secret key
- Copy to appsettings.json

**3. Pricing:**
- Input: $0.150 per 1M tokens
- Output: $0.600 per 1M tokens
- Very cost-effective for MVP

### Using Claude (Sonnet 3.5)

**1. Edit `appsettings.json`:**
```json
{
  "AI": {
    "Provider": "Claude"
  },
  "Anthropic": {
    "ApiKey": "sk-ant-api03-your-claude-key-here"
  }
}
```

**2. Get API Key:**
- Visit: https://console.anthropic.com/
- Create API key
- Copy to appsettings.json

**3. Pricing:**
- Input: $3.00 per 1M tokens
- Output: $15.00 per 1M tokens
- Higher quality, more expensive

## ⚙️ Configuration Options

### Full Configuration Example

```json
{
  "AI": {
    "Provider": "OpenAI",
    "_Comment": "Valid options: 'OpenAI' or 'Claude'"
  },
  "OpenAI": {
    "ApiKey": "sk-proj-..."
  },
  "Anthropic": {
    "ApiKey": "sk-ant-api03-..."
  }
}
```

### Environment Variables (Production)

Instead of storing keys in appsettings.json, use environment variables:

**PowerShell:**
```powershell
$env:AI__Provider = "OpenAI"
$env:OpenAI__ApiKey = "sk-proj-..."
```

**Linux/macOS:**
```bash
export AI__Provider="OpenAI"
export OpenAI__ApiKey="sk-proj-..."
```

### User Secrets (Development)

```powershell
cd src/InfiniteTavern.API

# Store OpenAI key
dotnet user-secrets set "OpenAI:ApiKey" "sk-proj-your-key"

# Store Claude key
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-your-key"

# Set provider
dotnet user-secrets set "AI:Provider" "OpenAI"
```

## 🎭 Provider Comparison

### OpenAI GPT-4o mini

**Pros:**
- ✅ Very fast responses
- ✅ Cost-effective (~5x cheaper)
- ✅ Good at following JSON format
- ✅ Reliable structured output
- ✅ Better availability

**Cons:**
- ⚠️ Less creative narratives
- ⚠️ Shorter context understanding
- ⚠️ May be more repetitive

**Best for:**
- MVP testing
- High-volume usage
- Budget-conscious projects
- Quick prototyping

### Claude Sonnet 3.5

**Pros:**
- ✅ More creative storytelling
- ✅ Better narrative quality
- ✅ Excellent at roleplay
- ✅ Better long-term memory
- ✅ More immersive descriptions

**Cons:**
- ⚠️ More expensive
- ⚠️ Slightly slower
- ⚠️ May need stricter prompts
- ⚠️ Rate limits may be tighter

**Best for:**
- Production quality
- Premium experiences
- Story-rich campaigns
- When narrative quality matters

## 🧪 Testing Both Providers

### Quick Test Script

```powershell
# Test with OpenAI
$body = @{
    playerName = "Tester"
    characterName = "Test Hero"
    race = "Human"
    class = "Warrior"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/game/new-game" `
    -Method Post -Body $body -ContentType "application/json"

Write-Host "Provider in use: Check console output"
Write-Host "Response quality: $($response.message)"
```

### A/B Testing

Create two configuration files:

**appsettings.OpenAI.json:**
```json
{
  "AI": {
    "Provider": "OpenAI"
  }
}
```

**appsettings.Claude.json:**
```json
{
  "AI": {
    "Provider": "Claude"
  }
}
```

Run with specific config:
```powershell
dotnet run --project src/InfiniteTavern.API -- --environment OpenAI
dotnet run --project src/InfiniteTavern.API -- --environment Claude
```

## 🏗️ Architecture

### Interface-Based Design

```csharp
IAIService
    ├── ClaudeService (Anthropic)
    └── OpenAIService (GPT-4o mini)
```

Both services implement the same interface:
```csharp
public interface IAIService
{
    Task<ClaudeResponse> GenerateResponseAsync(
        string systemPrompt, 
        string userPrompt
    );
}
```

### Adding More Providers

To add a new provider (e.g., Google Gemini):

**1. Create service:**
```csharp
public class GeminiService : IAIService
{
    public async Task<ClaudeResponse> GenerateResponseAsync(
        string systemPrompt, 
        string userPrompt)
    {
        // Implement Gemini API call
        // Parse response to ClaudeResponse format
    }
}
```

**2. Register in Program.cs:**
```csharp
else if (aiProvider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IAIService, GeminiService>();
}
```

**3. Add configuration:**
```json
{
  "Gemini": {
    "ApiKey": "your-gemini-key"
  }
}
```

## 🔍 Troubleshooting

### Provider Not Switching

**Check console output on startup:**
```
✓ Using OpenAI Service (GPT-4o mini)
```

**Verify configuration:**
```powershell
# View current configuration
dotnet run --project src/InfiniteTavern.API -- --help
```

### API Key Not Working

**OpenAI:**
- Verify key format: `sk-proj-...`
- Check billing: https://platform.openai.com/account/billing
- Verify organization access

**Claude:**
- Verify key format: `sk-ant-api03-...`
- Check usage limits: https://console.anthropic.com/
- Wait for API access approval

### Invalid Provider Error

```
Unknown AI provider: XYZ. Valid options: Claude, OpenAI
```

Make sure `AI:Provider` is exactly: `"OpenAI"` or `"Claude"` (case-insensitive)

## 📊 Cost Estimation

### Example Session (10 turns)

**Assumptions:**
- System prompt: ~500 tokens
- User prompt per turn: ~300 tokens
- AI response per turn: ~400 tokens
- Total per turn: ~1200 tokens

**10 turns = 12,000 tokens**

**OpenAI GPT-4o mini:**
- Input: 8,000 tokens × $0.15 / 1M = $0.0012
- Output: 4,000 tokens × $0.60 / 1M = $0.0024
- **Total: ~$0.0036 per session**

**Claude Sonnet 3.5:**
- Input: 8,000 tokens × $3.00 / 1M = $0.024
- Output: 4,000 tokens × $15.00 / 1M = $0.060
- **Total: ~$0.084 per session**

**Claude is ~23x more expensive for this use case**

## 🎯 Recommendations

### For Development & Testing
→ Use **OpenAI GPT-4o mini**
- Faster iteration
- Lower costs
- Good enough quality

### For Production (Budget)
→ Use **OpenAI GPT-4o mini**
- Scalable costs
- Reliable performance
- Fast responses

### For Production (Premium)
→ Use **Claude Sonnet 3.5**
- Best narrative quality
- Worth it for story-rich games
- Premium user experience

### Hybrid Approach
- Use GPT-4o mini for combat/mechanics
- Use Claude for important story moments
- Switch based on context (requires code changes)

## 🔐 Security Best Practices

1. **Never commit API keys to git**
2. **Use environment variables in production**
3. **Rotate keys regularly**
4. **Monitor usage and set billing alerts**
5. **Use user secrets in development**

## ✅ Quick Checklist

- [ ] Decided which provider to use
- [ ] Got API key from provider
- [ ] Updated `appsettings.json` OR set environment variables
- [ ] Verified provider selection in console output
- [ ] Tested with sample game session
- [ ] Monitored costs and usage
- [ ] Stored keys securely (not in git)

---

**Current Default:** OpenAI GPT-4o mini (cost-effective for MVP)

**Switch anytime:** Just change `AI:Provider` in config and restart!
