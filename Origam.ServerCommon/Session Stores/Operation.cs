namespace Origam.Server
{
    public enum Operation
    {
       DeleteAllData =-2, 
       Delete = -1, 
       Update = 0, 
       Create =1,
       FormSaved = 2,
       FormNeedsRefresh = 3,
       CurrentRecordNeedsUpdate = 4,
       RefreshPortal = 5
    }
}