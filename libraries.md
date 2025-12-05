# Libraries & Dependencies – ProQuote

Ce document liste les librairies (NuGet et outils externes) nécessaires pour chaque projet de la solution ProQuote.

---

# 1. Vue d’ensemble

Les projets suivent une architecture modulaire :

```text
src/
  ProQuote.Build
  ProQuote.Core
  ProQuote.Database
  ProQuote.Backend
  ProQuote.Front
  ProQuote.Automation
  ProQuote.Plugins
  ProQuote.Integrations
tests/
  ProQuote.Core.Tests
  ProQuote.Backend.Tests
  ProQuote.Automation.Tests
  ProQuote.Front.Tests
```

Chaque section ci-dessous donne les **packages NuGet recommandés** + les **options utiles**.

---

# 2. ProQuote.Build

**Type :** scripts / tooling (build, lint, ci)

### 📦 Libraries / Tools

- `dotnet-format` (outil global CLI)  
- `GitVersion.Tool` (optionnel)

### 🔧 Autres fichiers

- `.editorconfig`
- YAML GitHub Actions
- Scripts PowerShell/Bash

---

# 3. ProQuote.Core

**Objectif :** cœur métier **sans dépendances lourdes**

### 📦 Librairies recommandées

Aucune dépendance obligatoire.

### 📦 Optionnelles

- `FluentValidation` – pour validations métier (option)
- `OneOf` / `LanguageExt` – pour modèles fonctionnels (option)

> 💡 Pour une V1 : **rester 100% sans dépendance externe**.

---

# 4. ProQuote.Database

**Type :** EF Core + persistance

### 📦 Obligatoires

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`

### 📦 Provider (choisir 1)

- SQL Server :  
  - `Microsoft.EntityFrameworkCore.SqlServer`
- PostgreSQL :  
  - `Npgsql.EntityFrameworkCore.PostgreSQL`

### 📦 Optionnels

- `EFCore.NamingConventions`
- `Microsoft.Extensions.Logging.Console`

---

# 5. ProQuote.Backend

**Type :** ASP.NET Core Web API

### 📦 Principales

- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Swashbuckle.AspNetCore` — Swagger UI
- `Microsoft.Extensions.Configuration.UserSecrets`

### 📦 Logging

- `Serilog.AspNetCore`
- `Serilog.Sinks.Console`
- `Serilog.Sinks.File` (optionnel)
- `Serilog.Sinks.Seq` (optionnel)

### 📦 Optionnels (recommandés)

- `AutoMapper`
- `FluentValidation.AspNetCore`
- `Hellang.Middleware.ProblemDetails`

---

# 6. ProQuote.Front

**Type :** Blazor Server

### 📦 Obligatoires

- `MudBlazor`
- Framework Blazor (inclus via SDK)

### 📦 Composants UI supplémentaires (optionnels)

- `Blazored.LocalStorage`
- `Blazor.PDFViewer` ou wrapper JS (si besoin)

### 📦 Tests (dans projet tests)

- `bUnit`

---

# 7. ProQuote.Automation

**Type :** moteur de flows no-code

### 📦 Obligatoires

- Aucun package externe

### 📦 Optionnel

- `Hangfire.Core` + `Hangfire.AspNetCore`  
  (si tu veux des flows planifiés)

---

# 8. ProQuote.Plugins

**Type :** extensions personnalisées

### 📦 Obligatoires

- `ProQuote.Core` (référence interne)

### 📦 Optionnels

Selon les plugins spécifiques :
- SDK externes
- Import CSV/Excel (ex: `NPOI`)

---

# 9. ProQuote.Integrations

**Type :** intégrations externes (email, IA, stockage)

### 📦 Email

- `MailKit` — SMTP fiable

### 📦 Outlook / Microsoft 365

- `Microsoft.Graph`

### 📦 Stockage cloud (optionnel)

- Azure Blob : `Azure.Storage.Blobs`
- AWS S3 : `AWSSDK.S3`

### 📦 IA (optionnel)

- `OpenAI` ou `Azure.AI.OpenAI`

---

# 10. Tests

### 10.1 `ProQuote.Core.Tests`

- `xunit`
- `FluentAssertions`

### 10.2 `ProQuote.Backend.Tests`

- `xunit`
- `FluentAssertions`
- `Moq` ou `NSubstitute`
- `Microsoft.AspNetCore.Mvc.Testing`

### 10.3 `ProQuote.Automation.Tests`

- `xunit`
- `FluentAssertions`
- `Moq`

### 10.4 `ProQuote.Front.Tests`

- `bUnit`
- `xunit`
- `FluentAssertions`

---

# 11. Résumé par projet

| Projet | Librairies principales | Optionnels |
|--------|------------------------|------------|
| **Core** | aucune | FluentValidation |
| **Database** | EF Core + provider | NamingConventions |
| **Backend** | Identity, Swagger, Serilog | AutoMapper, ProblemDetails |
| **Front** | MudBlazor | PDF viewer, LocalStorage |
| **Automation** | aucune | Hangfire |
| **Plugins** | Core | SDK spécifiques |
| **Integrations** | MailKit, Microsoft.Graph | Azure/AWS Storage, IA |
| **Tests** | xunit, FluentAssertions | bUnit, Moq |

---

# 12. Conclusion

Ce document centralise toutes les dépendances nécessaires pour maintenir une architecture modulaire, testable et scalable.

Tu peux maintenant plugger ce fichier directement dans ton repo GitHub et le maintenir comme **référence officielle** des dépendances.
