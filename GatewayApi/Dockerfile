# Contexto de construcción: la raíz de la solución (docker build -f GatewayApi/Dockerfile .)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY GatewayApi/GatewayApi.csproj GatewayApi/
RUN dotnet restore GatewayApi/GatewayApi.csproj
COPY GatewayApi/ GatewayApi/
RUN dotnet publish GatewayApi/GatewayApi.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
# En Production el gateway carga ocelot.Production.json
ENV ASPNETCORE_ENVIRONMENT=Production
# Render inyecta PORT; si no existe se usa 8080.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet GatewayApi.dll"]
