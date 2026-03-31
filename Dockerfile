FROM ubuntu:22.04

WORKDIR /server

COPY build/out/x64-release-linux/ .
RUN chmod +x ./pragmabackend
EXPOSE 8081
EXPOSE 8082
EXPOSE 80
CMD ["./pragmabackend"]
