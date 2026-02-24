# 🚀 Деплой InfiniteTavern на Render

## Крок 1: Підготовка MongoDB

### Варіант А: MongoDB Atlas (рекомендовано)
1. Створіть безкоштовний акаунт на [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)
2. Створіть кластер (M0 Sandbox - безкоштовно)
3. Отримайте connection string: `mongodb+srv://username:password@cluster.mongodb.net/`

### Варіант Б: Render MongoDB (якщо доступно)
1. В Render Dashboard → New → MongoDB
2. Скопіюйте Internal Connection String

## Крок 2: Деплой Backend на Render

### Через Render Dashboard:

1. **Створіть новий Web Service:**
   - Перейдіть на [Render Dashboard](https://dashboard.render.com/)
   - Натисніть "New +" → "Web Service"
   - Підключіть ваш GitHub репозиторій

2. **Налаштуйте сервіс:**
   ```
   Name: infinite-tavern-api
   Region: Frankfurt (або найближчий до вас)
   Branch: main
   Runtime: Docker
   ```

3. **Додайте Environment Variables:**
   ```
   ConnectionStrings__DefaultConnection = mongodb+srv://your-connection-string
   MongoDB__DatabaseName = InfiniteTavern
   AI__Provider = OpenAI
   OpenAI__ApiKey = your-openai-api-key
   Anthropic__ApiKey = your-anthropic-api-key (якщо використовуєте)
   ASPNETCORE_ENVIRONMENT = Production
   ```

4. **Налаштування Docker:**
   - Docker Build Context: `/` (корінь проекту)
   - Dockerfile Path: `./Dockerfile`

5. **Натисніть "Create Web Service"**

## Крок 3: Перевірка

Після успішного деплою:
- URL API: `https://infinite-tavern-api.onrender.com`
- Swagger: `https://infinite-tavern-api.onrender.com/swagger`
- Тестовий endpoint: `https://infinite-tavern-api.onrender.com/api/game/health`

## Крок 4: Налаштування Frontend

Оновіть файл `frontend/src/services/gameService.ts`:
```typescript
const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://infinite-tavern-api.onrender.com';
```

## ⚠️ Важливі нотатки:

1. **Безкоштовний план Render:**
   - Сервіс "засинає" після 15 хв бездіяльності
   - Перший запит може тривати 30-50 секунд
   - 750 годин на місяць безкоштовно

2. **MongoDB Atlas Free Tier:**
   - 512 MB сховища
   - Достатньо для розробки та тестування

3. **API Keys:**
   - Ніколи не комітьте реальні ключі в Git
   - Використовуйте Environment Variables в Render

4. **CORS:**
   - Якщо потрібно обмежити домени, оновіть CORS політику в Program.cs

## 🔧 Troubleshooting

### Помилка підключення до MongoDB:
```bash
# Перевірте connection string в Environment Variables
# Переконайтеся, що IP Render додано в MongoDB Atlas Network Access (0.0.0.0/0)
```

### API не відповідає:
```bash
# Перегляньте логи: Render Dashboard → Your Service → Logs
# Перевірте health endpoint: curl https://your-app.onrender.com/api/game/health
```

### Повільний старт:
```bash
# Це нормально для безкоштовного плану
# Сервіс "просинається" за 30-50 секунд
```

## 📝 Альтернативи Render

- **Railway** - подібний до Render, також має free tier
- **Fly.io** - безкоштовні compute hours
- **Azure App Service** - $200 кредитів для нових користувачів
- **Google Cloud Run** - pay-per-use, дешево для малих проектів

## 🔄 Оновлення

Render автоматично перебудовує застосунок при кожному push в GitHub.

Для ручного деплою: Render Dashboard → Your Service → Manual Deploy → Deploy latest commit
