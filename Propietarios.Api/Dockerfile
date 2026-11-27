# Contexto de construcción: la raíz de la solución (docker build -f Propietarios.Api/Dockerfile .)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Propietarios.Api/Propietarios.Api.csproj Propietarios.Api/
RUN dotnet restore Propietarios.Api/Propietarios.Api.csproj
COPY Propietarios.Api/ Propietarios.Api/
RUN dotnet publish Propietarios.Api/Propietarios.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_ENVIRONMENT=Production
# Render inyecta PORT; si no existe se usa 8080.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet Propietarios.Api.dll"]
