# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1) Copia só o csproj da API para aproveitar cache
COPY task-mirror-api-dotnet/task-mirror-api-dotnet.csproj ./task-mirror-api-dotnet/

# 2) Restaura dependências usando o caminho correto
RUN dotnet restore ./task-mirror-api-dotnet/task-mirror-api-dotnet.csproj

# 3) Copia o restante do código da API
COPY task-mirror-api-dotnet/. ./task-mirror-api-dotnet/

# 4) Entra na pasta do projeto e publica
WORKDIR /src/task-mirror-api-dotnet
RUN dotnet publish task-mirror-api-dotnet.csproj -c Release -o /app/out /p:UseAppHost=false

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
