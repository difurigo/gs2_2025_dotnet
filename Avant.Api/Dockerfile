# Imagem de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# Imagem de runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app .

# Porta padrão
EXPOSE 8080

# AspNet vai escutar nessa porta
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Avant.Api.dll"]
