Emulates the backend service made by Pragma for the game spectre divide.

# Recommended Tools
- Visual Studio (either 2022 or 2026)
- pgAdmin 4
- Docker desktop
# Overview of Projects / Submodules

## Model
- our internal representation of the backend's data structures
- has embedded json unpacked
- has stuff like port numbers being doubles fixed
- most classes are designed to be SQL tables with player ids as primary key
- these are the structures that we will be passing around in the backend

## Packets
- spectre's representation of the backend data structures
- basically just the protobuf classes we had before
- contains constructors to convert from Model classes to Packet classes
- what actually gets sent / serialized to JSON
- Also has functions to go from packet to model classes for receiving update packets

## Persistence
- Pushing model classes to the database
- Receiving model classes from the database
- Might make this an interface class so we can support multiple backends (ie SQLite for testing / ease of use and Postgres for prod)

## BackendServer
- Packet processors
- Commands (eg making one user friends with another)
- Responds to requests 

# Development guide / setup workflow
1. With your ide of choice open the solution and get it to build. You can also rename env.example.json to env.json if you want to use the json configuration secrets file.
2. `cd Docker`
3. On windows, `copy .env.example .env` or on linux `cp .env.example .env`
4. `docker build -t pragmabackend:latest -f Backend.dockerfile ..`
5. `docker build -t pragmabackend-pgdb:latest -f Postgres.dockerfile .`
6. `docker compose up` Note that you can add the -d flag to this command if you would like your terminal to detach from the console view.
7. When finished, `docker compose down` will stop the application while preserving data while `docker compose down -v` will stop the application and remove all data.
If you want to only start the database, use `docker compose up backend-pgdb` and `docker compose down backend-pgdb`
