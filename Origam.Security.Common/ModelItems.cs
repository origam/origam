using System;

namespace Origam.Security.Common
{
    public static class ModelItems
    {
        public static readonly Guid ORIGAM_USER_DATA_STRUCTURE
            = new Guid("43b67a51-68f3-4696-b08d-de46ae0223ce");
        public static readonly Guid GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID
            = new Guid("982f45a9-b610-4e2f-8d7f-2b1eebe93390");
        public static readonly Guid GET_ORIGAM_USER_BY_USER_NAME
            = new Guid("a60c9817-ae18-465c-a91f-d4b8a25f15a4");
        public static readonly Guid LOOKUP_ORIGAM_USER_SECURITY_STAMP_BY_BUSINESSPARTNER_ID
            = new Guid("918a69c4-094c-456b-8ed7-af854b6b612f");
        public static readonly Guid CREATE_USER_WORKFLOW 
            = new Guid("2bd4dbcc-d01e-4c5d-bedb-a4150dcefd54");
    }
}