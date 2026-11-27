# Contexto de construcción: la raíz de la solución (docker build -f Veterinaria.Api/Dockerfile .)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Veterinaria.Api/Veterinaria.Api.csproj Veterinaria.Api/
COPY Veterinaria.Negocio/Veterinaria.Negocio.csproj Veterinaria.Negocio/
COPY Veterinaria.Datos/Veterinaria.Datos.csproj Veterinaria.Datos/
COPY Veterinaria.Entidades/Veterinaria.Entidades.csproj Veterinaria.Entidades/
RUN dotnet restore Veterinaria.Api/Veterinaria.Api.csproj
COPY Veterinaria.Api/ Veterinaria.Api/
COPY Veterinaria.Negocio/ Veterinaria.Negocio/
COPY Veterinaria.Datos/ Veterinaria.Datos/
COPY Veterinaria.Entidades/ Veterinaria.Entidades/
RUN dotnet publish Veterinaria.Api/Veterinaria.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_ENVIRONMENT=Production
# Render inyecta PORT; si no existe se usa 8080.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet Veterinaria.Api.dll"]
