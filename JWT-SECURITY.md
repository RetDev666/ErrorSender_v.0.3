# JWT Security Configuration

⚠️ **ВАЖЛИВО**: JWT токени НЕ повинні зберігатися в appsettings.json файлах, особливо в публічних репозиторіях!

## 🔐 Безпечні методи налаштування JWT

### Метод 1: Environment Variables (Рекомендовано для Production)

```bash
# Linux/Mac
export JWT__TokenKey="your-super-secret-jwt-key-at-least-32-characters-long"
export JWT__Issuer="ErrorSenderApi"
export JWT__Audience="https://yourdomain.com"
export JWT__TokenExpiryMinutes="60"
export JWT__RefreshTokenExpiryDays="7"

# Windows
set JWT__TokenKey=your-super-secret-jwt-key-at-least-32-characters-long
set JWT__Issuer=ErrorSenderApi
set JWT__Audience=https://yourdomain.com
set JWT__TokenExpiryMinutes=60
set JWT__RefreshTokenExpiryDays=7
```

### Метод 2: User Secrets (Для Development)

```bash
# Ініціалізувати User Secrets
dotnet user-secrets init --project ErrSendWebApi

# Додати JWT налаштування
dotnet user-secrets set "JWT:TokenKey" "your-super-secret-jwt-key-at-least-32-characters-long" --project ErrSendWebApi
dotnet user-secrets set "JWT:Issuer" "ErrorSenderApi" --project ErrSendWebApi
dotnet user-secrets set "JWT:Audience" "http://localhost:5001" --project ErrSendWebApi
dotnet user-secrets set "JWT:TokenExpiryMinutes" "60" --project ErrSendWebApi
dotnet user-secrets set "JWT:RefreshTokenExpiryDays" "7" --project ErrSendWebApi
```

### Метод 3: Azure Key Vault (Для Azure Production)

```csharp
// В Program.cs або Startup.cs
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Метод 4: Docker Secrets

```yaml
version: '3.8'
services:
  errorapi:
    image: errorapi:latest
    environment:
      - JWT__TokenKey_FILE=/run/secrets/jwt_token_key
    secrets:
      - jwt_token_key

secrets:
  jwt_token_key:
    external: true
```

## 🏗️ Структура JWT конфігурації

Якщо JWT налаштовано, додаток буде очікувати наступну структуру:

```json
{
  "JWT": {
    "TokenKey": "мінімум-32-символи-для-безпеки",
    "Issuer": "ErrorSenderApi",
    "Audience": "https://yourdomain.com",
    "TokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

## ✅ Перевірка налаштування

Додаток автоматично перевіряє наявність JWT конфігурації:
- Якщо `JWT:TokenKey` присутній → JWT authentication активований
- Якщо відсутній → API працює без authentication

## 🔑 Генерація безпечного ключа

```bash
# Генерація 256-бітного ключа
openssl rand -base64 32

# Або через PowerShell
[System.Convert]::ToBase64String([System.Security.Cryptography.RNGCryptoServiceProvider]::new().GetBytes(32))
```

## 🚫 Що НЕ робити

❌ Ніколи не додавайте JWT ключі в:
- appsettings.json
- appsettings.Development.json  
- Код-файли
- Git repositories
- Логи
- Error messages

✅ Завжди використовуйте:
- Environment variables
- User secrets (для розробки)
- Key vaults (для production)
- Docker secrets 