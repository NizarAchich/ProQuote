# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ProQuote.sln", "./"]
COPY ["src/ProQuote.Build/ProQuote.Build.csproj", "src/ProQuote.Build/"]
COPY ["src/ProQuote.Core/ProQuote.Core.csproj", "src/ProQuote.Core/"]
COPY ["src/ProQuote.Database/ProQuote.Database.csproj", "src/ProQuote.Database/"]
COPY ["src/ProQuote.Backend/ProQuote.Backend.csproj", "src/ProQuote.Backend/"]
COPY ["src/ProQuote.Front/ProQuote.Front.csproj", "src/ProQuote.Front/"]
COPY ["src/ProQuote.Automation/ProQuote.Automation.csproj", "src/ProQuote.Automation/"]
COPY ["src/ProQuote.Plugins/ProQuote.Plugins.csproj", "src/ProQuote.Plugins/"]
COPY ["src/ProQuote.Integrations/ProQuote.Integrations.csproj", "src/ProQuote.Integrations/"]
COPY ["tests/ProQuote.Core.Tests/ProQuote.Core.Tests.csproj", "tests/ProQuote.Core.Tests/"]
COPY ["tests/ProQuote.Backend.Tests/ProQuote.Backend.Tests.csproj", "tests/ProQuote.Backend.Tests/"]
COPY ["tests/ProQuote.Automation.Tests/ProQuote.Automation.Tests.csproj", "tests/ProQuote.Automation.Tests/"]
COPY ["tests/ProQuote.Front.Tests/ProQuote.Front.Tests.csproj", "tests/ProQuote.Front.Tests/"]
RUN dotnet restore ProQuote.sln
COPY . .
RUN dotnet publish src/ProQuote.Front/ProQuote.Front.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProQuote.Front.dll"]
