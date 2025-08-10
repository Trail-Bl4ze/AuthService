# Build stage (без изменений)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Auth.Api/Auth.Api.csproj", "Auth.Api/"]
COPY ["src/Auth.Core/Auth.Core.csproj", "Auth.Core/"]
COPY ["src/Auth.Infrastructure/Auth.Infrastructure.csproj", "Auth.Infrastructure/"]
RUN dotnet restore "Auth.Api/Auth.Api.csproj"
COPY ./src .
WORKDIR "/src/Auth.Api"
RUN dotnet build "Auth.Api.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/Auth.Api"
RUN dotnet publish "Auth.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Настройки (только HTTP)
ENV ASPNETCORE_URLS="http://+:80"
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 80
ENTRYPOINT ["dotnet", "Auth.Api.dll"]
