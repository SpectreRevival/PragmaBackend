FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY ../PragmaBackend.slnx ./
COPY --parents ../**/*.csproj ./
RUN dotnet restore

COPY ../ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 8080
EXPOSE 8081
EXPOSE 8082

ENTRYPOINT ["dotnet", "BackendServer.exe"]
