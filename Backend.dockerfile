FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY ./PragmaBackend.slnx ./
COPY ./BackendServer/BackendServer.csproj ./BackendServer/
COPY ./Model/Model.csproj ./Model/
COPY ./Packets/Packets.csproj ./Packets/
COPY ./Tests/Tests.csproj ./Tests/
COPY ./Processors/Processors.csproj ./Processors/
RUN dotnet restore

COPY ./BackendServer ./BackendServer
COPY ./Model/ ./Model
COPY ./Packets ./Packets
COPY ./Tests ./Tests
COPY ./Processors ./Processors 
RUN dotnet publish ./BackendServer/BackendServer.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build-env /app/out .

EXPOSE 8080
EXPOSE 8081
EXPOSE 8082

ENTRYPOINT ["dotnet", "BackendServer.dll"]
