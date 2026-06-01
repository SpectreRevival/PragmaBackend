Emulates the backend service made by Pragma for the game spectre divide.

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