# 🔄 Швидке переключення між AI провайдерами

## Наразі використовується: OpenAI GPT-4o mini

### Як переключитись на Claude

**1. Відкрийте файл конфігурації:**
```
src/InfiniteTavern.API/appsettings.json
```

**2. Змініть провайдера:**
```json
{
  "AI": {
    "Provider": "Claude"
  }
}
```

**3. Переконайтесь що є API ключ Claude:**
```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-api03-ваш-ключ-тут"
  }
}
```

**4. Перезапустіть API:**
```powershell
# Ctrl+C щоб зупинити
dotnet run --project src/InfiniteTavern.API
```

**5. Перевірте в консолі:**
```
✓ Using Claude AI Service (Sonnet 3.5)
```

### Як повернутись на OpenAI

**1. Змініть провайдера назад:**
```json
{
  "AI": {
    "Provider": "OpenAI"
  }
}
```

**2. Перезапустіть API**

## Порівняння

### OpenAI GPT-4o mini (Поточний)
- ✅ **Швидше** (~2-3 сек відповідь)
- ✅ **Дешевше** (~$0.004 за сесію)
- ✅ **Добре для MVP**
- ⚠️ Менш креативні наративи

### Claude Sonnet 3.5
- ✅ **Кращі історії**
- ✅ **Більш креативний**
- ✅ **Кращий roleplay**
- ⚠️ Дорожче (~$0.08 за сесію)
- ⚠️ Трохи повільніше

## Отримання API ключів

### OpenAI
1. Йдіть на https://platform.openai.com/api-keys
2. Натисніть "Create new secret key"
3. Скопіюйте ключ (формат: `sk-proj-...`)
4. Вставте в `appsettings.json`

### Claude
1. Йдіть на https://console.anthropic.com/
2. Створіть API key
3. Скопіюйте ключ (формат: `sk-ant-api03-...`)
4. Вставте в `appsettings.json`

## Тестування

```powershell
# Створіть тестову гру
$body = @{
    playerName = "Test"
    characterName = "Hero"
    race = "Human"
    class = "Warrior"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/game/new-game" `
    -Method Post -Body $body -ContentType "application/json"

$gameId = $response.gameSessionId

# Зробіть дію
$turn = @{
    gameSessionId = $gameId
    playerAction = "I look around the tavern"
} | ConvertTo-Json

$result = Invoke-RestMethod -Uri "http://localhost:5000/api/game/turn" `
    -Method Post -Body $turn -ContentType "application/json"

Write-Host "Narrative: $($result.narrative)"
```

## Повна документація

Детальніше про всі опції: [AI_PROVIDERS.md](AI_PROVIDERS.md)
