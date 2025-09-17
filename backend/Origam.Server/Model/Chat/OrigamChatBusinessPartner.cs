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
    public class OrigamChatBusinessPartner
    {
        public OrigamChatBusinessPartner(Guid guid, string name, string avatarurl)
        {
            id = guid;
            this.name = name;
            avatarUrl = avatarurl;
        }

        public Guid id { get; set; }
        public string name { get; set; }
        public string avatarUrl { get; set; }

        internal static List<OrigamChatBusinessPartner> CreateJson(DataSet datasetUsersForInvite, List<OrigamChatParticipant> participants,bool usersNotExistsInRoom = true)
        {
            List<OrigamChatBusinessPartner> mentions = new List<OrigamChatBusinessPartner>();
            foreach (DataRow row in datasetUsersForInvite.Tables["BusinessPartner"].Rows)
            {
                Guid ChatUser = row.Field<Guid>("Id");
                if (usersNotExistsInRoom)
                {
                    if (participants == null || !participants.Where(participant => participant.Id == ChatUser).Any())
                    {
                        mentions.Add(new OrigamChatBusinessPartner(row.Field<Guid>("Id"), row.Field<string>("FirstNameAndName"), row.Field<Guid>("Id").ToString()));
                    }
                }
                else
                {
                    if (participants.Where(participant => participant.Id == ChatUser).Any())
                    {
                        mentions.Add(new OrigamChatBusinessPartner(row.Field<Guid>("Id"), row.Field<string>("FirstNameAndName"), row.Field<Guid>("Id").ToString()));
                    }
                }
            }
            return mentions;
        }
    }
}
