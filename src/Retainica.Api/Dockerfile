# Multi-stage build for the Retainica API (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first for layer caching
COPY Retainica.Api.csproj ./
RUN dotnet restore Retainica.Api.csproj

# Build + publish
COPY . ./
RUN dotnet publish Retainica.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
# Render injects PORT; Program.cs binds to it. Default for local container runs:
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "Retainica.Api.dll"]
