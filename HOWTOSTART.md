# Setting up Development Environment for Origam
The following is a guide to help you set up Origam development environment. You will need it if you wish to contribute to Origam backend or frontend. It will be quite helpful if you are going to develop your own plugin for Origam too.

If you wish to build and debug the Architect, the Server and the client application you will need 
Windows. 

If you have Linux or Mac it is recommended to do the development in a 
virtual Windows machine. You can develop the server and the client application 
outside of Windows but if you wish to run and debug the Architect you will just need 
 Windows.
It is also possible to download the compiled server application 
[here](https://github.com/origam/origam/releases) and just build and debug the 
client application. 

## Download Software
Here is what you have to install first. Make sure you have at least 50 GB of free 
space on you hard drive before you install all the software. That way you should still have enough space to build origam 
and some space left after you finish installing.

#### SQL Database
Install [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)  and [SQL Server Management Studio ](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver15) to work with the database.

#### C# IDE
Install [Visual Studio](https://visualstudio.microsoft.com/downloads/). 
Select **ASP.NET and web development** and **.NET desktop development** during the installation process.

#### Node.js
Make sure you download at least version 18 of [Node.js](https://nodejs.org/en/).

#### Javascript IDE
Either [Visual Studio Code](https://code.visualstudio.com/), [JetBrains Webstorm](https://www.jetbrains.com/webstorm/) or something else.

#### Git
You probably have a [git](https://git-scm.com/) client in your IDE. Just make sure that git is working you will need it do clone Origam repository.

## Clone Origam github Repository 
Get the Origam source by cloning the Origam repository.
```
git clone https://github.com/origam/origam.git
```

## Running the demo project
If you wish to contribute to Origam you will need some project to test your 
modifications/fixes on. If have you own project you can use it if not you can 
run the origam demo project which comes with the source code. 

The demo project is used for automatic testing so there are a lot of features you can use for inspiration when working 
on your own project or for testing and bug fixes when contributing to Origam itself.

First we have to create an empty database. Open **SQL Server Management Studio** Right click on `Databases`
choose `New Database...` name it `origam-demo` and click Ok.

Then open the C# solution in Visual Studio. The solution file is at:
```
backend\Origam.sln
```
### Open the Project in Architect
In Visual Studio set the startup project to  **OrigamArchitect** switch the solution configuration to **Debug Architect** and run the project.

Wait for the Architect to start up. You will be presented with the New project wizard pop up, click Cancel to close it. Next to go `File` &rarr; 
`Connection Configuration...`. There are no configurations as you can see. The status 
bar at the bottom of the application shows path of the loaded settings file.
```
C:\Users\<userName>\AppData\Roaming\ORIGAM\0.0\OrigamSettings.config
```
Open the file in a text editor and replace its contents with this
```xml
<?xml version="1.0" encoding="UTF-8"?>
<OrigamSettings>
  <xmlSerializerSection type="Origam.OrigamSettingsCollection, Origam, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null">
    <ArrayOfOrigamSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <OrigamSettings>
        <ModelSourceControlLocation>C:\Repos\origam\model-tests\model</ModelSourceControlLocation>
        <DataConnectionString>Data Source=.;Initial Catalog=origam-demo;Integrated Security=True;User ID=;Password=;Pooling=True</DataConnectionString>
        <SchemaDataService>Origam.DA.Service.MsSqlDataService, Origam.DA.Service</SchemaDataService>
        <DataDataService>Origam.DA.Service.MsSqlDataService, Origam.DA.Service</DataDataService>
        <SQLReportServiceTimeout>60000</SQLReportServiceTimeout>
        <GUIExcelExportFormat>XLS</GUIExcelExportFormat>
        <DefaultSchemaExtensionId>f17329d6-3143-420a-a2e6-30e431eea51d</DefaultSchemaExtensionId>
        <ExtraSchemaExtensionId>00000000-0000-0000-0000-000000000000</ExtraSchemaExtensionId>
        <TitleText>Demo</TitleText>
        <Name>Demo</Name>
        <DataServiceSelectTimeout>120</DataServiceSelectTimeout>
        <AuthorizationProvider>Origam.Security.OrigamDatabaseAuthorizationProvider, Origam.Security</AuthorizationProvider>
        <ProfileProvider>Origam.Security.OrigamProfileProvider, Origam.Security</ProfileProvider>
        <LoadExternalWorkQueues>false</LoadExternalWorkQueues>
        <ExternalWorkQueueCheckPeriod>180</ExternalWorkQueueCheckPeriod>
        <TraceEnabled>true</TraceEnabled>
        <ModelProvider>Origam.OrigamEngine.FilePersistenceBuilder, Origam.OrigamEngine</ModelProvider>
      </OrigamSettings>
    </ArrayOfOrigamSettings>
  </xmlSerializerSection>
</OrigamSettings>
```
Make sure you set the correct **ModelSourceControlLocation** in the file. Open the 
Architect again. You should see the demo projects' packages. Double click the **Widgets**
package and a **Deployment Scripts Pending** pop up should show up. Click Yes. The database 
should now contain the generated tables and minimal data so the application can start.
Then double click the **Automatic Tests** package
and run the deployment scripts there too. These will add some test data to the generated tables 
so that you have same thing to play with.

Now we add a test user into the database. Open **SQL Server Management Studio** go to
the database **origam-demo** and run the following script
```tsql
DECLARE @userName NVARCHAR(max)
DECLARE @passwordHash NVARCHAR(max)

SET @userName = 'testUser'
SET @passwordHash = '1F4000.AOSufbh0cRh/jXwyPgGDtQI0KbvNHFgSaz2dgVMmsXvdDOFvw++tUOq4hovoSZoeDQ==' -- hash of: origam

INSERT INTO [dbo].[BusinessPartner] (
	UserName
	,Name
	,Id
	)
VALUES (
	@userName
	,@userName
	,'69aacf26-300e-477b-b9a6-408324ca1cad'
	);

INSERT INTO [dbo].[OrigamUser] (
	UserName
	,EmailConfirmed
	,refBusinessPartnerId
	,Password
	,Id
	,FailedPasswordAttemptCount
	,Is2FAEnforced
	)
VALUES (
	@userName
	,1
	,'69aacf26-300e-477b-b9a6-408324ca1cad'
	, @passwordHash
	,'f0154207-4f49-476d-b955-6f587dd61708'
	,0
	,0
	);

INSERT INTO [dbo].[BusinessPartnerOrigamRole] (
	Id
	,refBusinessPartnerId
	,refOrigamRoleId
	)
VALUES (
	'd8130a52-a52c-49bb-b80d-565fa5b9eb21'
	,'69aacf26-300e-477b-b9a6-408324ca1cad'
	,'E0AD1A0B-3E05-4B97-BE38-12FF63E7F2F2'
	);

UPDATE  [dbo].[OrigamParameters]
SET [BooleanValue] = 1
WHERE Id = 'e42f864f-5018-4967-abdc-5910439adc9a'

```

### Build the Client Application
To run the demo project in Origam server you first have to build the client application.
To do that open the folder `frontend-html` in your javascript IDE or CMD.exe and run the following.
```
npm install --global yarn
```
```
yarn 
```
```
yarn build
```
now you should have a production build of the front end application in 
```
frontend-html\dist
```

### Run the Server
After building the client application we can run the server. First create **OrigamSettings.config** for the server application.
Go to Visual Studio set the startup project to **Origam.Server** and switch the solution configuration to **Debug Server**.
Build the project. The debug folder should be created now and you can copy the OrigamSettings.config from the Architect there.

Copy
```
C:\Users\<userName>\AppData\Roaming\ORIGAM\0.0\OrigamSettings.config
```
to
```
backend\Origam.Server\bin\Debug\net6.0
```
Then we have to configure logging by adding the **log4net.config** to the same folder
```
backend\Origam.Server\bin\Debug\net6.0\log4net.config
```
You can customize the file however you want. Here is a basic logging setup
```xml
<?xml version="1.0" encoding="utf-8" ?>
<log4net>
  <appender name="FileAppender" type="log4net.Appender.FileAppender">
    <file value="./logs/OrigamServer.log" />
    <appendToFile value="false" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date [%thread] %level %logger - %message%newline" />
    </layout>
  </appender>
  <root>
    <level value="ERROR"/>
    <appender-ref ref="FileAppender" />
  </root>
</log4net>
```
Next run these commands to create a *.pfx certificate file for JWT tokens. 
```
"C:\Program Files\Git\usr\bin\openssl.exe" req -newkey rsa:2048 -nodes -keyout serverCore.key -x509 -days 365 -out serverCore.cer
```
```
"C:\Program Files\Git\usr\bin\openssl.exe"   pkcs12 -export -in serverCore.cer -inkey serverCore.key -out serverCore.pfx
```
Then put the generated `serverCore.pfx` file to
```
backend\Origam.Server
```
The server needs some additional settings that we can pass in ***appsettings.json*** located 
here 
```
backend\Origam.Server\appsettings.json
```
Create the file and put this in it
```json
{
  "ReloadModelWhenFilesChangesDetected": "true",
  "PathToClientApp": "<AbsolutePathToTheDistFolder>",
  "IdentityGuiConfig": {
    "AllowPasswordReset": "true"
  },
  "CustomAssetsConfig": {
    "PathToCustomAssetsFolder": "<AbsolutePathToFolderWhereYouHaveTheImages>",
    "RouteToCustomAssetsFolder": "/customAssets",
    "IdentityGuiLogoUrl": "/customAssets/avatarTest.png",
    "Html5ClientLogoUrl": "/customAssets/avatarTest.png",
    "FaviconLogoUrl": "/customAssets/faviconTest.png"
  },
  "IdentityServerConfig": {
    "CookieSlidingExpiration": true,
    "PathToJwtCertificate": "serverCore.pfx",
    "PasswordForJwtCertificate": "<PasswordYouEnteredWhenCreatingTheCertificate>",
    "WebClient": {
      "RedirectUris": [
        "https://localhost:44357/#origamClientCallback/",
        "https://localhost:44357/#origamClientCallbackRenew/",
        "https://localhost:5173/#origamClientCallback/",
        "https://localhost:5173/#origamClientCallbackRenew/"
      ],
      "PostLogoutRedirectUris": [
        "https://localhost:44357",
        "https://localhost:5173"
      ]
    },
    "ServerClient": {
      "ClientSecret": "serverSecret"
    }
  },
  "UserLockoutConfig": {
    "MaxFailedAccessAttempts": 3
  },
  "PasswordConfig": {
    "RequireDigit": "false",
    "RequiredLength": "6",
    "RequireNonAlphanumeric": "false",
    "RequireUppercase": "false",
    "RequireLowercase": "true"
  },
  "MailConfig": {
    "Port": 587,
    "UseSsl": "true",
    "PickupDirectoryLocation": "<AbsolutePathToFolderWhereTheEmailsWillBeStored>"
  },
  "UserApiOptions": {
    "RestrictedRoutes": [
      "/api/private"
    ],
    "PublicRoutes": [
      "/api/attachment",
      "/api/public"
    ]
  },
  "ClientFilteringConfig": {
    "CaseSensitive": "false",
    "AccentSensitive": "false"
  },
  "LanguageConfig": {
    "Default": "en-US",
    "Allowed": [
      {
        "Culture": "en-US",
        "Caption": "English",
        "ResetPasswordMailSubject": "Reset Password",
        "ResetPasswordMailBodyFileName": "testResetPwFile.txt"
      }
    ]
  }
}
```
Make sure you set 
- **PathToClientApp** to the absolute path to the
`frontend-html\dist` folder where we built the frontend application before.
- **PathToCustomAssetsFolder** to absolute path to a folder where you keep the application images. The images don't have to be there for development.
- **PickupDirectoryLocation** to absolute path to a folder where the emails will be saved instead 
of sending them to the actual email addresses. This is quite useful for development.
- **PasswordForJwtCertificate** password you entered when creating the `serverCore.pfx` 
file

All back slashes in the paths should be escaped i.e. \\ instead of \.

When you opened the solution Visual Studio created the file 
```
backend\Origam.Server\Properties\launchSettings.json
```
Open the file and set the **https** port to **44357**. You can ignore the http port number.



Now go to Visual Studio and run the Origam.Server project. Everything should work and 
your default browser should open with the login screen. Enter the following:

**Username: testUser**

**Password: origam**

The production build of the client application located at `frontend-html\dist` will run after you login.

### Run the Javascript Development Server
Next we will run the front end javascript application in a development server so that
we can debug it. 
Just open CMD.exe in the `frontend-html` folder and run
```
yarn dev
```
The development server will start and show you the port number where you can reach it.

### Conclusion 
We have configured the Origam development environment. Now you can debug all parts of Origam.

To debug the Architect in Visual Studio set the startup project to **OrigamArchitect** and switch the solution configuration to **Debug Architect**. Then run the project.

If you want to debug the server set the startup project to **Origam.Server** switch the solution configuration to **Debug Server** and run the project.

If you need to debug the front end application in browser run the Origam server (previous paragraph) and start the node development server with `yarn dev`. 
