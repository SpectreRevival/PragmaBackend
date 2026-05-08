FROM ubuntu:24.04

WORKDIR /server

COPY out/build/x64-release-linux/ .
RUN chmod +x ./pragmabackend
EXPOSE 8081
EXPOSE 8082
EXPOSE 80
CMD ["./pragmabackend"]
