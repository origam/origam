#!bin/bash
cd /home/origam/HTML5
kill -15 $(pgrep -f dotnet)
sleep 5
kill -9 $(pgrep -f dotnet)
./startServer.sh
