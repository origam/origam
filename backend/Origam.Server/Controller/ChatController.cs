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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Server.Model.Chat;
using Origam.Service.Core;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Controller;

[ApiController]
[Route("chatrooms/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> log;

    private readonly Guid OrigamChatMessageDataStructureId = new Guid(
        "f9ec17ce-13fc-420c-88b0-3a793fae4001"
    );
    private readonly Guid OrigamChatMessageDataAfterIdIncludingMethodId = new Guid(
        "61544d12-c4ec-4b05-83c7-427b8cc8bb32"
    );
    private readonly Guid OrigamChatMessageDataBeforeIncludingMethodId = new Guid(
        "338cca32-f6d2-454f-b4b0-e33a39a1a3cb"
    );
    private readonly Guid OrigamChatMessageDataGetByIdMethodId = new Guid(
        "88542750-75e0-4d48-b271-793126cf4847"
    );
    private readonly Guid OrigamChatMessageDataGetByRoomIdMethodId = new Guid(
        "42076303-19b4-4afa-9836-d38d87036c29"
    );
    private readonly Guid OrigamChatMessageDataOrderByCreatedDateSortSetId = new Guid(
        "72c14783-0667-4827-b4ee-137e17fe0e3e"
    );

    private readonly Guid OrigamChatRoomDatastructureId = new Guid(
        "ae1cd694-9354-49bf-a68e-93b1c241afd2"
    );
    private readonly Guid OrigamChatRoomGetById = new Guid("a2d56dbe-6807-48ef-9765-7fd8cdaa09c7");

    private readonly Guid OrigamChatRoomBusinessPartnerId = new Guid(
        "fab83217-6cb4-4693-b10e-6ded25a23abf"
    );
    private readonly Guid OrigamChatRoomBusinessPartnerGetParticipantsId = new Guid(
        "f7d1096f-4dfc-4b66-bef5-f54c7a1d9ee1"
    );
    private readonly Guid OrigamChatRoomBusinessPartner_GetBusinessPartnerId = new Guid(
        "5e3d904a-6ad9-4040-ac50-6fee9ee2debb"
    );
    private readonly Guid GetByBusinessPartnerId_IsInvited = new Guid(
        "36fd5fb8-5aba-4e87-85c2-e412516888ad"
    );
    private readonly Guid GetByOrigamChatRoomId = new Guid("c69ed9ec-df67-4c6e-b99a-726e40316b5e");
    private readonly Guid GetByOrigamChatRoomIdSearch = new Guid(
        "fda05fef-8069-41a2-a8fb-d67395e9cea8"
    );

    private readonly Guid LookupBusinessPartner = new Guid("d7921e0c-b763-4d07-a019-7e948b4c49a6");
    private readonly Guid DefaultBusinessPartner = new Guid("ab4eb78c-6dcf-46c7-a316-e856f835fbfa");
    private readonly Guid LookupBusinessPartnerGetByIdWithoutMe = new Guid(
        "34dcbb06-b353-42ff-9f33-f03478eb7ece"
    );
    private readonly Guid LookupBusinessPartnerGetAllUsers = new Guid(
        "73bd2418-fc6c-4cf4-b44b-ad2084e25af9"
    );

    private readonly Guid OnlineUsers = new Guid("aa4c9df9-d6da-408e-a095-fd377ffcc319");
    private readonly Guid OrigamChatMessageBusinessPartnerDataStructureId = new Guid(
        "763e029f-b306-40bd-98f9-36f89297cfbf"
    );

    public ChatController(ILogger<ChatController> log)
    {
        this.log = log;
    }

    [HttpGet("chatrooms/{requestChatRoomId:guid}/messages")]
    public IActionResult GetMessagesRequest(
        Guid requestChatRoomId,
        [FromQuery] int limit,
        [FromQuery] Guid afterIdIncluding,
        [FromQuery] Guid beforeIdIncluding
    )
    {
        return RunWithErrorHandler(() =>
            Ok(GetMessages(requestChatRoomId, limit, afterIdIncluding, beforeIdIncluding))
        );
    }

    [HttpGet("chatrooms/{requestChatRoomId:guid}/info")]
    public IActionResult GetChatRoomInfoRequest(Guid requestChatRoomId)
    {
        return RunWithErrorHandler(() => Ok(GetChatRoomInfo(requestChatRoomId)));
    }

    [HttpGet("chatrooms/{requestChatRoomId:guid}/participants")]
    public IActionResult GetChatRoomParticipantsRequest(Guid requestChatRoomId)
    {
        return RunWithErrorHandler(() => Ok(GetChatRoomParticipants(requestChatRoomId)));
    }

    //List user for possible invite into Chatroom without exists users.
    [HttpGet("chatrooms/{requestChatRoomId:guid}/usersToInvite")]
    public IActionResult GetUsersToInviteToRoomRequest(
        Guid requestChatRoomId,
        [FromQuery] int limit,
        [FromQuery] int offset,
        [FromQuery] string searchPhrase
    )
    {
        return RunWithErrorHandler(() =>
            GetRoomUsers(requestChatRoomId, limit, offset, searchPhrase, true)
        );
    }

    //List Participant user for outvite
    [HttpGet("chatrooms/{requestChatRoomId:guid}/usersToOutvite")]
    public IActionResult GetUsersToOutviteToRoomRequest(
        Guid requestChatRoomId,
        [FromQuery] int limit,
        [FromQuery] int offset,
        [FromQuery] string searchPhrase
    )
    {
        return RunWithErrorHandler(() =>
            GetOutviteRoomUsers(requestChatRoomId, limit, offset, searchPhrase)
        );
    }

    //List user invite to chatroom (New Room)
    [HttpGet("users/listToInvite")]
    public IActionResult GetAllUsersToInviteRequest(
        [FromQuery] int limit,
        [FromQuery] int offset,
        [FromQuery] string searchPhrase
    )
    {
        return RunWithErrorHandler(() => GetUsersToInviteToNewRoom(limit, offset, searchPhrase));
    }

    [HttpGet("chatrooms/{requestChatRoomId:guid}/usersToMention")]
    public IActionResult GetUsersToMentionRequest(
        Guid requestChatRoomId,
        [FromQuery] int limit,
        [FromQuery] int offset,
        [FromQuery] string searchPhrase
    )
    {
        return RunWithErrorHandler(() =>
            GetRoomUsers(requestChatRoomId, limit, offset, searchPhrase, false)
        );
    }

    [HttpGet("users")]
    public IActionResult GetLocalUsersRequest()
    {
        return RunWithErrorHandler(() => Ok(GetLocalUsers()));
    }

    [HttpGet("chatrooms/{requestChatRoomId:guid}/polledData")]
    public IActionResult GetPollDataRequest(
        Guid requestChatRoomId,
        [FromQuery] int limit,
        [FromQuery] Guid afterIdIncluding,
        [FromQuery] Guid beforeIdIncluding
    )
    {
        return RunWithErrorHandler(() =>
            GetPollData(requestChatRoomId, limit, afterIdIncluding, beforeIdIncluding)
        );
    }

    [HttpGet()]
    public IActionResult GetRoomsRequest()
    {
        return RunWithErrorHandler(() => Ok(GetRoomsData()));
    }

    [HttpPost("chatrooms/create")]
    public IActionResult CreateRoomRequest([FromBody] NewChatRoom newRoomJson)
    {
        return RunWithErrorHandler(() => CreateRoom(newRoomJson));
    }

    [HttpPost("chatrooms/{requestChatRoomId:guid}/messages")]
    public IActionResult PostMessagesRequest(
        Guid requestChatRoomId,
        [FromBody] OrigamChatMessage chatMessages
    )
    {
        return RunWithErrorHandler(() => PostMessages(requestChatRoomId, chatMessages));
    }

    [HttpPost("chatrooms/{requestChatRoomId:guid}/inviteUser")]
    public IActionResult PostInviteUserRequest(
        Guid requestChatRoomId,
        [FromBody] RequestUserId requestId
    )
    {
        return RunWithErrorHandler(() => PostInviteUser(requestChatRoomId, requestId.UserId));
    }

    [HttpPost("chatrooms/{requestChatRoomId:guid}/outviteUser")]
    public IActionResult PostRoomAbandonRequest(Guid requestChatRoomId, OutviteUser outviteUser)
    {
        return RunWithErrorHandler(() => PostRoomAbandon(requestChatRoomId, outviteUser));
    }

    [HttpPost("chatrooms/{requestChatRoomId:guid}/info")]
    public IActionResult PostRoomAbandonRequest(Guid requestChatRoomId, [FromBody] Info topic)
    {
        return RunWithErrorHandler(() => PostRoomChangeTopic(requestChatRoomId, topic));
    }

    private IActionResult PostRoomChangeTopic(Guid requestChatRoomId, Info topic)
    {
        var roomInfo = GetChatRoom(requestChatRoomId);
        var dataRow = roomInfo.Tables[0].Rows[0];
        dataRow["Name"] = topic.Topic;
        DataService.Instance.StoreData(OrigamChatRoomDatastructureId, roomInfo, false, null);
        return Ok();
    }

    private IActionResult CreateRoom(NewChatRoom newRoomJson)
    {
        var newChatRoomId = CreateRoomDatabase(newRoomJson);
        return Ok(newChatRoomId);
    }

    private Guid CreateRoomDatabase(NewChatRoom newChatRoom)
    {
        var profile = SecurityManager.CurrentUserProfile();
        var newChatRoomId = Guid.NewGuid();
        var datasetGenerator = new DatasetGenerator(true);
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var dataStructure = (DataStructure)
            persistenceService.SchemaProvider.RetrieveInstance(
                typeof(AbstractSchemaItem),
                new ModelElementKey(OrigamChatRoomDatastructureId)
            );
        var dataset = datasetGenerator.CreateDataSet(dataStructure);
        var row = dataset.Tables["OrigamChatRoom"].NewRow();
        row["Id"] = newChatRoomId;
        row["Name"] = newChatRoom.Topic;
        if (
            newChatRoom.ReferenceRecordId.HasValue
            && !string.IsNullOrEmpty(newChatRoom.ReferenceCategory)
        )
        {
            row["ReferenceId"] = newChatRoom.ReferenceRecordId.Value;
            row["ReferenceEntity"] = newChatRoom.ReferenceCategory;
        }
        row["RecordCreated"] = DateTime.Now;
        row["RecordCreatedBy"] = profile.Id;
        dataset.Tables["OrigamChatRoom"].Rows.Add(row);
        DataService.Instance.StoreData(OrigamChatRoomDatastructureId, dataset, false, null);
        newChatRoom.InviteUsers.Add(new InviteUser(profile.Id));
        AddUsersIntoChatRoom(newChatRoomId, newChatRoom.InviteUsers);
        return newChatRoomId;
    }

    private void AddUsersIntoChatRoom(Guid newChatRoomId, List<InviteUser> inviteUsers)
    {
        var profile = SecurityManager.CurrentUserProfile();
        var datasetGenerator = new DatasetGenerator(true);
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var dataStructure = (DataStructure)
            persistenceService.SchemaProvider.RetrieveInstance(
                typeof(AbstractSchemaItem),
                new ModelElementKey(OrigamChatRoomBusinessPartnerId)
            );
        var dataSet = datasetGenerator.CreateDataSet(dataStructure);
        foreach (var inviteUser in inviteUsers)
        {
            var userId = inviteUser.Id;
            var row = dataSet.Tables["OrigamChatRoomBusinessPartner"].NewRow();
            row["Id"] = Guid.NewGuid();
            row["IsInvited"] = true;
            row["refOrigamChatRoomId"] = newChatRoomId;
            row["refBusinessPartnerId"] = userId;
            row["RecordCreated"] = DateTime.Now;
            row["RecordCreatedBy"] = profile.Id;
            dataSet.Tables["OrigamChatRoomBusinessPartner"].Rows.Add(row);
        }
        DataService.Instance.StoreData(OrigamChatRoomBusinessPartnerId, dataSet, false, null);
    }

    private IActionResult PostRoomAbandon(Guid requestChatRoomId, OutviteUser outviteUser)
    {
        var datasetUsersForInvite = GetActiveUserChatRoom(requestChatRoomId, outviteUser);
        if (datasetUsersForInvite != null)
        {
            datasetUsersForInvite.Tables[0].Rows[0].Delete();
            DataService.Instance.StoreData(
                OrigamChatRoomBusinessPartnerId,
                datasetUsersForInvite,
                false,
                null
            );
        }
        return Ok();
    }

    private DataSet GetActiveUserChatRoom(Guid requestChatRoomId, OutviteUser outviteUser)
    {
        var parameters = new QueryParameterCollection
        {
            new QueryParameter(
                "OrigamChatRoomBusinessPartner_parBusinessPartnerId",
                outviteUser.UserId
            ),
            new QueryParameter(
                "OrigamChatRoomBusinessPartner_parOrigamChatRoomId",
                requestChatRoomId
            ),
        };
        var resultData = LoadData(
            OrigamChatRoomBusinessPartnerId,
            OrigamChatRoomBusinessPartner_GetBusinessPartnerId,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        return resultData.Tables[0].Rows.Count == 0 ? null : resultData;
    }

    private IActionResult PostInviteUser(Guid requestChatRoomId, Guid userId)
    {
        var users = new List<InviteUser> { new InviteUser(userId) };
        AddUsersIntoChatRoom(requestChatRoomId, users);
        return Ok();
    }

    private IActionResult GetUsersToInviteToNewRoom(int limit, int offset, string searchPhrase)
    {
        var methodId = LookupBusinessPartnerGetByIdWithoutMe;
        QueryParameterCollection parameters = new QueryParameterCollection();
        if (!string.IsNullOrEmpty(searchPhrase))
        {
            parameters.Add(new QueryParameter("BusinessPartner_parSearchText", searchPhrase));
            methodId = DefaultBusinessPartner;
        }
        if (limit > 0)
        {
            parameters.Add(new QueryParameter("BusinessPartner__pageNumber", offset));
            parameters.Add(new QueryParameter("BusinessPartner__pageSize", limit));
        }
        var datasetUsersForInvite = LoadData(
            LookupBusinessPartner,
            methodId,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        return Ok(OrigamChatBusinessPartner.CreateJson(datasetUsersForInvite, null));
    }

    private IActionResult GetOutviteRoomUsers(
        Guid requestChatRoomId,
        int limit,
        int offset,
        string searchPhrase
    )
    {
        var participants = GetChatRoomParticipants(requestChatRoomId);
        var methodId = GetByOrigamChatRoomId;
        var parameters = new QueryParameterCollection
        {
            new QueryParameter(
                "OrigamChatRoomBusinessPartner_parOrigamChatRoomId",
                requestChatRoomId
            ),
        };
        if (!string.IsNullOrEmpty(searchPhrase))
        {
            parameters.Add(new QueryParameter("BusinessPartnerLookup_parSearchText", searchPhrase));
            methodId = GetByOrigamChatRoomIdSearch;
        }
        if (limit > 0)
        {
            parameters.Add(new QueryParameter("BusinessPartner__pageNumber", offset));
            parameters.Add(new QueryParameter("BusinessPartner__pageSize", limit));
        }
        var datasetUsersForInvite = LoadData(
            OrigamChatRoomBusinessPartnerId,
            methodId,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        return Ok(OrigamChatBusinessPartner.CreateJson(datasetUsersForInvite, participants, false));
    }

    private IActionResult GetRoomUsers(
        Guid requestChatRoomId,
        int limit,
        int offset,
        string searchPhrase,
        bool usersNotExistsInRoom
    )
    {
        var participants = GetChatRoomParticipants(requestChatRoomId);
        var methodId = usersNotExistsInRoom
            ? LookupBusinessPartnerGetByIdWithoutMe
            : LookupBusinessPartnerGetAllUsers;
        var parameters = new QueryParameterCollection();
        if (!string.IsNullOrEmpty(searchPhrase))
        {
            parameters.Add(new QueryParameter("BusinessPartner_parSearchText", searchPhrase));
            methodId = DefaultBusinessPartner;
        }
        if (limit > 0)
        {
            parameters.Add(new QueryParameter("BusinessPartner__pageNumber", offset));
            parameters.Add(new QueryParameter("BusinessPartner__pageSize", limit));
        }
        var datasetUsersForInvite = LoadData(
            LookupBusinessPartner,
            methodId,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        return Ok(
            OrigamChatBusinessPartner.CreateJson(
                datasetUsersForInvite,
                participants,
                usersNotExistsInRoom
            )
        );
    }

    private List<OrigamChatRoom> GetRoomsData()
    {
        var profile = SecurityManager.CurrentUserProfile();
        var parameters = new QueryParameterCollection
        {
            new QueryParameter("OrigamChatRoomBusinessPartner_parBusinessPartnerId", profile.Id),
            new QueryParameter("OrigamChatRoomBusinessPartner_parIsInvited", true),
        };
        var chatRooms = LoadData(
            OrigamChatRoomBusinessPartnerId,
            GetByBusinessPartnerId_IsInvited,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        var unreadMessages = GetUnreadMessages(chatRooms);
        return OrigamChatRoom.CreateJson(chatRooms, unreadMessages);
    }

    private Dictionary<Guid, int> GetUnreadMessages(DataSet chatRooms)
    {
        var unreadMessages = new Dictionary<Guid, int>();
        foreach (DataRow dataRow in chatRooms.Tables["OrigamChatRoomBusinessPartner"].Rows)
        {
            var parameters = new QueryParameterCollection
            {
                new QueryParameter(
                    "OrigamChatMessage_parCreatedDateTime",
                    dataRow.Field<DateTime?>("LastSeen") ?? new DateTime(1900, 1, 1)
                ),
                new QueryParameter(
                    "OrigamChatMessage_parOrigamChatRoomId",
                    dataRow.Field<Guid>("refOrigamChatRoomId")
                ),
            };
            var datasetRoom = LoadData(
                OrigamChatMessageDataStructureId,
                OrigamChatMessageDataAfterIdIncludingMethodId,
                Guid.Empty,
                Guid.Empty,
                null,
                parameters
            );
            unreadMessages.Add(
                dataRow.Field<Guid>("refOrigamChatRoomId"),
                datasetRoom.Tables["OrigamChatMessage"].Rows.Count
            );
        }
        return unreadMessages;
    }

    private List<OrigamChatBusinessPartner> GetLocalUsers()
    {
        var datasetUsersForInvite = LoadData(
            LookupBusinessPartner,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            null,
            null
        );
        return OrigamChatBusinessPartner.CreateJson(datasetUsersForInvite, null);
    }

    private object GetLocalUser()
    {
        var profile = SecurityManager.CurrentUserProfile();
        return new OrigamChatBusinessPartner(profile.Id, profile.FullName, profile.Id.ToString());
    }

    private List<OrigamChatParticipant> GetChatRoomParticipants(Guid requestChatRoomId)
    {
        var parameters = new QueryParameterCollection
        {
            new QueryParameter(
                "OrigamChatRoomBusinessPartner_parOrigamChatRoomId",
                requestChatRoomId
            ),
            new QueryParameter("OrigamChatRoomBusinessPartner_parIsInvited", true),
        };
        var datasetParticipants = LoadData(
            OrigamChatRoomBusinessPartnerId,
            OrigamChatRoomBusinessPartnerGetParticipantsId,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        var onlineUsers = LoadData(OnlineUsers, Guid.Empty, Guid.Empty, Guid.Empty, null, null);
        return OrigamChatParticipant.CreateJson(datasetParticipants, onlineUsers);
    }

    private OrigamChatRoom GetChatRoomInfo(Guid requestChatRoomId)
    {
        var datasetGetById = GetChatRoom(requestChatRoomId);
        return OrigamChatRoom.CreateJson(datasetGetById, new Dictionary<Guid, int>())[0];
    }

    private DataSet GetChatRoom(Guid requestChatRoomId)
    {
        var parameters = new QueryParameterCollection
        {
            new QueryParameter("OrigamChatRoom_parId", requestChatRoomId),
        };
        var datasetRoom = LoadData(
            OrigamChatRoomDatastructureId,
            OrigamChatRoomGetById,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        return datasetRoom;
    }

    private IActionResult PostMessages(Guid requestChatRoomId, OrigamChatMessage chatMessages)
    {
        var profile = SecurityManager.CurrentUserProfile();
        var datasetGenerator = new DatasetGenerator(true);
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var messageDataStructure = (DataStructure)
            persistenceService.SchemaProvider.RetrieveInstance(
                typeof(AbstractSchemaItem),
                new ModelElementKey(OrigamChatMessageDataStructureId)
            );
        var messageDataset = datasetGenerator.CreateDataSet(messageDataStructure);
        var messageRow = messageDataset.Tables["OrigamChatMessage"].NewRow();
        messageRow["Id"] = chatMessages.Id;
        messageRow["TextMessage"] = chatMessages.Text;
        messageRow["refOrigamChatRoomId"] = requestChatRoomId;
        messageRow["RecordCreated"] = DateTime.Now;
        messageRow["RecordCreatedBy"] = profile.Id;
        messageRow["refBusinessPartnerId"] = profile.Id;
        messageRow["Mentions"] = chatMessages.Mentions.Count;
        messageDataset.Tables["OrigamChatMessage"].Rows.Add(messageRow);
        DataService.Instance.StoreData(
            OrigamChatMessageDataStructureId,
            messageDataset,
            false,
            null
        );
        var messageBusinessPartnerDataStructure = (DataStructure)
            persistenceService.SchemaProvider.RetrieveInstance(
                typeof(AbstractSchemaItem),
                new ModelElementKey(OrigamChatMessageBusinessPartnerDataStructureId)
            );
        var messageBusinessPartnerDataSet = datasetGenerator.CreateDataSet(
            messageBusinessPartnerDataStructure
        );
        foreach (var messageMention in chatMessages.Mentions)
        {
            var messageBusinessPartnerRow = messageBusinessPartnerDataSet
                .Tables["OrigamChatMessageBusinessPartner"]
                .NewRow();
            messageBusinessPartnerRow["Id"] = Guid.NewGuid();
            messageBusinessPartnerRow["RecordCreated"] = DateTime.Now;
            messageBusinessPartnerRow["RecordCreatedBy"] = profile.Id;
            messageBusinessPartnerRow["refBusinessPartnerId"] = messageMention;
            messageBusinessPartnerRow["refOrigamChatMessageId"] = chatMessages.Id;
            messageBusinessPartnerDataSet
                .Tables["OrigamChatMessageBusinessPartner"]
                .Rows.Add(messageBusinessPartnerRow);
        }
        DataService.Instance.StoreData(
            OrigamChatMessageBusinessPartnerDataStructureId,
            messageBusinessPartnerDataSet,
            false,
            null
        );
        CreateLastSeen(requestChatRoomId);
        return Ok();
    }

    private IActionResult GetPollData(
        Guid requestChatRoomId,
        int limit,
        Guid afterIdIncluding,
        Guid beforeIdIncluding
    )
    {
        var pollData = new Dictionary<string, object>();
        var participants = GetChatRoomParticipants(requestChatRoomId);
        var activeUser = (OrigamChatBusinessPartner)GetLocalUser();
        if (participants.All(participant => participant.Id != activeUser.id))
        {
            return StatusCode(403, "You are not allowed to join this chatroom.");
        }
        pollData.Add(
            "messages",
            GetMessages(requestChatRoomId, limit, afterIdIncluding, beforeIdIncluding)
        );
        pollData.Add("localUser", activeUser);
        pollData.Add("participants", participants);
        pollData.Add("info", GetChatRoomInfo(requestChatRoomId));
        CreateLastSeen(requestChatRoomId);
        return Ok(pollData);
    }

    private void CreateLastSeen(Guid requestChatRoomId)
    {
        var profile = SecurityManager.CurrentUserProfile();
        var inviteUser = new OutviteUser(profile.Id);
        var userDataset = GetActiveUserChatRoom(requestChatRoomId, inviteUser);
        var userRow = userDataset.Tables[0].Rows[0];
        userRow["LastSeen"] = DateTime.Now;
        DataService.Instance.StoreData(OrigamChatRoomBusinessPartnerId, userDataset, false, null);
    }

    private List<OrigamChatMessage> GetMessages(
        Guid requestChatRoomId,
        int limit,
        Guid afterIdIncluding,
        Guid beforeIdIncluding
    )
    {
        var allUsers = GetLocalUsers();
        var includingId = Guid.Empty;
        var methodId = OrigamChatMessageDataGetByRoomIdMethodId;
        if (afterIdIncluding != Guid.Empty)
        {
            includingId = afterIdIncluding;
            methodId = OrigamChatMessageDataAfterIdIncludingMethodId;
        }
        if (beforeIdIncluding != Guid.Empty)
        {
            includingId = beforeIdIncluding;
            methodId = OrigamChatMessageDataBeforeIncludingMethodId;
        }
        var parametersMessages = new QueryParameterCollection();
        if (includingId != Guid.Empty)
        {
            var parameters = new QueryParameterCollection
            {
                new QueryParameter("OrigamChatMessage_parId", includingId),
            };
            var getRecordCreated = LoadData(
                OrigamChatMessageDataStructureId,
                OrigamChatMessageDataGetByIdMethodId,
                Guid.Empty,
                Guid.Empty,
                null,
                parameters
            );
            parametersMessages.Add(
                new QueryParameter(
                    "OrigamChatMessage_parCreatedDateTime",
                    getRecordCreated.Tables[0].Rows[0]["RecordCreated"]
                )
            );
        }
        parametersMessages.Add(
            new QueryParameter("OrigamChatMessage_parOrigamChatRoomId", requestChatRoomId)
        );
        if (limit > 0)
        {
            parametersMessages.Add(new QueryParameter("OrigamChatMessage__pageNumber", 0));
            parametersMessages.Add(new QueryParameter("OrigamChatMessage__pageSize", limit));
        }
        var messagesDataSet = LoadData(
            OrigamChatMessageDataStructureId,
            methodId,
            Guid.Empty,
            OrigamChatMessageDataOrderByCreatedDateSortSetId,
            null,
            parametersMessages
        );
        return OrigamChatMessage.CreateJson(messagesDataSet, allUsers);
    }

    private DataSet LoadData(
        Guid dataStructureId,
        Guid methodId,
        Guid defaultSetId,
        Guid sortSetId,
        string transactionId,
        QueryParameterCollection parameters
    )
    {
        return DataService.Instance.LoadData(
            dataStructureId,
            methodId,
            defaultSetId,
            sortSetId,
            transactionId,
            parameters
        );
    }

    protected IActionResult RunWithErrorHandler(Func<IActionResult> func)
    {
        try
        {
            return func();
        }
        catch (DBConcurrencyException ex)
        {
            log.LogError(ex, ex.Message);
            return StatusCode(409, ex);
        }
        catch (Exception ex)
        {
            if (ex is IUserException)
            {
                return StatusCode(420, ex);
            }
            log.LogOrigamError(ex, ex.Message);
            return StatusCode(500, ex);
        }
    }
}
