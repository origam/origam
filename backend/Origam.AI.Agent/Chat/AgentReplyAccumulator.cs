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

using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Origam.AI.Agent.Models.Responses;

namespace Origam.AI.Agent.Chat;

public sealed class AgentReplyAccumulator
{
    private readonly StringBuilder replyText = new();
    private readonly UsageDetails usage = new();

    public bool ClosedWithText { get; private set; }

    public string ReplyText => replyText.ToString();

    public RunUsage Usage => TokenUsageReader.Read(usage);

    public void Add(AgentResponseUpdate update)
    {
        foreach (var content in update.Contents)
        {
            if (content is UsageContent usageContent)
            {
                usage.Add(usageContent.Details);
            }
            else if (content is TextContent textContent)
            {
                replyText.Append(textContent.Text);
                ClosedWithText = textContent.Text.Length > 0 || ClosedWithText;
            }
            else if (content is FunctionCallContent or FunctionResultContent)
            {
                ClosedWithText = false;
            }
        }
    }
}
