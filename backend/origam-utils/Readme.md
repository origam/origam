## Run project under linux

Install dotnet on linux

First install [dotnet](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x)

then on console run 

apt-get install dotnet-sdk-2.1



#### *Microsoft Visual Studio*

> Setup Publish
> Target Framework: netcoreapp2.1
> Target Runtime: Portable
> Target Location: your path



Then Click on **Publish**

Copy Target Location    to    directory on Linux.

Configure OrigamSettings.config:

```OrigamSettings.config
<ModelSourceControlLocation>path to project</ModelSourceControlLocation>	<DefaultSchemaExtensionId>set guid of package</DefaultSchemaExtensionId>
```

If you need setup specific language :

```OrigamSettings.config
<LocalizationFolder>path to localizationFolder</LocalizationFolder>
```



Then run :

dotnet  origam-utils.dll  