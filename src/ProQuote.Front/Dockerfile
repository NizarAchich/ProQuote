# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY [\"ProQuote.sln\", \"./\"]
COPY [\"src/ProQuote.Core/ProQuote.Core.csproj\", \"src/ProQuote.Core/\"]
COPY [\"src/ProQuote.Front/ProQuote.Front.csproj\", \"src/ProQuote.Front/\"]
RUN dotnet restore ProQuote.sln
COPY . .
RUN dotnet publish src/ProQuote.Front/ProQuote.Front.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT [\"dotnet\", \"ProQuote.Front.dll\"]
