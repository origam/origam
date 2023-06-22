# Origam
This repository contains main code for [Origam](https://www.origam.com).
## Contributing
See [the contributing guide](CONTRIBUTING.md) for detailed instructions on how to get involved with [Origam](https://www.origam.com).
## Repository Structure
* `backend` - C# code for backend part of the Origam stack (`Architect`, `Client`, `server`, `origam-utils`, `scheduler`)
* `build` - build scripts for CI/CD
* `docker` - resources for Origam Docker images
* `frontend-html` - typescript/react code for frontend part of the Origam stack
* `model-root` - basic Origam model packages
* `model-tests` - model packages used during integration tests
* `test` - resources for end-to-end tests
## License
Origam is licensed under a [GPL-3.0 license](LICENSE).


# **Creating development environment for Origam.**

## :rotating_light: Step 1. Download software.
- Visual Studio       [Download Page](https://visualstudio.microsoft.com/downloads/).
- Visual Studio Code  [Download Page](https://code.visualstudio.com/).
- JetBrains Rider     [Download Page](https://www.jetbrains.com/rider/). [Paid with 30days trial]
- Node.js             [Download Page](https://nodejs.org/en/).

- MsSQL (For Local Database)[Download Page](https://go.microsoft.com/fwlink/?linkid=866662).
- SQL Server Management Studio (To work with databases) [Download Page](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver15).

## :rotating_light: Step 2. Clone and download source files from GitHub: 
```
Origam-origam => Clone and Open as Solution.
```
                 
## :rotating_light: Step 3. Clone Origam-origam git using IDEs tools.

>Rider
- Open Rider > Get from VCS > Paste URL of git to URL field (Choose Directory of local Repository)

>Visual Studio
- Open VS and use Clone from Repository

>Visual Studio Code
- On get Started > Clone Git Repository...

## :rotating_light: Step 4. SDKs and Packages/Tools

We can easily use Visual Studio Installer for that.
When installing Visual Studio, it will show us which developer packages we want to install.
Find and choose these two.

- .NET 5
- ASP.NET

## :rotating_light: Step 5. Install MsSQL Server and SQL Server Management Studio
```
Use Origam.Utils: with these parameters create-demo -n 'name' -p 'password'.
```

## :rotating_light: Step 6. Modify OrigamiSettings.config

Folder is in: 
```
YourRepositoryFolder\origam-origam\Origam.ServerCore\bin\Debug\net5.0
```

<details><summary>Show Settings</summary>
<p>

```
Line DataConnectionString

<DataConnectionString>Data Source=.;Initial Catalog=bes;Integrated Security=True;User ID=;Password=;Pooling=True</DataConnectionString>

Line DefaultSchemaExtensionId

<DefaultSchemaExtensionId>f17329d6-3143-420a-a2e6-30e431eea51d</DefaultSchemaExtensionId>
```

</p>
</details>

<details><summary>Show Settings</summary>
<p>

```
<?xml version="1.0" encoding="UTF-8"?>
<OrigamSettings>
  <xmlSerializerSection type="Origam.OrigamSettingsCollection, Origam, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null">
    <ArrayOfOrigamSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <OrigamSettings>
        <BaseFolder>C:\Users\jindr\Documents\Source\Repos\Origam2\origam-source\Origam.ServerCore\bin\Debug\netcoreapp2.1\</BaseFolder>
        <SchemaConnectionString />
        <ModelSourceControlLocation>C:\Users\Sapphire\Desktop\origam-demo-master\model</ModelSourceControlLocation>
		<DataConnectionString>Data Source=.;Initial Catalog=origam-demo;Integrated Security=True;User ID=;Password=;Pooling=True</DataConnectionString>        
        <SchemaDataService>Origam.DA.Service.MsSqlDataService, Origam.DA.Service</SchemaDataService>
        <DataDataService>Origam.DA.Service.MsSqlDataService, Origam.DA.Service</DataDataService>
        <SecurityDomain />
        <ReportConnectionString />
        <PrintItServiceUrl />
        <SQLReportServiceUrl />
        <SQLReportServiceAccount />
        <SQLReportServicePassword />
        <SQLReportServiceTimeout>60000</SQLReportServiceTimeout>
        <GUIExcelExportFormat>XLS</GUIExcelExportFormat>
        <DefaultSchemaExtensionId>f17329d6-3143-420a-a2e6-30e431eea51d</DefaultSchemaExtensionId>
        <ExtraSchemaExtensionId>00000000-0000-0000-0000-000000000000</ExtraSchemaExtensionId>
        <TitleText>origam-demo</TitleText>
        <Slogan />
        <Name>origam-demo</Name>
        <LocalizationFolder />
        <TranslationBuilderLanguages />
        <HelpUrl>https://www.merriam-webster.com/dictionary/help</HelpUrl>
        <DataServiceSelectTimeout>120</DataServiceSelectTimeout>
        <AuthorizationProvider>Origam.Security.OrigamDatabaseAuthorizationProvider, Origam.Security</AuthorizationProvider>
        <ProfileProvider>Origam.Security.OrigamProfileProvider, Origam.Security</ProfileProvider>
        <LoadExternalWorkQueues>true</LoadExternalWorkQueues>
        <ExternalWorkQueueCheckPeriod>180</ExternalWorkQueueCheckPeriod>
        <ModelProvider>Origam.OrigamEngine.FilePersistenceBuilder, Origam.OrigamEngine</ModelProvider>
      </OrigamSettings>
    </ArrayOfOrigamSettings>
  </xmlSerializerSection>
</OrigamSettings>
```

</p>
</details>

## :rotating_light: Step 7. Modify appsettings.json (secrets.json)

U will find this thanks to Visual Studio Manage User Secrets.

<details><summary>Show example picture.</summary>
<p>

![This is an image](https://fv2-2.failiem.lv/thumb_show.php?i=tzjuskzs3&view)

</p>
</details>

### Paste this settings:

<details><summary>Show Settings</summary>
<p>

```
{
  "PathToClientApp": "C:\\Repos\\origamclient\\origam-html\\build",
  "ChatConfig": {
    "PathToChatApp": "",
    "ChatRefreshInterval": 1000
  },
  "ReloadModelWhenFilesChangesDetected": "false",
  "UserConfig": {
    "FromAddress": "admin@localhost",
    "NewUserRoleId": "",
    "UserUnlockNotificationSubject": "",
    "UserUnlockNotificationBodyFileName": "",
    "UserRegistrationMailSubject": "Register",
    "UserRegistrationMailBodyFileName": "testNewUserFile.txt",
    "MultiFactorMailSubject": "Register",
    "MultiFactorMailBodyFileName": "testMultiFactorFile.txt",
    "MailQueueName": "",
    "UserRegistrationAllowed": "true"
  },
  "IdentityGuiConfig": {
    "AllowPasswordReset": "false"
  },
  "CustomAssetsConfig": {
    //"PathToCustomAssetsFolder": "C:\\someDirectory",
    "RouteToCustomAssetsFolder": "/customAssets",
    "IdentityGuiLogoUrl": "/customAssets/someFile1.png",
    "Html5ClientLogoUrl": "/customAssets/someFile2.png"
  },
  "IdentityServerConfig": {
    "PathToJwtCertificate": "serverCore.pfx",
    "PasswordForJwtCertificate": "bla",
    "UseGoogleLogin": "false",
    "GoogleClientId": "",
    "GoogleClientSecret": "",
    "WebClient": {
      "RedirectUris": [
        "https://localhost:3000/#origamClientCallback/",
        "https://localhost:44356/#origamClientCallback/",
        "http://localhost:3000/#origamClientCallback/",
        "https://localhost:3000/#origamClientCallbackRenew/"
      ],
      "PostLogoutRedirectUris": [ "/", "https://192.168.0.80:45455" ]
    },
    "MobileClient": {
      "RedirectUris": [ "http://localhost/xamarincallback" ],
      "ClientSecret": "mobileSecret",
      "PostLogoutRedirectUris": [ "/", "https://192.168.0.80:45455" ]
    },
    "ServerClient": {
      "ClientSecret": "serverSecret"
    }
  },
  "UserLockoutConfig": {
    "LockoutTimeMinutes": 5,
    "MaxFailedAccessAttempts": 5
  },
  "PasswordConfig": {
    "RequireDigit": "false",
    "RequiredLength": "6",
    "RequireNonAlphanumeric": "false",
    "RequireUppercase": "false",
    "RequireLowercase": "true"
  },
  "MailConfig": {
    "UserName": "",
    "Password": "",
    "Server": "",
    "Port": 587,
    "UseSsl": "true",
    "PickupDirectoryLocation": "C:\\directoryToSaveTheEmailsTo"
  },
  "urls": "https://localhost:44356;http://localhost:5000",
  "UserApiOptions": {
    "RestrictedRoutes": [
      "/api/private"
    ],
    "PublicRoutes": [
      "/api/attachment"
    ]
  },
  "SoapAPI": {
    "Enabled": "false",
    "RequiresAuthentication": "true",
    "ExpectAndReturnOldDotNetAssemblyReferences": "true"
  },
  "BehindProxy": "false",
  "ClientFilteringConfig": {
    "CaseSensitive": "false",
    "AccentSensitive": "true"
  },
  "LanguageConfig": {
    "Default": "en-US",
    "Allowed": [
      {
        "Culture": "en-US",
        "Caption": "English",
        "ResetPasswordMailSubject": "Reset Password",
        "ResetPasswordMailBodyFileName": "testResetPwFile.txt",
        "DateCompleterConfig": {
          "DateSeparator": ".",
          "TimeSeparator": ":",
          "DateTimeSeparator": " ",
          "DateSequence": "MonthDayYear"
        },
        "DefaultDateFormats": {
          "Short": "MM/dd/yyyy",
          "Long": "MM/dd/yyyy HH:mm:ss",
          "Time": "HH:mm:ss"
        }
      },
      {
        "Culture": "cs-CZ",
        "Caption": "Česky",
        "ResetPasswordMailSubject": "Obnova Hesla",
        "ResetPasswordMailBodyFileName": "testResetPwFile.txt"
      },
      {
        "Culture": "de-DE",
        "Caption": "Deutsch",
        "ResetPasswordMailSubject": "Passwort Zurücksetzen",
        "ResetPasswordMailBodyFileName": "testResetPwFile.txt"
      }
    ]
  },
  "HtmlClientConfig": {
    "ShowToolTipsForMemoFieldsOnly": "false"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.*": "Warning"
    }
  }
}
```
</p>
</details>

Is important to set up path to hmtlclient, first line :
```
"PathToClientApp": "C:\\Repos\\origamclient\\origam-html\\build",

Modify path to your path of hmtl client build.
```


## :rotating_light: Step 8. Create and modify log4net.config.
```
yourRepositoryFolder\\origam-source\Origam.ServerCore\bin\Debug\net5.0\
```
<details><summary>Show Settings</summary>
<p>

</p>
</details>

## :rotating_light: Step 9. Download  ServerCore.pfx Certificate from Origam Core Team

Paste it into your repository folder, so should look like this: 
```
yourRepositoryFolder\origam-source\Origam.ServerCore
```
## :rotating_light: Step 10. Running the Client.

Compile the html client application. First clone repository GitHub - origam/origam-html: ORIGAM HTML Client application. somewhere on your machine and install latest version of Node.js.

Then open the command line in the repo’s root directory and run:
```
npm install -g yarn
```
this will install the yarn package manager. Next run
```
yarn install
```
This will download and install all dependencies.

We will run the client application on a nodejs development server to make debugging simple. Before we do that we will also need an origam server to connect to and get the data from. You can use any running origam server you have access to installed in IIS or in docker, local or remote. Just note the address and port the server is running on and write it to an environment variable WDS_PROXY_TARGET.

Now you can run the project with this command
```
yarn start
```
a development server should be started at https://localhost:3000/ when you go to that address you should see the client application.


## :rotating_light: Step 11. Running Debugging.
Make sure of these tasks complete.
- [x] Running DB server (After MsSQL installation it runs in backround)
- [x] Restored test database to demo-database 

<details><summary>Show Example Image</summary>
<p>
	
![image](https://user-images.githubusercontent.com/32484607/138682182-280f7245-61dd-4c52-a81c-5a82936f4183.png)
</p>
	
</details>




- [x] Start a debug of Server.Core in Visual Studio or Rider 

<details><summary>Show Example Image</summary>
<p>
	

![image](https://user-images.githubusercontent.com/32484607/138683171-ecb46caa-eda9-46e9-8419-bd8b38d84206.png)
	
</p>
	
</details>



- [x] It should look like this after starting server.core.

<details><summary>Show Example Image</summary>
<p>
	
![image](https://user-images.githubusercontent.com/32484607/138681887-264b622c-ca1d-4cf2-ab4a-ea6caccfd48d.png)
	
</p>
	
</details>



# :stop_sign: Common Problems.
System.Exception: Could not find instance with id: f17329d6-3143-420a-a2e6-30e431eea51d
This is caused by the wrong path to a demo-model.


              
