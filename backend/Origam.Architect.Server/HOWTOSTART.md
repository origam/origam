### Run the Server

Go to Visual Studio set the startup project to **Origam.Architect.Server** and switch the solution configuration to **Debug Architect Server**.
Build the project.

Create OrigamSettings.config file to
```
backend\Origam.Architect.Server\bin\Debug\net8.0
```
Content must contain the following, but make sure you set the correct value for **ModelSourceControlLocation**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<OrigamSettings>
  <xmlSerializerSection type="Origam.OrigamSettingsCollection, Origam, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null">
    <ArrayOfOrigamSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <OrigamSettings>
        <ModelSourceControlLocation>C:\Repos\origam\model-tests\model</ModelSourceControlLocation>
        <DataConnectionString>Data Source=.;Initial Catalog=origam-demo;Integrated Security=True;User ID=;Password=;Pooling=True; Encrypt= False</DataConnectionString>
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
Then we have to configure logging by adding the **log4net.config** to the same folder
```
backend\Origam.Architect.Server\bin\Debug\net8.0\log4net.config
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
Create ***appsettings.json*** 
```
backend\Origam.Architect.Server\appsettings.json
```
```json
{
  "SpaConfig": {
    "PathToClientApplication": "C:\Repos\origam\architect-html"
  }
}
```
Make sure you set the correct **PathToClientApplication** in the file.

All back slashes in the paths should be escaped i.e. \\ instead of \.

### Run the Javascript Development Server
Next we will run the front end javascript application in a development server so that
we can debug it. 
Just open CMD.exe in the `architect-html` folder and run
```
yarn dev
```
The development server will start and show you the port number where you can reach it.