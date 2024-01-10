SET "containerName=origam-demo"
SET "projectDir=%cd%\.."

docker container rm %containerName%

docker run -d --name %containerName% --env-file mydemo.env -it -v %projectDir%:/home/origam/HTML5/data/origam -p 8080:8080 origam/server:2024.1.0.3214.linux

Rem # konfigurační soubory
docker exec %containerName% sh -c "cp -r /home/origam/HTML5/data/origam/_origamconf/* /home/origam/HTML5"

docker restart %containerName%
