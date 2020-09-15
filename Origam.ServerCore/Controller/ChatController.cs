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
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Server;
using Origam.ServerCore.Model.Chat;
using Origam.ServerCore.Resources;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore.Controller
{
    [ApiController]
    [Route("chatrooms/[controller]")]
    public class ChatController : AbstractController
    {
        private readonly IStringLocalizer<SharedResources> localizer;
        private readonly Guid OrigamChatMessageDataStructureID = new Guid("f9ec17ce-13fc-420c-88b0-3a793fae4001");
        private readonly Guid OrigamChatMessageDataAfterIdIncludingMethodId = new Guid("61544d12-c4ec-4b05-83c7-427b8cc8bb32");
        private readonly Guid OrigamChatMessageDataBeforeIncludingMethodId = new Guid("338cca32-f6d2-454f-b4b0-e33a39a1a3cb");
        private readonly Guid OrigamChatMessageDataGetByIdMethodId = new Guid("88542750-75e0-4d48-b271-793126cf4847");
        private readonly Guid OrigamChatMessageDataGetByRoomIdMethodId = new Guid("42076303-19b4-4afa-9836-d38d87036c29");
        private readonly Guid OrigamChatMessageDataOrderbyCreatedDateSortSetId = new Guid("72c14783-0667-4827-b4ee-137e17fe0e3e");

        //private readonly Guid MessageDataOrderbyCreatedToomsId = new Guid("42076303-19b4-4afa-9836-d38d87036c29");
        private readonly Guid OrigamChatRoomDatastructureId = new Guid("ae1cd694-9354-49bf-a68e-93b1c241afd2");
        private readonly Guid OrigamChatRoomGetById = new Guid("a2d56dbe-6807-48ef-9765-7fd8cdaa09c7");

        private readonly Guid OrigamChatRoomBusinessPartnerId = new Guid("fab83217-6cb4-4693-b10e-6ded25a23abf");
        private readonly Guid OrigamChatRoomBusinessPartnerGetParticipantsId = new Guid("f7d1096f-4dfc-4b66-bef5-f54c7a1d9ee1");
        private readonly Guid OrigamChatRoomBusinessPartner_GetBusinessPartnerId = new Guid("5e3d904a-6ad9-4040-ac50-6fee9ee2debb");

        private readonly Guid LookupBusinessPartner = new Guid("f905e3d5-6255-46c3-96e9-4eab51df333a");
        private readonly Guid DefaultBusinessPartner = new Guid("df9202a0-18ad-4584-ba6b-9ef0be33e5de");
        private readonly Guid LookupBusinessPartnerGetById = new Guid("d8e44ef4-b151-46c9-bf2a-d4515f37821d");

        private readonly Guid OnlineUsers = new Guid("aa4c9df9-d6da-408e-a095-fd377ffcc319");
        private readonly Guid OrigamChatMessageBusinessPartnerDataStructureID = new Guid("763e029f-b306-40bd-98f9-36f89297cfbf");

        public ChatController(
            SessionObjects sessionObjects,
            IStringLocalizer<SharedResources> localizer,
            ILogger<AbstractController> log) : base(log, sessionObjects)
        {
            this.localizer = localizer;
        }

        [HttpGet("chatrooms/{requestChatRoomId:guid}/messages")]
        public IActionResult GetMessagesRequest(Guid requestChatRoomId,
            [FromQuery] int limit, [FromQuery] Guid afterIdIncluding, [FromQuery] Guid beforeIdIncluding)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(GetMessages(requestChatRoomId, limit, afterIdIncluding, beforeIdIncluding));
            });

        }

        [HttpGet("chatrooms/{requestChatRoomId:guid}/info")]
        public IActionResult GetChatRoomInfoRequest(Guid requestChatRoomId)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(GetChatRoomInfo(requestChatRoomId));
            });

        }

        [HttpGet("chatrooms/{requestChatRoomId:guid}/participants")]
        public IActionResult GetChatRoomParticipantsRequest(Guid requestChatRoomId)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(GetChatRoomParticipants(requestChatRoomId));
            });

        }

        [HttpGet("chatrooms/{requestChatRoomId:guid}/usersToInvite")]
        public IActionResult GetUsersToInviteRequest(Guid requestChatRoomId,
            [FromQuery] int limit, [FromQuery] int offset, [FromQuery] string searchPhrase)
        {
            return RunWithErrorHandler(() =>
            {
                return GetUsersToInvite(requestChatRoomId, limit,offset, searchPhrase);
            });

        }

        [HttpGet("chatrooms/{requestChatRoomId:guid}/usersToMention")]
        public IActionResult GetUsersToMentionRequest(Guid requestChatRoomId,
            [FromQuery] int limit, [FromQuery] int offset, [FromQuery] string searchPhrase)
        {
            return RunWithErrorHandler(() =>
            {
                return GetUsersToInvite(requestChatRoomId, limit, offset, searchPhrase);
            });

        }

        [HttpGet("users")]
        public IActionResult GetLocalUsersRequest()
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(GetLocalUsers());
            });

        }

        [HttpGet("chatrooms/{requestChatRoomId:guid}/polledData")]
        public IActionResult GetPollDataRequest(Guid requestChatRoomId,
            [FromQuery] int limit, [FromQuery] Guid afterIdIncluding, [FromQuery] Guid beforeIdIncluding)
        {
            return RunWithErrorHandler(() =>
            {
                return GetPollData(requestChatRoomId, limit, afterIdIncluding, beforeIdIncluding);
            });

        }

        [HttpGet()]
        public IActionResult GetRoomsRequest()
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(GetRoomsData());
            });

        }
        [HttpPost("chatrooms/{requestChatRoomId:guid}/messages")]
        public IActionResult PostMessagesRequest(Guid requestChatRoomId, [FromBody] OrigamChatMessage chatMessages)
        {
            return RunWithErrorHandler(() =>
            {
                return PostMessages(requestChatRoomId, chatMessages);
            });

        }

        [HttpPost("chatrooms/{requestChatRoomId:guid}/inviteUser")]
        public IActionResult PostinviteUserRequest(Guid requestChatRoomId, [FromBody] RequestUserId RequestId)
        {
            return RunWithErrorHandler(() =>
            {
                return PostinviteUser(requestChatRoomId, RequestId.userId);
            });

        }

        [HttpPost("chatrooms/{requestChatRoomId:guid}/abandon")]
        public IActionResult PostRoomAbandonRequest(Guid requestChatRoomId)
        {
            return RunWithErrorHandler(() =>
            {
                return PostRoomAbandon(requestChatRoomId);
            });

        }

        private IActionResult PostRoomAbandon(Guid requestChatRoomId)
        {
            DataSet datasetUsersForInvite = GetUserChatRoom(requestChatRoomId);
            if (datasetUsersForInvite != null)
            {
                datasetUsersForInvite.Tables[0].Rows[0].Delete();
                DataService.StoreData(OrigamChatRoomBusinessPartnerId, datasetUsersForInvite, false, null);
            }
            return Ok();
        }
        private object GetAvatar(Guid avatarId)
        {
            QueryParameterCollection parameters = new QueryParameterCollection
            {
                new QueryParameter("BusinessPartner_parId",avatarId)
            };
            DataSet datasetUsersForInvite = LoadData(LookupBusinessPartner, LookupBusinessPartnerGetById,
               Guid.Empty, Guid.Empty, null, parameters);
            if (datasetUsersForInvite.Tables[0].Rows.Count == 0)
            {
                return Ok();
            }
            return Ok(datasetUsersForInvite.Tables[0].Rows[0].Field<object>("avatar"));
        }
        private DataSet GetUserChatRoom(Guid requestChatRoomId)
        {
            UserProfile profile = SecurityTools.CurrentUserProfile();
            QueryParameterCollection parameters = new QueryParameterCollection
            {
                 new QueryParameter("OrigamChatRoomBussinesPartner_parBusinessPartnerId", profile.Id),
                 new QueryParameter("OrigamChatRoomBussinesPartner_parOrigamChatRoomId", requestChatRoomId)
            };
            DataSet resultdata = LoadData(OrigamChatRoomBusinessPartnerId, 
                                          OrigamChatRoomBusinessPartner_GetBusinessPartnerId,
                                          Guid.Empty, Guid.Empty, null, parameters);
            if (resultdata.Tables[0].Rows.Count == 0)
            {
                return null;
            }
            return resultdata;
        }

        private IActionResult PostinviteUser(Guid requestChatRoomId, Guid userId)
        {
            UserProfile profile = SecurityTools.CurrentUserProfile();
            if (GetUserChatRoom(requestChatRoomId)==null)
            {
                return Ok();
            }
            DatasetGenerator dsg = new DatasetGenerator(true);
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            DataStructure ds = (DataStructure)ps.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(OrigamChatRoomBusinessPartnerId));
            DataSet data = dsg.CreateDataSet(ds);
            DataRow r = data.Tables["OrigamChatRoomBussinesPartner"].NewRow();
            r["Id"] = Guid.NewGuid();
            r["isInvited"] = true;
            r["refOrigamChatRoomId"] = requestChatRoomId;
            r["refBusinessPartnerId"] = userId;
            r["RecordCreated"] = DateTime.Now;
            r["RecordCreatedBy"] = profile.Id;
            data.Tables["OrigamChatRoomBussinesPartner"].Rows.Add(r);
            DataService.StoreData(OrigamChatRoomBusinessPartnerId, data, false, null);
            return Ok();
        }
        private IActionResult GetUsersToInvite(Guid requestChatRoomId, int limit, int offset, string searchPhrase)
        {
            if(GetUserChatRoom(requestChatRoomId)==null)
            {
                return StatusCode(401,"You are not allowed add invite user to this chatroom.");
            }
            Guid methodid = Guid.Empty;
            QueryParameterCollection parameters = new QueryParameterCollection();
            if(!string.IsNullOrEmpty(searchPhrase))
            {
                parameters.Add(new QueryParameter("BusinessPartner_parSearchText", searchPhrase));
                methodid = DefaultBusinessPartner;

            };
            if (limit > 0)
            {
                parameters.Add(new QueryParameter("BusinessPartner__pageNumber", offset));
                parameters.Add(new QueryParameter("BusinessPartner__pageSize", limit));
            }
            DataSet datasetUsersForInvite = LoadData(LookupBusinessPartner,methodid,
                Guid.Empty, Guid.Empty, null, parameters);
            return Ok(OrigamChatBusinessPartner.CreateJson(datasetUsersForInvite));
        }

        private OrigamChatRoom GetRoomsData()
        {
            //UserProfile
            DataSet ds = LoadData(OrigamChatRoomDatastructureId, Guid.Empty,
               Guid.Empty, Guid.Empty, null, null);
            return OrigamChatRoom.CreateJson(ds);
        }

        private List<OrigamChatBusinessPartner> GetLocalUsers()
        {
            DataSet datasetUsersForInvite = LoadData(LookupBusinessPartner, Guid.Empty,
               Guid.Empty, Guid.Empty, null, null);
            return OrigamChatBusinessPartner.CreateJson(datasetUsersForInvite);
        }

        private object GetLocalUser()
        {
            UserProfile profile = SecurityTools.CurrentUserProfile();
            return new OrigamChatBusinessPartner(profile.Id,profile.FullName,profile.Id.ToString());
        }

        private List<OrigamChatParticipant> GetChatRoomParticipants(Guid requestChatRoomId)
        {
            QueryParameterCollection parameters = new QueryParameterCollection
                {
                    new QueryParameter("OrigamChatRoomBussinesPartner_parOrigamChatRoomId", requestChatRoomId),
                    new QueryParameter("OrigamChatRoomBussinesPartner_parIsInvited", true)
                };
            DataSet datasetParticipants = LoadData(OrigamChatRoomBusinessPartnerId, OrigamChatRoomBusinessPartnerGetParticipantsId,
                Guid.Empty, Guid.Empty, null, parameters);
            DataSet onlineUsers = LoadData(OnlineUsers,Guid.Empty,
                Guid.Empty, Guid.Empty, null, null);
            return OrigamChatParticipant.CreateJson(datasetParticipants,onlineUsers);
        }

        private object GetChatRoomInfo(Guid requestChatRoomId)
        {
            QueryParameterCollection parameters = new QueryParameterCollection
            {
               new QueryParameter("OrigamChatRoom_parId", requestChatRoomId)
            };
            DataSet datasetGetById = LoadData(OrigamChatRoomDatastructureId, OrigamChatRoomGetById,
                Guid.Empty, Guid.Empty, null, parameters);
            return OrigamChatRoom.CreateJson(datasetGetById);
        }

        private IActionResult PostMessages(Guid requestChatRoomId, OrigamChatMessage chatMessages)
        {
            if (GetUserChatRoom(requestChatRoomId) == null)
            {
                return StatusCode(401, "You are not allowed add Message to this chatroom.");
            }
            UserProfile profile = SecurityTools.CurrentUserProfile();
            DatasetGenerator dsg = new DatasetGenerator(true);
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            DataStructure ds = (DataStructure)ps.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(OrigamChatMessageDataStructureID));
            DataSet data = dsg.CreateDataSet(ds);
            DataRow r = data.Tables["OrigamChatMessage"].NewRow();
            r["Id"] = chatMessages.id;
            r["TextMessage"] = chatMessages.text;
            r["refOrigamChatRoomId"] = requestChatRoomId;
            r["RecordCreated"] = DateTime.Now;
            r["RecordCreatedBy"] = profile.Id;
            r["refBusinessPartnerId"] = profile.Id;
            r["Mentions"] = chatMessages.mentions.Count;
            data.Tables["OrigamChatMessage"].Rows.Add(r);
            DataService.StoreData(OrigamChatMessageDataStructureID, data, false, null);

            DatasetGenerator dsgmen = new DatasetGenerator(true);
            DataStructure dsmen = (DataStructure)ps.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), 
                                                new ModelElementKey(OrigamChatMessageBusinessPartnerDataStructureID));
            DataSet datanem = dsgmen.CreateDataSet(dsmen);
            foreach (Guid messageMention in chatMessages.mentions)
            {
                DataRow rmen = datanem.Tables["OrigamChatMessageBusinessPartner"].NewRow();
                rmen["Id"] = Guid.NewGuid();
                rmen["RecordCreated"] = DateTime.Now;
                rmen["RecordCreatedBy"] = profile.Id;
                rmen["refBusinessPartnerId"] = messageMention;
                rmen["refOrigamChatMessageId"] = chatMessages.id;
                datanem.Tables["OrigamChatMessageBusinessPartner"].Rows.Add(rmen);
            }
            DataService.StoreData(OrigamChatMessageBusinessPartnerDataStructureID, datanem, false, null);
            return Ok();
        }
        private IActionResult GetPollData(Guid requestChatRoomId, int limit, Guid afterIdIncluding, Guid beforeIdIncluding)
        {
            Dictionary<string, object> pollData = new Dictionary<string, object>();
            List<OrigamChatParticipant> participants = GetChatRoomParticipants(requestChatRoomId);
            OrigamChatBusinessPartner activeUser = (OrigamChatBusinessPartner)GetLocalUser();
            if (!participants.Where(participant => participant.id == activeUser.id).Any())
            {
                return StatusCode(401, "You are not allowed add Message to this chatroom.");
            }
            pollData.Add("messages", GetMessages(requestChatRoomId, limit, afterIdIncluding, beforeIdIncluding));
            pollData.Add("localUser", activeUser);
            pollData.Add("participants", participants);
            pollData.Add("info", GetChatRoomInfo(requestChatRoomId));
            return Ok(pollData);
        }

        private List<OrigamChatMessage> GetMessages(Guid requestChatRoomId, int limit, Guid afterIdIncluding, Guid beforeIdIncluding)
        {
            List<OrigamChatBusinessPartner> allUsers = GetLocalUsers();

            Guid including = Guid.Empty;
            Guid MethodId = Guid.Empty;

            if (afterIdIncluding != null && afterIdIncluding != Guid.Empty)
            {
                including = afterIdIncluding;
                MethodId = OrigamChatMessageDataAfterIdIncludingMethodId;
            }
            if (beforeIdIncluding != null && beforeIdIncluding != Guid.Empty)
            {
                including = beforeIdIncluding;
                MethodId = OrigamChatMessageDataBeforeIncludingMethodId;
            }
            
            QueryParameterCollection parametersMessages = new QueryParameterCollection();
            if (including != Guid.Empty)
            {
                QueryParameterCollection parameters = new QueryParameterCollection
                {
                    new QueryParameter("OrigamChatMessage_parId", including)
                };

                DataSet getRecordCreated = LoadData(OrigamChatMessageDataStructureID, OrigamChatMessageDataGetByIdMethodId,
                        Guid.Empty, Guid.Empty, null, parameters);
                parametersMessages.Add(new QueryParameter("OrigamChatMessage_parCreatedDateTime", getRecordCreated.Tables[0].Rows[0]["RecordCreated"]));
            }

            parametersMessages.Add(new QueryParameter("OrigamChatMessage_parOrigamChatRoomId", requestChatRoomId));
            if (limit > 0)
            {
                parametersMessages.Add(new QueryParameter("OrigamChatMessage__pageNumber", 0));
                parametersMessages.Add(new QueryParameter("OrigamChatMessage__pageSize", limit));
            }
            DataSet MessagesDataSet = LoadData(OrigamChatMessageDataStructureID, MethodId,
                Guid.Empty, OrigamChatMessageDataOrderbyCreatedDateSortSetId, null, parametersMessages);
            return OrigamChatMessage.CreateJson(MessagesDataSet, allUsers);
        }

        private DataSet LoadData(Guid messageDataStructureID, Guid messageDataafterIdIncludingMethodId, Guid guid, Guid messageDataOrderbyCreatedDateSortSetId, string transactionId, QueryParameterCollection parameters)
        {
           return DataService.LoadData(messageDataStructureID,
                    messageDataafterIdIncludingMethodId,
                    guid,
                    messageDataOrderbyCreatedDateSortSetId,
                    transactionId,
                    parameters);
        }
    }
}
