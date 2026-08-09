#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

namespace Origam.AI.Agent.Tests.Infrastructure;

public sealed record AgentMessage(string Id, string Role, string Content);

public sealed class AgentConversation
{
    public string ThreadId { get; } = Guid.NewGuid().ToString();

    public List<AgentMessage> Messages { get; } = [];

    public void Append(string prompt, string reply)
    {
        Messages.Add(new AgentMessage(Guid.NewGuid().ToString(), Role: "user", prompt));
        if (reply.Trim().Length > 0)
        {
            Messages.Add(new AgentMessage(Guid.NewGuid().ToString(), Role: "assistant", reply));
        }
    }
}
