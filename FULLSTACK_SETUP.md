# Infinite Tavern - Full Stack Setup Guide

Повна інструкція для запуску Infinite Tavern (Backend + Frontend + MongoDB).

## 📋 Що потрібно

- **Node.js 18+** - для фронтенду
- **.NET 8 SDK** - для бекенду
- **Docker Desktop** - для MongoDB (або локальний MongoDB)
- **AI API Key** - OpenAI або Anthropic

## 🚀 Швидкий запуск (3 кроки)

### 1. Запустити MongoDB
```powershell
docker-compose up -d
```

### 2. Запустити Backend
```powershell
# Налаштувати API key в appsettings.json
dotnet run --project src/InfiniteTavern.API
```

Backend: http://localhost:5000

### 3. Запустити Frontend
```powershell
cd frontend
npm install
npm run dev
```

Frontend: http://localhost:3000

## ✅ Перевірка

### MongoDB працює?
```powershell
docker ps
# Має бути: infinitetavern_mongodb
```

### Backend працює?
```powershell
curl http://localhost:5000/api/health
# Або відкрити: http://localhost:5000 (Swagger)
```

### Frontend працює?
Відкрити браузер: http://localhost:3000

## 🎮 Як грати

1. Відкрити http://localhost:3000
2. Заповнити форму створення персонажа
3. Натиснути "Begin Adventure"
4. Вводити дії в поле внизу
5. Насолоджуватись грою! 🎲

## 🛠️ Детальні інструкції

### Backend Setup

#### 1. Налаштувати AI Provider

Редагувати `src/InfiniteTavern.API/appsettings.json`:

```json
{
  "AI": {
    "Provider": "OpenAI"
  },
  "OpenAI": {
    "ApiKey": "sk-proj-YOUR-KEY-HERE"
  }
}
```

Для Claude:
```json
{
  "AI": {
    "Provider": "Claude"
  },
  "Anthropic": {
    "ApiKey": "sk-ant-YOUR-KEY-HERE"
  }
}
```

#### 2. Зібрати проект
```powershell
dotnet build
```

#### 3. Запустити
```powershell
dotnet run --project src/InfiniteTavern.API
```

### Frontend Setup

#### 1. Встановити залежності
```powershell
cd frontend
npm install
```

#### 2. Запустити dev server
```powershell
npm run dev
```

Vite автоматично проксує API запити з `/api/*` на `http://localhost:5000`.

## 🐛 Troubleshooting

### MongoDB не запускається
```powershell
# Перевірити логи
docker logs infinitetavern_mongodb

# Перезапустити
docker-compose down
docker-compose up -d
```

### Backend compile errors
```powershell
dotnet clean
dotnet restore
dotnet build
```

### Frontend не підключається до API
- Перевірити що backend запущений на порту 5000
- Перевірити CORS налаштування в Program.cs
- Перевірити vite.config.ts proxy налаштування

### AI API помилки
- Перевірити що API key правильний
- Перевірити що є інтернет з'єднання
- Перевірити що є кредити на акаунті (OpenAI/Anthropic)

## 📊 Архітектура

```
┌─────────────────────┐
│   Browser (React)   │  ← http://localhost:3000
│   - Vite Dev Server │
└──────────┬──────────┘
           │ HTTP /api/*
           │ (proxied by Vite)
           ▼
┌─────────────────────┐
│  ASP.NET Core API   │  ← http://localhost:5000
│  - Game Logic       │
│  - AI Integration   │
└──────────┬──────────┘
           │
    ┌──────┴──────┐
    │             │
    ▼             ▼
┌─────────┐  ┌──────────┐
│ OpenAI  │  │ MongoDB  │  ← docker container
│ Claude  │  │ :27017   │
└─────────┘  └──────────┘
```

## 🔧 Development Tips

### Hot Reload
- **Frontend**: Vite автоматично перезавантажує при змінах
- **Backend**: dotnet watch для hot reload
  ```powershell
  dotnet watch run --project src/InfiniteTavern.API
  ```

### Debug Console
- **Frontend**: Відкрити DevTools (F12) → Console
- **Backend**: Логи виводяться в консоль

### MongoDB Data
```powershell
# Підключитись до MongoDB
mongosh

# Переключитись на БД
use InfiniteTavern

# Переглянути ігрові сесії
db.GameSessions.find().pretty()

# Видалити все (для тестів)
db.GameSessions.deleteMany({})
```

## 📦 Production Build

### Backend
```powershell
dotnet publish -c Release -o ./publish
```

### Frontend
```powershell
cd frontend
npm run build
# Папка dist/ містить static files
```

Deploy на:
- Backend: Azure, AWS, DigitalOcean
- Frontend: Netlify, Vercel, GitHub Pages
- MongoDB: MongoDB Atlas

## 🎯 Next Steps

Після успішного запуску:
1. ✅ Спробуйте різні AI providers (OpenAI vs Claude)
2. ✅ Експериментуйте з промптами
3. ✅ Додайте нові фічі у фронтенд
4. ✅ Кастомізуйте UI тему
5. ✅ Додайте звуки/анімації

## 📚 Документація

- [README.md](README.md) - Загальна інформація
- [frontend/README.md](frontend/README.md) - Frontend специфіка
- [MONGODB_MIGRATION.md](MONGODB_MIGRATION.md) - Міграція на MongoDB
- [ARCHITECTURE.md](ARCHITECTURE.md) - Детальна архітектура

---

**Happy Gaming! 🎲⚔️🏰**
