#!/bin/bash
cd /home/origam/HTML5
DIR="data"
if [[ -n ${gitPullOnStart} && ${gitPullOnStart} == true ]]; then
	if [[ -n ${gitUrl} ]]; then
	   rm -rf $DIR
	   mkdir $DIR
	   cd $DIR
	   gitcredentials=""
	   if [[ -n ${gitUsername} && -n ${gitPassword} ]]; then
			gitcredentials="${gitUsername}:${gitPassword}@"
	   fi
	   if [[ ${gitUrl} == https:* ]]; then
			fullgiturl="https://$gitcredentials${gitUrl//https:\/\//}"
		git clone $fullgiturl
	   fi
	   if [[ ${gitUrl} == http:* ]]; then
		fullgiturl="http://$gitcredentials${gitUrl//http:\/\//}"
		git clone $fullgiturl
	   fi
	   if [[ -n ${gitBranch} ]]; then
	    cd `ls`
	    git checkout ${gitBranch}
		cd ..
	   fi
	   cd ..
	fi
fi

if [ ! "$(ls -A $DIR)" ]; then 
	echo “Server has no model!!!! Please set up with GIT.”;
	echo "Mandatory: gitPullOnStart(true)"
	echo "Mandatory: gitUrl(ie:https://github.com/user/HelloWord.git)"
	echo "Optional: gitUsername, gitPassword"
	exit 1
fi

if [ ! -f "serverCore.key" ]; then
	openssl rand -base64 10 >certpass
	openssl req -batch -newkey rsa:2048 -nodes -keyout serverCore.key -x509 -days 728 -out serverCore.cer
	openssl pkcs12 -export -in serverCore.cer -inkey serverCore.key -passout file:certpass -out /home/origam/HTML5/serverCore.pfx
	cp _appsettings.template appsettings.prepare
        sed -i "s|certpassword|$(cat certpass)|" appsettings.prepare
fi
KEYEAR=`ls -l --time-style=+%Y serverCore.key | cut -d" " -f6`

if [ `date "+%Y"` -gt $KEYEAR ]; then
	rm serverCore.key
	rm serverCore.cer
	rm serverCore.pfx
	openssl rand -base64 10 >certpass
	openssl req -batch -newkey rsa:2048 -nodes -keyout serverCore.key -x509 -days 728 -out serverCore.cer
	openssl pkcs12 -export -in serverCore.cer -inkey serverCore.key -passout file:certpass -out /home/origam/HTML5/serverCore.pfx
	cp _appsettings.template appsettings.prepare
	sed -i "s|certpassword|$(cat certpass)|" appsettings.prepare
fi

if [[ ! -z ${ExternalDomain_SetOnStart} ]]; then
	rm -f appsettings.json
	cp appsettings.prepare appsettings.json 
	sed -i "s|ExternalDomain|${ExternalDomain_SetOnStart}|" appsettings.json
fi

if [[ ! -z ${EnableChat} && ${EnableChat} == true ]]; then
	sed -i "s|pathchatapp|/home/origam/HTML5/clients/chat|" appsettings.json
	sed -i "s|chatinterval|1000|" appsettings.json
else
	sed -i "s|pathchatapp||" appsettings.json
	sed -i "s|chatinterval|0|" appsettings.json
fi

if [[ -n ${OrigamSettings_SetOnStart} && ${OrigamSettings_SetOnStart} == true ]]; then
	rm -f OrigamSettings.config
fi
if [[ ! -f "OrigamSettings.config" ]]; then
	if [[ -z ${OrigamSettings_SchemaExtensionGuid} || -z ${OrigamSettings_DbHost}  || -z ${OrigamSettings_DbPort}  || -z ${OrigamSettings_DbUsername}  || -z ${OrigamSettings_DbPassword}  || -z ${DatabaseName}  || -z ${OrigamSettings_ModelName}  ]];then
		echo "OrigamSettings.config not exists!!"
		echo "one or more variables are undefined"
		echo "Please check if environment variables are set properly."
		echo "OrigamSettings_SchemaExtensionGuid, OrigamSettings_DbHost, OrigamSettings_DbPort, OrigamSettings_DbUsername, OrigamSettings_DbPassword, DatabaseName,OrigamSettings_ModelName"
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
		sed -i "s/OrigamSettings_DatabaseName/${DatabaseName}/" OrigamSettings.config
		sed -i "s|OrigamSettings_ModelName|data\/${OrigamSettings_ModelName}${OrigamSettings_ModelSubDirectory}|" OrigamSettings.config
		sed -i "s/OrigamSettings_Title/${OrigamSettings_ModelName}/" OrigamSettings.config
		sed -i "s|OrigamSettings_ReportDefinitionsPath|${OrigamSettings_ReportDefinitionsPath}|" OrigamSettings.config
	fi
fi
export ASPNETCORE_URLS="http://+:8080"
dotnet Origam.ServerCore.dll

