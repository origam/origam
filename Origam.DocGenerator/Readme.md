## Run project under linux

Install dotnet on linux

First install [dotnet](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x)

then on console run 

apt-get install dotnet-sdk-2.1



#### *Microsoft Visual Studio*

> Setup Publish
> Target Framework: netstandard2.0
> Target Runtime: Portable
> Target Location: your path



Then Click on **Publish**



Copy Target Location    to    directory on Linux.



create into directory file  Origam.DocGenerator.runtimeconfig.json

inside put this text:

```
 {
   "runtimeOptions": {
     "framework": {
      "name": "Microsoft.NETCore.App",
       "version": "2.1.0"
       }
     }
  }
```

**Save**

Then run inside of directory:

dotnet Origam.DocGenerator.dll 