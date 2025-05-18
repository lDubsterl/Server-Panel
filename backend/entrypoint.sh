#!/bin/bash
vsftpd /etc/vsftpd.conf &
dotnet Panel.WebAPI.dll