FROM postgres:alpine
COPY ./init-uuid.sh /docker-entrypoint-initdb.d/