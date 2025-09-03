using System;

namespace Origam.Server.Common;

public static class OrigamEvent
{
    public static readonly Guid DataStructureId 
        = new Guid("c35a0893-1b41-4a4a-a70d-795a088957ae");
    
    public static readonly (string FeatureCode, Guid EventId) SignIn 
        = ("EVENT_SIGN_IN", 
            new Guid("3d91cd2c-1265-4022-8834-150de0237a3c"));
    public static readonly (string FeatureCode, Guid EventId) SignOut 
        = ("EVENT_SIGN_OUT", 
            new Guid("b737ea8b-1cda-42d1-845a-c0c909386389"));
    public static readonly (string FeatureCode, Guid EventId) OpenScreen 
        = ("EVENT_OPEN_SCREEN", 
            new Guid("4c14c1da-797b-4f0f-b6df-f787ca7e9647"));
    public static readonly (string FeatureCode, Guid EventId) ExportToExcel 
        = ("EVENT_EXPORT_TO_EXCEL", 
            new Guid("62f161e9-b817-4631-93e3-360551257dc2"));
}