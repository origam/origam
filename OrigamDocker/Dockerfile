#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
RUN apt-get update
RUN apt-get -y install git sudo nginx ssh
RUN apt-get -y install libgdiplus
RUN useradd -ms /bin/bash origam
RUN usermod -aG sudo origam
RUN echo "%sudo   ALL=NOPASSWD: ALL" >> /etc/sudoers
WORKDIR /root
COPY ["updateTimezone.sh", "/root"]
RUN unlink /etc/nginx/sites-enabled/default
COPY ["reverse-proxy.conf", "/etc/nginx/sites-available"]
RUN ln -s /etc/nginx/sites-available/reverse-proxy.conf /etc/nginx/sites-enabled/reverse-proxy.conf
RUN nginx -t
RUN chmod +x updateTimezone.sh
RUN su origam
USER origam
WORKDIR /home/origam

# Fix of https://community.origam.com/t/not-displaying-c-at-the-end-of-words-in-pdf-generated-from-fastreport/755/19
# Rebuilding libgdiplus library to correctly display international characters 
# in FastReports. i.e. in czech language it doesn't display è located in the end
# of string. Instead of Kè it printed just K.

RUN sudo apt-get -y install libgif-dev autoconf libtool automake build-essential gettext libglib2.0-dev libcairo2-dev libtiff-dev libexif-dev libpango1.0-dev
RUN git clone https://github.com/mono/libgdiplus.git /home/origam/libgdiplus
WORKDIR /home/origam/libgdiplus
RUN ./autogen.sh --with-pango --prefix=/usr
RUN make
RUN sudo make install

#End of rebuild libgdiplus

WORKDIR /home/origam
RUN mkdir HTML5
COPY --chown=origam:origam HTML5 /home/origam/HTML5
COPY --chown=origam:origam ["_appsettings.template", "/home/origam/HTML5"]
COPY --chown=origam:origam ["_OrigamSettings.mssql.template", "/home/origam/HTML5"]
COPY --chown=origam:origam ["_OrigamSettings.postgres.template", "/home/origam/HTML5"]
COPY --chown=origam:origam ["startServer.sh", "/home/origam/HTML5"]
COPY --chown=origam:origam ["log4net.config", "/home/origam/HTML5"]
COPY --chown=origam:origam ["updateEnvironment.sh", "/home/origam/HTML5"]
COPY --chown=origam:origam ["updateEnvironmentRoot.sh", "/home/origam/HTML5"]
WORKDIR /home/origam/HTML5
RUN mkdir data
RUN mkdir logs
RUN chmod +x startServer.sh
RUN chmod +x updateEnvironment.sh
RUN chmod +x updateEnvironmentRoot.sh
CMD sudo /etc/init.d/nginx start & sudo /root/updateTimezone.sh & ./startServer.sh
