using System;
using System.Collections.Generic;

namespace Origam.ServerCore.Model.Chat
{
    public class NewChatRoom
    {
        public string topic { get; set; }
        public List<InviteUser> inviteUsers { get; set; }
        public Dictionary<object, object> references { get; set; }
    }
}
