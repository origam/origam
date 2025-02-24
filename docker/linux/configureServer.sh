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
DIR="projectData"
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
		   rm -rf $DIR
		   mkdir $DIR
		   cd $DIR
		   git clone $gitcloneBranch --single-branch $fullgiturl

		   if [  "$(ls -A `pwd`)" ]; then
			   ln -s `pwd`/`ls` `pwd`/origam
			   #test custom scripts	
			   cd origam
			   if [ -f custom.js ]; then
					cp custom.js /home/origam/HTML5/assets/identity/js/custom.js
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
cd /home/origam/HTML5

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
		   if [ -f _OrigamSettings.mssql.template ]; then
			cp _OrigamSettings.mssql.template ../../
		   fi
		   if [ -f _OrigamSettings.postgres.template ]; then
			cp _OrigamSettings.postgres.template ../../
		   fi
		   if [ -f _appsettings.template ]; then
			cp _appsettings.template ../../
		   fi
		   if [ -f log4net.config ]; then
			cp log4net.config ../../
		   fi
		   if [ -f custom.js ]; then
			cp custom.js /home/origam/HTML5/assets/identity/js/custom.js
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

cd /home/origam/HTML5

if [ ! -d "$DIR" ]; then
	echo “Server has no model!!! Review the instance setup.”;
	echo "Mandatory: gitPullOnStart(true)"
	echo "Mandatory: gitUrl(ie:https://github.com/user/HelloWord.git)"
	echo "Mandatory: gitUrl(ie:https://github.com/user/HelloWord.git)"
	echo "Optional: gitUsername, gitPassword"
	exit 1
fi
# generate certificate every start.
	openssl rand -base64 10 >certpass
	run_silently openssl req -batch -newkey rsa:2048 -nodes -keyout serverCore.key -x509 -days 728 -out serverCore.cer
	openssl pkcs12 -export -in serverCore.cer -inkey serverCore.key -passout file:certpass -out /home/origam/HTML5/serverCore.pfx
	cp _appsettings.template appsettings.prepare
    sed -i "s|certpassword|$(cat certpass)|" appsettings.prepare


if [[ ! -z ${ExternalDomain_SetOnStart} ]]; then
	rm -f appsettings.json
	cp appsettings.prepare appsettings.json 
	sed -i "s|ExternalDomain|${ExternalDomain_SetOnStart}|" appsettings.json
fi

if [[ ! -z ${EnableChat} && ${EnableChat} == true ]]; then
	sed -i "s|pathchatapp|/home/origam/HTML5/clients/chat|" appsettings.json
	sed -i "s|chatinterval|10000|" appsettings.json
else
	sed -i "s|pathchatapp||" appsettings.json
	sed -i "s|chatinterval|0|" appsettings.json
fi

if [[ -n ${OrigamSettings_SetOnStart} && ${OrigamSettings_SetOnStart} == true ]]; then
	rm -f OrigamSettings.config
fi
if [[ ! -f "OrigamSettings.config" ]]; then
	if [[ -z ${OrigamSettings_SchemaExtensionGuid} || -z ${OrigamSettings_DbHost}  || -z ${OrigamSettings_DbPort}  || -z ${OrigamSettings_DbUsername}  || -z ${OrigamSettings_DbPassword}  || -z ${DatabaseName}  ]];then
		echo "OrigamSettings.config not exists!!"
		echo "one or more variables are undefined"
		echo "Please check if environment variables are set properly."
		echo "OrigamSettings_SchemaExtensionGuid, OrigamSettings_DbHost, OrigamSettings_DbPort, OrigamSettings_DbUsername, OrigamSettings_DbPassword, DatabaseName"
		exit 1
	fi
	if [[ -z ${DatabaseType} ]]; then
		echo "Please set Type of Database (mssql/postgresql)"
		exit 1
	else
		if [[ ${DatabaseType} == mssql ]]; then
			cp _OrigamSettings.mssql.template OrigamSettings.config
		fi
		if [[ ${DatabaseType} == postgresql ]]; then
			cp _OrigamSettings.postgres.template OrigamSettings.config
		fi
		if [[ ! -f "OrigamSettings.config" ]]; then
			echo "Please set 'DatabaseType' Type of Database (mssql/postgresql)"
			exit 1
		fi
		sed -i "s/OrigamSettings_SchemaExtensionGuid/${OrigamSettings_SchemaExtensionGuid}/" OrigamSettings.config
		sed -i "s/OrigamSettings_DbHost/${OrigamSettings_DbHost}/" OrigamSettings.config
		sed -i "s/OrigamSettings_DbPort/${OrigamSettings_DbPort}/" OrigamSettings.config
		sed -i "s/OrigamSettings_DbUsername/${OrigamSettings_DbUsername}/" OrigamSettings.config
		sed -i "s/OrigamSettings_DbPassword/${OrigamSettings_DbPassword}/" OrigamSettings.config
		sed -i "s/OrigamSettings_DatabaseName/${DatabaseName}/" OrigamSettings.config
		sed -i "s|OrigamSettings_ModelSourceControlLocation|/home/origam/projectData/model|" OrigamSettings.config
		sed -i "s/OrigamSettings_Title/${OrigamSettings_Title}/" OrigamSettings.config
		sed -i "s|OrigamSettings_ReportDefinitionsPath|${OrigamSettings_ReportDefinitionsPath}|" OrigamSettings.config
		sed -i "s|OrigamSettings_RuntimeModelConfigurationPath|${OrigamSettings_RuntimeModelConfigurationPath}|" OrigamSettings.config
	fi
fi
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
./updateEnvironment.sh
sudo ./updateEnvironmentRoot.sh
