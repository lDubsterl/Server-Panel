#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

RUN apt-get update && \
    apt-get install -y \
    apt-transport-https \
    ca-certificates \
    curl \
    gnupg \
    lsb-release

RUN curl -fsSL https://download.docker.com/linux/debian/gpg | apt-key add - && \
    echo "deb [arch=amd64] https://download.docker.com/linux/debian $(lsb_release -cs) stable" > /etc/apt/sources.list.d/docker.list

RUN apt-get update && \
    apt-get install -y docker-ce-cli

RUN apt-get update && apt-get install -y vsftpd libpam-pwdfile apache2-utils

RUN sh -c "rm -rf /etc/pam.d/vsftpd && echo -n > /etc/pam.d/vsftpd && echo -n > /etc/vsftpwd"
COPY ./Configs/SystemConfigs/vsftpd.conf /etc/vsftpd.conf
COPY ./Configs/SystemConfigs/vsftpd /etc/pam.d/vsftpd

COPY ["Configs/DST templates", "/data/DST templates/"]
COPY ./Configs/Terraria /data/Terraria/

COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ServerPanel/Panel.WebAPI.csproj", "ServerPanel/"]
COPY ["Panel.Infrastructure/Panel.Infrastructure.csproj", "Panel.Infrastructure/"]
COPY ["Panel.Application/Panel.Application.csproj", "Panel.Application/"]
COPY ["Panel.Domain/Panel.Domain.csproj", "Panel.Domain/"]
COPY ["Panel.Shared/Panel.Shared.csproj", "Panel.Shared/"]
RUN dotnet restore "./ServerPanel/Panel.WebAPI.csproj"
COPY . .
WORKDIR "/src/ServerPanel"
RUN dotnet build "./Panel.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Panel.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["/bin/bash", "/entrypoint.sh"]