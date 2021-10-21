#!/bin/bash
cd /home/origam
echo "-->$PG_Origam_Password<--";
if [[ -f "postgres.13.template.yml" ]]; then
	echo "alter user postgres with password '$PG_Origam_Password';" >postgres_password.sql
	sudo pups postgres.13.template.yml
	rm postgres.13.template.yml
	rm postgres_password.sql
else
sudo /etc/init.d/postgresql start
fi

echo "Press [CTRL+C] to stop.."
while true
do
    sleep 10
done
