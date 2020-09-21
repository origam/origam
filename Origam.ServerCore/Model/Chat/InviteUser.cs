using System;

namespace Origam.ServerCore.Model.Chat
{
    public class InviteUser
    {
        public InviteUser(Guid userId)
        {
            this.id = userId;
        }

        public Guid id { get; set; }
    }
}
