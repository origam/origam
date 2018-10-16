<%@ Page Language="C#" %>
<script runat="server">
    private string GetFixtureUrl()
    {
        return Request.Params.Get("fixtureFile") + "?runId=" + Guid.NewGuid().ToString();
    } 
</script>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN"
        "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
    <title>Fixture Runner</title>
    <meta http-equiv="cache-control" content="no-cache">
    <meta http-equiv="expires" content="0">
    <meta http-equiv="pragma" content="no-cache">
    <meta http-equiv="Content-type" content="text/html;charset=UTF-8"/>
    <link rel="stylesheet" href="css/qunit-1.11.0.css">
    <script src="js/AC_OETags.js" type="text/javascript"></script>
    <style type="text/css">
        body { margin: 0px;}
    </style>
    <script language="JavaScript" type="text/javascript">
        <!--
        // -----------------------------------------------------------------------------
        // Globals
        // Major version of Flash required
        var requiredMajorVersion = 11;
        // Minor version of Flash required
        var requiredMinorVersion = 1;
        // Minor version of Flash required
        var requiredRevision = 0;
        // -----------------------------------------------------------------------------
        // -->
    </script>
</head>
<body>
<div id="qunit"></div>
<div id="qunit-fixture"></div>
<div style="height:800px;">
    <script language="JavaScript" type="text/javascript">
        <!--
        // Version check for the Flash Player that has the ability to start Player Product Install (6.0r65)
        var hasProductInstall = DetectFlashVer(6, 0, 65);

        // Version check based upon the values defined in globals
        var hasRequestedVersion = DetectFlashVer(requiredMajorVersion, requiredMinorVersion, requiredRevision);

        if (hasProductInstall && !hasRequestedVersion) {
            // DO NOT MODIFY THE FOLLOWING FOUR LINES
            // Location visited after installation is complete if installation is required
            var MMPlayerType = (isIE == true) ? "ActiveX" : "PlugIn";
            var MMredirectURL = window.location;
            document.title = document.title.slice(0, 47) + " - Flash Player Installation";
            var MMdoctitle = document.title;

            AC_FL_RunContent(
                    "src", "playerProductInstall",
                    "FlashVars", "MMredirectURL=" + MMredirectURL + '&MMplayerType=' + MMPlayerType + '&MMdoctitle=' + MMdoctitle + "",
                    "width", "100%",
                    "height", "100%",
                    "align", "middle",
                    "id", "playerInstall",
                    "quality", "high",
                    "bgcolor", "#382210",
                    "name", "playerInstall",
                    "allowScriptAccess", "sameDomain",
                    "type", "application/x-shockwave-flash",
                    "pluginspage", "http://www.adobe.com/go/getflashplayer"
            );
        } else if (hasRequestedVersion) {
            // if we've detected an acceptable version
            // embed the Flash Content SWF when all tests are passed
            AC_FL_RunContent(
                    "src", "TestPortal?culture=cs_CZ",
                    "width", "100%",
                    "height", "100%",
                    "align", "middle",
                    "id", "portal",
                    "quality", "high",
                    "bgcolor", "#382210",
                    "name", "portal",
                    "wmode", "opaque",
                    "allowScriptAccess", "sameDomain",
                    "type", "application/x-shockwave-flash",
                    "pluginspage", "http://www.adobe.com/go/getflashplayer"
            );
        } else {  // flash is too old or we can't detect the plugin
            var alternateContent = 'Alternate HTML content should be placed here. '
                    + 'This content requires the Adobe Flash Player. '
                    + '<a href=http://www.adobe.com/go/getflash/>Get Flash</a>';
            document.write(alternateContent);  // insert non-flash content
        }
        // -->
    </script>
</div>
<script src="js/qunit-1.11.0.js"></script>
<script src="js/jquery-1.9.1.min.js"></script>
<script src="<%=GetFixtureUrl() %>"></script>
</body>
</html>
