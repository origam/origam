#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Origam.Server.Model.Chat
{
    public class OrigamChatMessage
    {
        public OrigamChatMessage(
            Guid id,
            Guid authorId,
            string authorName,
            string authorAvatarUrl,
            DateTime dateCreated,
            string TextMessages,
            List<Guid> mentions
        )
        {
            this.Id = id;
            this.AuthorId = authorId;
            this.AuthorName = authorName;
            this.AuthorAvatarUrl = authorAvatarUrl;
            this.DateCreated = dateCreated;
            Text = TextMessages;
            this.Mentions = mentions;
        }

        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorAvatarUrl { get; set; }
        public DateTime TimeSent
        {
            get { return DateCreated; }
        }
        public DateTime DateCreated { get; set; }
        public string Text { get; set; }
        public List<Guid> Mentions { get; set; }

        internal static List<OrigamChatMessage> CreateJson(
            DataSet MessagesDataSet,
            List<OrigamChatBusinessPartner> allusers
        )
        {
            List<OrigamChatMessage> messages = new List<OrigamChatMessage>();
            foreach (DataRow row in MessagesDataSet.Tables["OrigamChatMessage"].Rows)
            {
                Guid userId = row.Field<Guid>("RecordCreatedBy");
                var username = allusers
                    .Where(mention => mention.id.Equals(userId))
                    .Select(mention =>
                    {
                        return mention.name;
                    })
                    .FirstOrDefault();
                List<Guid> listmention = new List<Guid>();
                foreach (
                    DataRow rowBpartner in MessagesDataSet
                        .Tables["OrigamChatMessageBusinessPartner"]
                        .Rows
                )
                {
                    if (rowBpartner.Field<Guid>("refOrigamChatMessageId") == row.Field<Guid>("Id"))
                    {
                        listmention.Add(rowBpartner.Field<Guid>("refBusinessPartnerId"));
                    }
                }
                messages.Add(
                    new OrigamChatMessage(
                        row.Field<Guid>("Id"),
                        userId,
                        username,
                        userId.ToString(),
                        row.Field<DateTime>("RecordCreated"),
                        row.Field<string>("TextMessage"),
                        listmention
                    )
                );
            }
            return messages;
        }
    }
}
