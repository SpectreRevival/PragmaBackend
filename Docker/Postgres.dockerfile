FROM postgres:alpine
COPY ./init-uuid.sql /docker-entrypoint-initdb.d/
