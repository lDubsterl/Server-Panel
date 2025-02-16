FROM bookworm-slim:latest

RUN apt install wget && wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt update && apt install vsftpd libpam-pwdfile apache2-utils aspnetcore-runtime-8.0

RUN rm -rf /etc/pam.d/vsftpd && 
    touch /etc/pam.d/vsftpd &&
    touch /etc/vsftpwd

COPY ./Configs/SystemConfigs/vsftpd.conf /etc/vsftpd.conf
COPY ./Configs/SystemConfigs/vsftpd /etc/pam.d/vsftpd

RUN service vsftpd restart


#WORKDIR mnt
#EXPOSE 25500