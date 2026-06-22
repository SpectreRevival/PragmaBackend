FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY ../PragmaBackend.slnx ./
COPY --parents ../**/*.csproj ./
RUN dotnet restore

# env.json shouldn't be included in the build as it contains secrets
COPY ../ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build-env /app/out .

EXPOSE 8080
EXPOSE 8081
EXPOSE 8082

ENTRYPOINT ["dotnet", "BackendServer.dll"]
