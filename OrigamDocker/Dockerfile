#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
RUN apt-get update
RUN apt-get -y install git
RUN useradd -ms /bin/bash origam
RUN su origam
USER origam
WORKDIR /home/origam
RUN openssl rand -base64 10 >certpass
RUN mkdir HTML5
COPY --chown=origam:origam HTML5 /home/origam/HTML5
RUN openssl req -batch -newkey rsa:2048 -nodes -keyout serverCore.key -x509 -days 728 -out serverCore.cer
RUN openssl pkcs12 -export -in serverCore.cer -inkey serverCore.key -passout file:certpass -out /home/origam/HTML5/serverCore.pfx
COPY --chown=origam:origam ["_appsettings.template", "/home/origam/HTML5"]
COPY --chown=origam:origam ["_OrigamSettings.mssql.template", "/home/origam/HTML5"]
COPY --chown=origam:origam ["_OrigamSettings.postgres.template", "/home/origam/HTML5"]
COPY --chown=origam:origam ["startServer.sh", "/home/origam/HTML5"]
COPY --chown=origam:origam ["log4net.config", "/home/origam/HTML5"]
WORKDIR HTML5
RUN cp _appsettings.template appsettings.json
RUN mkdir data
RUN mkdir logs
RUN sed -i "s|certpassword|$(cat ..\/certpass)|" appsettings.json
RUN sed -i "s|certpassword|$(cat ..\/certpass)|" _appsettings.template
RUN rm ../certpass
RUN rm ../serverCore.cer
RUN chmod +x startServer.sh
CMD ./startServer.sh
 
