# Error Sender API

Простий API для відправки помилок в Telegram групу з використанням Clean Architecture, MediatR, AutoMapper та JWT Authentication.

## 🚀 Особливості

- ✅ **Clean Architecture** з CQRS pattern
- ✅ **Автоматична відправка помилок** через middleware
- ✅ **JWT Authentication** (опціонально)
- ✅ **AutoMapper** з автоматичним маппінгом через IMapWith
- ✅ **Comprehensive Exception Handling**
- ✅ **Structured Logging** (файли + консоль)
- ✅ **Swagger UI** з повною документацією
- ✅ **HTTPS підтримка** в production
- 🔒 **Security First** - конфідеційні дані не зберігаються в коді

## Налаштування Telegram Бота

### 1. Створення Telegram Бота

1. Відкрийте Telegram і знайдіть @BotFather
2. Відправте команду `/newbot`
3. Дайте ім'я вашому боту
4. Дайте username боту (повинен закінчуватися на "bot")
5. Збережіть отриманий токен

### 2. Отримання Chat ID

1. Створіть групу в Telegram або використайте існуючу
2. Додайте вашого бота в групу
3. Відправте будь-яке повідомлення в групу
4. Перейдіть за посиланням: `https://api.telegram.org/bot<YOUR_BOT_TOKEN>/getUpdates`
5. Знайдіть `chat.id` у відповіді

### 3. Конфігурація додатку

Оновіть `appsettings.json`:

```json
{
  "AllowedHosts": "*",
  "serverUrl": "http://localhost:5001/",
  "Telegram": {
    "BotToken": "1234567890:AAEhBOweik6ad9r_QXMENQjcrGbqCr4K-1s",
    "ChatId": "-1001234567890",
    "BaseUrl": "https://api.telegram.org/bot"
  }
}
```

## 🔒 JWT Authentication (Опціонально)

⚠️ **ВАЖЛИВА ЗАУВАГА БЕЗПЕКИ**: JWT налаштування НЕ зберігаються в appsettings.json файлах з міркувань безпеки!

**Для налаштування JWT authentication дивіться: [JWT-SECURITY.md](JWT-SECURITY.md)**

Підтримувані методи:
- Environment Variables (Production)
- User Secrets (Development)  
- Azure Key Vault (Azure Production)
- Docker Secrets (Container Production)

Якщо JWT налаштовано → автоматично активується authentication  
Якщо не налаштовано → API працює без authentication

## 📡 API Endpoints

### 1. Відправка повної інформації про помилку

**POST** `/api/Error/SendToTelegram`

```json
{
  "errorMessage": "Null reference exception occurred",
  "source": "UserController.GetUser",
  "stackTrace": "at UserController.GetUser()...",
  "userId": "user123",
  "severity": "Error",
  "additionalInfo": "User was trying to access deleted record"
}
```

**Відповідь:**
```json
{
  "executionStatus": {
    "errorCode": 200,
    "status": "OK",
    "errors": []
  },
  "data": {
    "isSuccess": true,
    "message": "Error sent successfully to Telegram",
    "telegramMessageId": "123"
  }
}
```

### 2. Відправка простої помилки

**POST** `/api/Error/SendSimpleError?errorMessage=Something went wrong&source=API&severity=Warning`

## 🤖 Автоматична відправка помилок

Додаток автоматично відправляє всі необроблені винятки в Telegram групу через middleware `TelegramErrorMiddleware`. Повідомлення включають:

- 🔴 **Error** - Критичні помилки  
- 🟡 **Warning** - Попередження
- 🔵 **Info** - Інформаційні повідомлення

## 🏗️ Архітектура проекту

```
📁 Domain/
├── Models/
│   ├── ErrorReport.cs       # Доменна модель помилки
│   └── ExecutionStatus.cs   # Стандартна відповідь API

📁 ErrSendApplication/
├── DTO/
│   └── ErrorReportDto.cs    # DTO з автоматичним маппінгом
├── Interfaces/
│   ├── ITelegramService.cs  # Інтерфейс Telegram сервісу  
│   └── IHttpClientWr.cs     # HTTP клієнт wrapper
├── Proceses/
│   ├── SendErrorToTelegramCommand.cs        # MediatR команда
│   └── SendErrorToTelegramCommandHandler.cs # Обробник команди
├── Common/Configs/
│   ├── TelegramConfig.cs    # Конфігурація Telegram
│   └── TokenConfig.cs       # Конфігурація JWT
└── Behaviors/
    ├── LoggingBehavior.cs   # Pipeline логування
    └── ValidationBehavior.cs # Pipeline валідації

📁 ErrSendPersistensTelegram/
└── Services/
    └── TelegramService.cs   # Реалізація Telegram API

📁 ErrSendWebApi/
├── Controllers/
│   ├── BaseController.cs    # Базовий контролер з MediatR
│   └── ErrorController.cs   # API для помилок
└── Middleware/
    └── TelegramErrorMiddleware.cs # Автоматична відправка помилок
```

## 🛠️ Використані технології

- **.NET 8** - Основна платформа
- **MediatR** - CQRS pattern і pipeline behaviors
- **AutoMapper** - Маппінг між DTO і Domain моделями
- **FluentValidation** - Валідація запитів
- **Serilog** - Структуроване логування
- **JWT Bearer** - Authentication (опціонально)
- **Swagger/OpenAPI** - Документація API
- **System.Text.Json** - JSON серіалізація

## 🔧 Exception Handling

Додаток обробляє наступні типи винятків:

- `ValidationException` → 400 Bad Request
- `SecurityTokenException` → 401 Unauthorized  
- `UnauthorizedAccessException` → 401 Unauthorized
- `ArgumentNullException` → 400 Bad Request
- `ArgumentException` → 400 Bad Request
- `InvalidOperationException` → 409 Conflict
- `TimeoutException` → 408 Request Timeout
- `NotImplementedException` → 501 Not Implemented
- `Exception` (загальний) → 500 Internal Server Error

## 🚀 Запуск проекту

1. **Склонуйте репозиторій**
   ```bash
   git clone <repository-url>
   cd ErrorSender
   ```

2. **Оновіть конфігурацію**
   ```bash
   # Скопіюйте приклад конфігурації
   cp ErrSendWebApi/appsettings.example.json ErrSendWebApi/appsettings.json
   
   # Додайте ваші Telegram credentials
   # Опціонально: Налаштуйте JWT (див. JWT-SECURITY.md)
   ```

3. **Запустіть проект**
   ```bash
   dotnet run --project ErrSendWebApi
   ```

4. **Відкрийте Swagger UI**
   - Development: `http://localhost:5001`
   - Production: `https://localhost:5002`

## 📝 Логування

Логи записуються в:
- **Файли**: `ErrSendWebApi/Logs/ErrorSender-YYYY-MM-DD.log`
- **Консоль**: Структуровані логи через Serilog
- **База даних**: Готово до підключення (TODO)

## 🛡️ Безпека

- ✅ JWT токени зберігаються безпечно (Environment Variables/User Secrets)
- ✅ Конфідеційні дані не потрапляють в Git
- ✅ HTTPS в production
- ✅ Proper error handling без розкриття внутрішньої інформації
- ✅ Input validation через FluentValidation

## 🔄 Приклад використання

### В коді C#:
```csharp
// Автоматично через middleware - всі винятки відправляються в Telegram

// Або вручну через API:
var errorReport = new ErrorReportDto
{
    ErrorMessage = "Database connection failed",
    Source = "DatabaseService",
    Severity = "Error",
    UserId = "user123",
    AdditionalInfo = "Connection timeout after 30 seconds"
};

// Відправляється через POST /api/Error/SendToTelegram
```

### Через HTTP:
```bash
curl -X POST "http://localhost:5001/api/Error/SendSimpleError" \
     -d "errorMessage=Test error&source=Manual Test&severity=Warning"
```

## 🎯 Telegram повідомлення виглядає так:

```
🔴 ERROR REPORT
Severity: Error
Time: 2024-01-15 14:30:25 UTC
Source: UserController.GetUser
User: user123
Message: Object reference not set to an instance of an object
Additional Info: User was trying to access deleted record
Stack Trace:
at UserController.GetUser() in UserController.cs:line 45
```

## 📋 Файли конфігурації

- `appsettings.json` - основна конфігурація (БЕЗ секретів)
- `appsettings.Development.json` - налаштування розробки (БЕЗ секретів)
- `appsettings.example.json` - приклад конфігурації
- `JWT-SECURITY.md` - інструкції з безпечного налаштування JWT

Готово! Тепер ваш API повністю безпечний і готовий для публічного розміщення! 🎉🔒 