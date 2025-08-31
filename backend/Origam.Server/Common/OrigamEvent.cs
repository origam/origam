using System;

namespace Origam.Server.Common;

public class OrigamEvent
{
    public static readonly (string Feature, Guid EventId) UserSignIn 
        = ("ORIGAM_EVENT_USER_SIGN_IN", Guid.Empty);
    public static readonly (string Feature, Guid EventId) UserSignedOut 
        = ("ORIGAM_EVENT_USER_SIGN_OUT", Guid.Empty);
    public static readonly (string Feature, Guid EventId) ScreenOpened 
        = ("ORIGAM_EVENT_OPEN_SCREEN", Guid.Empty);
    public static readonly (string Feature, Guid EventId) ExportToExcel 
        = ("ORIGAM_EVENT_EXPORT_TO_EXCEL", Guid.Empty);
    
}