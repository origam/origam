#!/bin/bash
cd /home/origam/HTML5
DIR="data"

# generate certificate every start.
	openssl rand -base64 10 >certpass
	openssl req -batch -newkey rsa:2048 -nodes -keyout serverCore.key -x509 -days 728 -out serverCore.cer  > /dev/null 2>&1
	openssl pkcs12 -export -in serverCore.cer -inkey serverCore.key -passout file:certpass -out /home/origam/HTML5/serverCore.pfx  > /dev/null 2>&1
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
		echo "OrigamSettings.config does not exist!!"
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
		sed -i "s/OrigamSettings_DatabaseName/${DatabaseName}/" OrigamSettings.config
		sed -i "s|OrigamSettings_ModelName|data\/origam${OrigamSettings_ModelSubDirectory}|" OrigamSettings.config
		sed -i "s/OrigamSettings_Title/${OrigamSettings_Title}/" OrigamSettings.config
		sed -i "s|OrigamSettings_ReportDefinitionsPath|${OrigamSettings_ReportDefinitionsPath}|" OrigamSettings.config
		sed -i "s|OrigamSettings_RuntimeModelConfigurationPath|${OrigamSettings_RuntimeModelConfigurationPath}|" OrigamSettings.config
	fi
fi

