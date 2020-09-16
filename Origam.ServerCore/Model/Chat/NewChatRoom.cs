using System;
using System.Collections.Generic;

namespace Origam.ServerCore.Model.Chat
{
    public class NewChatRoom
    {
        public string topic { get; set; }
        public List<Guid> inviteUsers { get; set; }
        public Dictionary<string,string> references { get; set; }
    }
}
