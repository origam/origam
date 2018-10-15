using System;

namespace Origam.DA
{
    public class MergeParams
    {
        public bool TrueDelete { get; set; } 
        public bool PreserveChanges { get; set; }
        public bool SourceIsFragment { get; set; }
        public bool PreserveNewRowState { get; set; }
        public object ProfileId {get; set; }

        public MergeParams()
        {
            TrueDelete = false;
            PreserveChanges = false;
            SourceIsFragment = false;
            PreserveNewRowState = false;
            ProfileId = DBNull.Value;
        }

        public MergeParams(object ProfileId) : this()
        {
            this.ProfileId = ProfileId;
        }
    }
}
