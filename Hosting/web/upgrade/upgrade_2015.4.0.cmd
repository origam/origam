@ECHO OFF
ECHO Removing obsoletes files and fixing file pathes...
ECHO Moving AC_OETags.js to js folder
MOVE /Y ..\AC_OETags.js ..\js\AC_OETags.js
ECHO Removing AttachUser.aspx
DEL /F ..\AttachUser.aspx
ECHO Removing CrateAndAttachUser.aspx
DEL /F ..\CreateAndAttachUser.aspx
ECHO Removing CrateNewCompany.aspx
DEL /F ..\CreateNewCompany.aspx
ECHO Removing FullScreen.aspx
DEL /F ..\FullScreen.aspx
ECHO Removing Gate.ashx
DEL /F ..\Gate.ashx
ECHO Removing ChangePassword.aspx
DEL /F ..\ChangePassword.aspx
ECHO Removing InternalUtils.asmx
DEL /F ..\InternalUtils.asmx
ECHO Removing Login.aspx
DEL /F ..\Login.aspx
ECHO Moving mousetrap.min.js to js folder
MOVE /Y ..\mousetrap.min.js ..\js\mousetrap.min.js
ECHO Removing PrecompiledApp.config
DEL /F ..\PrecompiledApp.config
ECHO Removing Recover.aspx
DEL /F ..\Recover.aspx
ECHO Removing Register.aspx
DEL /F ..\Register.aspx
ECHO Removing RegisterDone.aspx
DEL /F ..\RegisterDone.aspx
ECHO Removing VerifyNewUser.aspx
DEL /F ..\VerifyNewUser.aspx
ECHO Removing compiled files
DEL /F ..\bin\*.compiled
ECHO Removing ORIGAMHosting.dll
DEL /F ..\bin\ORIGAMHosting.dll
ECHO Removing ORIGAMHosting resources
DEL /F ..\bin\cs-CZ\ORIGAMHosting.resources.dll
DEL /F ..\bin\de-DE\ORIGAMHosting.resources.dll
ECHO Removing handlers
DEL /F ..\*.ashx
ECHO Removing Error pages
DEL /F ..\Error.cs-CZ.html
DEL /F ..\Error.en-US.html
ECHO Removing Gateway.aspx
DEL /F ..\Gateway.aspx
ECHO Removing Global.asax
DEL /F ..\Global.asax
ECHO Removing Portal.aspx
DEL /F ..\Portal.aspx
ECHO Cleaning App_Code folder
IF EXIST ..\App_Code\ (
DEL /F ..\App_Code\OrigamHostingException.cs
DEL /F ..\App_Code\ConfigUtils.cs
DEL /F ..\App_Code\Mailing.cs
DEL /F ..\App_Code\PathResolver.cs
)
IF EXIST ..\App_Code\WS (
RD /S /Q ..\App_Code\WS
)
ECHO Cleaning App_GlobalResources
IF EXIST ..\App_GlobalResources (
RD /S /Q ..\App_GlobalResources
)
ECHO Removing Crystal Reports files
DEL /F bin\CrystalDecisions*.*
ECHO Operation finished.
PAUSE
