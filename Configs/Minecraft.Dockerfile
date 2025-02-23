FROM eclipse-temurin:17-jre-alpine

WORKDIR "/usr"
ENTRYPOINT ["/bin/sh", "-c"]
EXPOSE 25565