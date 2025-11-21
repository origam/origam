#!/bin/bash

run_silently() {
    local output
    output=$("$@" 2>&1)
    local exit_code=$?
    if [ $exit_code -ne 0 ]; then
        echo "Error in command: $1"
        echo "$output"
        return $exit_code
    fi
}

cd /home/origam
PROJECT_DATA_DIRECTORY="/home/origam/projectData"
if [[ -n ${gitPullOnStart} && ${gitPullOnStart} == true ]]; then
	   # Directory data
	   gitcredentials=""
	   gitcloneBranch="-b master"
	   fullgiturl=""
	   if [[ -n ${gitBranch} ]]; then
	    gitcloneBranch="-b $gitBranch"
	   fi

		if [[ -n ${gitUrl} ]]; then
		   if [[ -n ${gitUsername} && -n ${gitPassword} ]]; then
				gitcredentials="${gitUsername}:${gitPassword}@"
		   fi
		   if [[ ${gitUrl} == https:* ]]; then
				fullgiturl="https://$gitcredentials${gitUrl//https:\/\//}"
		   fi
		   if [[ ${gitUrl} == http:* ]]; then
			fullgiturl="http://$gitcredentials${gitUrl//http:\/\//}"
		   fi
		fi
		if [[ -n ${gitSshUrl} && -n ${gitSshKey} && -n ${gitSshDomain} ]]; then
			#Prepare ssh keys
			mkdir -p /home/origam/.ssh
			echo ${gitSshKey} | base64 -d  >/home/origam/.ssh/id_rsa
			chmod 600 /home/origam/.ssh/id_rsa
			ssh-keyscan ${gitSshDomain} >> /home/origam/.ssh/known_hosts
			fullgiturl=${gitSshUrl}
		fi
		   rm -rf $PROJECT_DATA_DIRECTORY
		   mkdir $PROJECT_DATA_DIRECTORY
		   cd $PROJECT_DATA_DIRECTORY
		   git clone $gitcloneBranch --single-branch $fullgiturl

		   if [  "$(ls -A `pwd`)" ]; then
			   ln -s `pwd`/`ls` `pwd`/origam
			   #test custom scripts	
			   cd origam
			   if [ -f custom.js ]; then
					cp custom.js /home/origam/server_bin/assets/identity/js/custom.js
			   fi
			   if [ -f reverse-proxy.conf ]; then
					sudo cp reverse-proxy.conf /etc/nginx/sites-available/reverse-proxy.conf
					sudo /etc/init.d/nginx restart
			   fi
		   fi
		   if [ -f "/home/origam/.ssh/id_rsa" ]; then
				#Remove key
				rm /home/origam/.ssh/id_rsa
		   fi
fi
cd /home/origam/server_bin

DIRCONFIG="configuredata"
if [[ -n ${gitConfPullOnStart} && ${gitConfPullOnStart} == true ]]; then

	   gitconfcredentials=""
	   gitconfcloneBranch="-b master"
	   fullconfgiturl=""
	   if [[ -n ${gitConfBranch} ]]; then
			gitconfcloneBranch="-b $gitConfBranch"
	   fi
	   
		if [[ -n ${gitConfUrl} ]]; then
		   if [[ -n ${gitConfUsername} && -n ${gitConfPassword} ]]; then
				gitconfcredentials="${gitConfUsername}:${gitConfPassword}@"
		   fi
		   if [[ ${gitConfUrl} == https:* ]]; then
				fullconfgiturl="https://$gitconfcredentials${gitConfUrl//https:\/\//}"
		   fi
		   if [[ ${gitConfUrl} == http:* ]]; then
			fullconfgiturl="http://$gitconfcredentials${gitConfUrl//http:\/\//}"
		   fi
		 fi
		 if [[ -n ${gitConfSshUrl} && -n ${gitConfSshKey} && -n ${gitConfSshDomain} ]]; then
			#Prepare ssh keys
			mkdir -p /home/origam/.ssh
			echo ${gitConfSshKey} | base64 -d  >/home/origam/.ssh/id_rsa
			chmod 600 /home/origam/.ssh/id_rsa
			ssh-keyscan ${gitConfSshDomain} >> /home/origam/.ssh/known_hosts
			fullconfgiturl=${gitConfSshUrl}
		fi
	   rm -rf $DIRCONFIG
	   mkdir $DIRCONFIG
	   cd $DIRCONFIG
	   git clone $gitconfcloneBranch --single-branch $fullconfgiturl

       if [  "$(ls -A `pwd`)" ]; then

		   ln -s `pwd`/`ls` `pwd`/origam
		   #need to move to gitRootDirectory everytime
		   cd origam
		   if [ -f _OrigamSettings.template ]; then
			cp _OrigamSettings.template ../../
		   fi
		   if [ -f _appsettings.template ]; then
			cp _appsettings.template ../../
		   fi
		   if [ -f log4net.config ]; then
			cp log4net.config ../../
		   fi
		   if [ -f custom.js ]; then
			cp custom.js /home/origam/server_bin/assets/identity/js/custom.js
		   fi
		   if [ -f reverse-proxy.conf ]; then
				sudo cp reverse-proxy.conf /etc/nginx/sites-available/reverse-proxy.conf
				sudo /etc/init.d/nginx restart
		   fi
	   fi
	   if [ -f "/home/origam/.ssh/id_rsa" ]; then
		#Remove key
		rm /home/origam/.ssh/id_rsa
	   fi
fi

cd /home/origam/server_bin

if [ ! -d "$PROJECT_DATA_DIRECTORY" ]; then
	echo "Server has no model!!! Review the instance setup.";
	exit 1
fi

cp _appsettings.template appsettings.prepare

if [[ ! -z ${ExternalDomain_SetOnStart} ]]; then
	rm -f appsettings.json
	cp appsettings.prepare appsettings.json 
	sed -i "s|ExternalDomain|${ExternalDomain_SetOnStart}|" appsettings.json
fi

if [[ ! -z ${EnableChat} && ${EnableChat} == true ]]; then
	sed -i "s|pathchatapp|/home/origam/server_bin/clients/chat|" appsettings.json
	sed -i "s|chatinterval|10000|" appsettings.json
else
	sed -i "s|pathchatapp||" appsettings.json
	sed -i "s|chatinterval|0|" appsettings.json
fi

ORIGAM_SETTINGS_FILE="OrigamSettings.config"
cp _OrigamSettings.template "$ORIGAM_SETTINGS_FILE"

source fill_origam_settings_config.sh
fill_origam_settings_config "$ORIGAM_SETTINGS_FILE" "${DatabaseType}"

export gitUrl
export gitBranch
export gitUsername
export gitPassword
export gitSshUrl
export gitSshKey
export gitSshDomain
export gitConfUrl
export gitConfUsername
export gitConfPassword
export gitConfUrl
export gitConfSshUrl
export gitConfSshKey
export gitConfSshDomain
export OrigamSettings_DbUsername
export OrigamSettings_DbPassword
