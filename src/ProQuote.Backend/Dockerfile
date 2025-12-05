# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY [\"ProQuote.sln\", \"./\"]
COPY [\"src/ProQuote.Core/ProQuote.Core.csproj\", \"src/ProQuote.Core/\"]
COPY [\"src/ProQuote.Database/ProQuote.Database.csproj\", \"src/ProQuote.Database/\"]
COPY [\"src/ProQuote.Automation/ProQuote.Automation.csproj\", \"src/ProQuote.Automation/\"]
COPY [\"src/ProQuote.Integrations/ProQuote.Integrations.csproj\", \"src/ProQuote.Integrations/\"]
COPY [\"src/ProQuote.Plugins/ProQuote.Plugins.csproj\", \"src/ProQuote.Plugins/\"]
COPY [\"src/ProQuote.Backend/ProQuote.Backend.csproj\", \"src/ProQuote.Backend/\"]
RUN dotnet restore ProQuote.sln
COPY . .
RUN dotnet publish src/ProQuote.Backend/ProQuote.Backend.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT [\"dotnet\", \"ProQuote.Backend.dll\"]
