# Этап 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore
COPY ["PasswordManager.Web/PasswordManager.Web.csproj", "PasswordManager.Web/"]
RUN dotnet restore "PasswordManager.Web/PasswordManager.Web.csproj"

COPY . .
WORKDIR "/src/PasswordManager.Web"
RUN dotnet build "PasswordManager.Web.csproj" -c Release -o /app/build

# Этап 2: Publish 
FROM build AS publish
RUN dotnet publish "PasswordManager.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап 3: Runtime 
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD wget -q --spider http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "PasswordManager.Web.dll"]
