# Stage 1: Runtime Base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto csproj para restaurar dependências
COPY ["src/AgroGuard.Api/AgroGuard.Api.csproj", "src/AgroGuard.Api/"]
COPY ["src/AgroGuard.Application/AgroGuard.Application.csproj", "src/AgroGuard.Application/"]
COPY ["src/AgroGuard.Domain/AgroGuard.Domain.csproj", "src/AgroGuard.Domain/"]
COPY ["src/AgroGuard.Infrastructure/AgroGuard.Infrastructure.csproj", "src/AgroGuard.Infrastructure/"]

# Restaurar pacotes NuGet
RUN dotnet restore "src/AgroGuard.Api/AgroGuard.Api.csproj"

# Copiar todo o código fonte restante
COPY . .

# Compilar a API
WORKDIR "/src/src/AgroGuard.Api"
RUN dotnet build "AgroGuard.Api.csproj" -c Release -o /app/build

# Stage 3: Publicação dos Artefatos
FROM build AS publish
RUN dotnet publish "AgroGuard.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Imagem de Produção Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgroGuard.Api.dll"]
