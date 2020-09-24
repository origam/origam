#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

namespace Origam.ServerCore.Model.Chat
{
    public class OrigamChatRoom
    {
        public OrigamChatRoom(Guid id, string topic,int unreadMessageCount)
        {
            this.id = id;
            this.topic = topic;
            this.unreadMessageCount = unreadMessageCount;
        }
        public Guid id { get; set; }
        public string topic { get; set; }
        
        public int unreadMessageCount { get; private set; }
        internal static List<OrigamChatRoom> CreateJson(DataSet ChatRoomDataSet)
        {
            List<OrigamChatRoom> chatRoom = new List<OrigamChatRoom>();
            DataTable table = ChatRoomDataSet.Tables["OrigamChatRoom"]; 
            foreach (DataRow row in table.Rows)
            {
                chatRoom.Add(new OrigamChatRoom(row.Field<Guid>("Id"), row.Field<string>("Name"),getCount(ChatRoomDataSet)));
            }
            return chatRoom;
        }

        private static int getCount(DataSet dataTables)
        {
            UserProfile profile = SecurityManager.CurrentUserProfile();
            DataTable dataTable = dataTables.Tables["OrigamChatRoomBussinesPartner"];
            foreach (DataRow row in dataTable.Rows)
            {
                if(row.Field<Guid>("refBusinessPartnerId") ==profile.Id)
                {
                    return GetCountMessages(dataTables.Tables["OrigamChatMessage"],row.Field<DateTime>("LastSeen"));
                }
            }
            return 0;
        }

        private static int GetCountMessages(DataTable dataTable, DateTime dateTime)
        {
            if(dateTime==null)
            {
                return dataTable.Rows.Count;
            }
            int count = 0;
            foreach(DataRow row in dataTable.Rows)
            {
                DateTime date = row.Field<DateTime>("RecordCreated");
                int result = DateTime.Compare(date, dateTime);
                if (result>0)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
