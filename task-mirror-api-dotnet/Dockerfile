# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tudo do repo (dessa pasta do projeto)
COPY . .

# Restaura dependências do projeto da API
RUN dotnet restore ./task-mirror-api-dotnet.csproj

# Publica em Release
RUN dotnet publish ./task-mirror-api-dotnet.csproj -c Release -o /app/out

# ===== runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia artefatos publicados
COPY --from=build /app/out ./

# Porta interna do container
EXPOSE 8080

# Render injeta $PORT; localmente usa 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}

# DLL gerada pelo projeto (mesmo nome do csproj)
ENTRYPOINT ["dotnet","task-mirror-api-dotnet.dll"]
