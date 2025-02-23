#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

RUN apt update && apt install -y docker.io
#RUN apt install -y wget apt-transport-https gpg
#RUN wget -qO - https://packages.adoptium.net/artifactory/api/gpg/key/public | gpg --dearmor | tee /etc/apt/trusted.gpg.d/adoptium.gpg > /dev/null
#RUN echo "deb https://packages.adoptium.net/artifactory/deb $(awk -F= '/^VERSION_CODENAME/{print$2}' /etc/os-release) main" | tee /etc/apt/sources.list.d/adoptium.list
#RUN apt update && apt install temurin-21-jre
RUN mkdir -p /mnt/ApplicationData/Servers /mnt/ApplicationData/Configs

RUN apt install vsftpd libpam-pwdfile apache2-utils
RUN rm -rf /etc/pam.d/vsftpd && 
    touch /etc/pam.d/vsftpd &&
    touch /etc/vsftpwd
COPY ./Configs/SystemConfigs/vsftpd.conf /etc/vsftpd.conf
COPY ./Configs/SystemConfigs/vsftpd /etc/pam.d/vsftpd
RUN service vsftpd restart

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
ENTRYPOINT ["dotnet", "Panel.WebAPI.dll"]