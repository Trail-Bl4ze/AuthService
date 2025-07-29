# Build stage (без изменений)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AuthService/AuthService.csproj", "AuthService/"]
COPY ["AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["AuthService.App/AuthService.App.csproj", "AuthService.App/"]
RUN dotnet restore "AuthService/AuthService.csproj"
COPY . .
WORKDIR "/src/AuthService"
RUN dotnet build "AuthService.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/AuthService"
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Настройки (только HTTP)
ENV ASPNETCORE_URLS="http://+:80"
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 80
ENTRYPOINT ["dotnet", "AuthService.dll"]
