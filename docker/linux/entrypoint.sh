#!/bin/bash
cd /etc/nginx/ssl
sudo /etc/nginx/ssl/createSslCertificate.sh
sudo /etc/init.d/nginx start
sudo /root/updateTimezone.sh
cd /home/origam/HTML5
./configureServer.sh
export ASPNETCORE_URLS="http://+:8080"
exec dotnet Origam.Server.dll