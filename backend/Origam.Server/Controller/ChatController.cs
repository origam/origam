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
using Microsoft.AspNetCore.Authorization;
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

[Authorize(Policy = "InternalApi")]
[ApiController]
[Route(template: "chatrooms/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> log;

    private readonly Guid OrigamChatMessageDataStructureId = new Guid(
        g: "f9ec17ce-13fc-420c-88b0-3a793fae4001"
    );
    private readonly Guid OrigamChatMessageDataAfterIdIncludingMethodId = new Guid(
        g: "61544d12-c4ec-4b05-83c7-427b8cc8bb32"
    );
    private readonly Guid OrigamChatMessageDataBeforeIncludingMethodId = new Guid(
        g: "338cca32-f6d2-454f-b4b0-e33a39a1a3cb"
    );
    private readonly Guid OrigamChatMessageDataGetByIdMethodId = new Guid(
        g: "88542750-75e0-4d48-b271-793126cf4847"
    );
    private readonly Guid OrigamChatMessageDataGetByRoomIdMethodId = new Guid(
        g: "42076303-19b4-4afa-9836-d38d87036c29"
    );
    private readonly Guid OrigamChatMessageDataOrderByCreatedDateSortSetId = new Guid(
        g: "72c14783-0667-4827-b4ee-137e17fe0e3e"
    );

    private readonly Guid OrigamChatRoomDatastructureId = new Guid(
        g: "ae1cd694-9354-49bf-a68e-93b1c241afd2"
    );
    private readonly Guid OrigamChatRoomGetById = new Guid(
        g: "a2d56dbe-6807-48ef-9765-7fd8cdaa09c7"
    );

    private readonly Guid OrigamChatRoomBusinessPartnerId = new Guid(
        g: "fab83217-6cb4-4693-b10e-6ded25a23abf"
    );
    private readonly Guid OrigamChatRoomBusinessPartnerGetParticipantsId = new Guid(
        g: "f7d1096f-4dfc-4b66-bef5-f54c7a1d9ee1"
    );
    private readonly Guid OrigamChatRoomBusinessPartner_GetBusinessPartnerId = new Guid(
        g: "5e3d904a-6ad9-4040-ac50-6fee9ee2debb"
    );
    private readonly Guid GetByBusinessPartnerId_IsInvited = new Guid(
        g: "36fd5fb8-5aba-4e87-85c2-e412516888ad"
    );
    private readonly Guid GetByOrigamChatRoomId = new Guid(
        g: "c69ed9ec-df67-4c6e-b99a-726e40316b5e"
    );
    private readonly Guid GetByOrigamChatRoomIdSearch = new Guid(
        g: "fda05fef-8069-41a2-a8fb-d67395e9cea8"
    );

    private readonly Guid LookupBusinessPartner = new Guid(
        g: "d7921e0c-b763-4d07-a019-7e948b4c49a6"
    );
    private readonly Guid DefaultBusinessPartner = new Guid(
        g: "ab4eb78c-6dcf-46c7-a316-e856f835fbfa"
    );
    private readonly Guid LookupBusinessPartnerGetByIdWithoutMe = new Guid(
        g: "34dcbb06-b353-42ff-9f33-f03478eb7ece"
    );
    private readonly Guid LookupBusinessPartnerGetAllUsers = new Guid(
        g: "73bd2418-fc6c-4cf4-b44b-ad2084e25af9"
    );

    private readonly Guid OnlineUsers = new Guid(g: "aa4c9df9-d6da-408e-a095-fd377ffcc319");
    private readonly Guid OrigamChatMessageBusinessPartnerDataStructureId = new Guid(
        g: "763e029f-b306-40bd-98f9-36f89297cfbf"
    );

    public ChatController(ILogger<ChatController> log)
    {
        this.log = log;
    }

    [HttpGet(template: "chatrooms/{requestChatRoomId:guid}/messages")]
    public IActionResult GetMessagesRequest(
        Guid requestChatRoomId,
        [FromQuery] int limit,
        [FromQuery] Guid afterIdIncluding,
        [FromQuery] Guid beforeIdIncluding
    )
    {
        return RunWithErrorHandler(func: () =>
            Ok(
                value: GetMessages(
                    requestChatRoomId: requestChatRoomId,
                    limit: limit,
                    afterIdIncluding: afterIdIncluding,
                    beforeIdIncluding: beforeIdIncluding
                )
            )
        );
    }

    [HttpGet(template: "chatrooms/{requestChatRoomId:guid}/info")]
    public IActionResult GetChatRoomInfoRequest(Guid requestChatRoomId)
    {
        return RunWithErrorHandler(func: () =>
            Ok(value: GetChatRoomInfo(requestChatRoomId: requestChatRoomId))
        );
    }

    [HttpGet(template: "chatrooms/{requestChatRoomId:guid}/participants")]
    public IActionResult GetChatRoomParticipantsRequest(Guid requestChatRoomId)
    {
        return RunWithErrorHandler(func: () =>
            Ok(value: GetChatRoomParticipants(requestChatRoomId: requestChatRoomId))
        );
    }

    //List user for possible invite into Chatroom without exists users.
    [HttpGet(template: "chatrooms/{requestChatRoomId:guid}/usersToInvite")]
    public IActionResult GetUsersToInviteToRoomRequest(
        Guid requestChatRoomId,
        [FromQuery] int limit,
        [FromQuery] int offset,
        [FromQuery] string searchPhrase
    )
    {
        return RunWithErrorHandler(func: () =>
            GetRoomUsers(
                requestChatRoomId: requestChatRoomId,
                limit: limit,
                offset: offset,
                searchPhrase: searchPhrase,
                usersNotExistsInRoom: true
            )
        );
    }

    //List Participant user for outvite
    [HttpGet(template: "chatrooms/{requestChatRoomId:guid}/usersToOutvite")]
    public IActionResult GetUsersToOutviteToRoomRequest(
        Guid requestChatRoomId,
        [FromQuery] int limit,
        [FromQuery] int offset,
        [FromQuery] string searchPhrase
    )
    {
        return RunWithErrorHandler(func: () =>
            GetOutviteRoomUsers(
                requestChatRoomId: requestChatRoomId,
                limit: limit,
                offset: offset,
                searchPhrase: searchPhrase
            )
        );
    }

    //List user invite to chatroom (New Room)
    [HttpGet(template: "users/listToInvite")]
    public IActionResult GetAllUsersToInviteRequest(
        [FromQuery] int limit,
        [FromQuery] int offset,
        [FromQuery] string searchPhrase
    )
    {
        return RunWithErrorHandler(func: () =>
            GetUsersToInviteToNewRoom(limit: limit, offset: offset, searchPhrase: searchPhrase)
        );
    }

    [HttpGet(template: "chatrooms/{requestChatRoomId:guid}/usersToMention")]
    public IActionResult GetUsersToMentionRequest(
        Guid requestChatRoomId,
        [FromQuery] int limit,
        [FromQuery] int offset,
        [FromQuery] string searchPhrase
    )
    {
        return RunWithErrorHandler(func: () =>
            GetRoomUsers(
                requestChatRoomId: requestChatRoomId,
                limit: limit,
                offset: offset,
                searchPhrase: searchPhrase,
                usersNotExistsInRoom: false
            )
        );
    }

    [HttpGet(template: "users")]
    public IActionResult GetLocalUsersRequest()
    {
        return RunWithErrorHandler(func: () => Ok(value: GetLocalUsers()));
    }

    [HttpGet(template: "chatrooms/{requestChatRoomId:guid}/polledData")]
    public IActionResult GetPollDataRequest(
        Guid requestChatRoomId,
        [FromQuery] int limit,
        [FromQuery] Guid afterIdIncluding,
        [FromQuery] Guid beforeIdIncluding
    )
    {
        return RunWithErrorHandler(func: () =>
            GetPollData(
                requestChatRoomId: requestChatRoomId,
                limit: limit,
                afterIdIncluding: afterIdIncluding,
                beforeIdIncluding: beforeIdIncluding
            )
        );
    }

    [HttpGet()]
    public IActionResult GetRoomsRequest()
    {
        return RunWithErrorHandler(func: () => Ok(value: GetRoomsData()));
    }

    [HttpPost(template: "chatrooms/create")]
    public IActionResult CreateRoomRequest([FromBody] NewChatRoom newRoomJson)
    {
        return RunWithErrorHandler(func: () => CreateRoom(newRoomJson: newRoomJson));
    }

    [HttpPost(template: "chatrooms/{requestChatRoomId:guid}/messages")]
    public IActionResult PostMessagesRequest(
        Guid requestChatRoomId,
        [FromBody] OrigamChatMessage chatMessages
    )
    {
        return RunWithErrorHandler(func: () =>
            PostMessages(requestChatRoomId: requestChatRoomId, chatMessages: chatMessages)
        );
    }

    [HttpPost(template: "chatrooms/{requestChatRoomId:guid}/inviteUser")]
    public IActionResult PostInviteUserRequest(
        Guid requestChatRoomId,
        [FromBody] RequestUserId requestId
    )
    {
        return RunWithErrorHandler(func: () =>
            PostInviteUser(requestChatRoomId: requestChatRoomId, userId: requestId.UserId)
        );
    }

    [HttpPost(template: "chatrooms/{requestChatRoomId:guid}/outviteUser")]
    public IActionResult PostRoomAbandonRequest(Guid requestChatRoomId, OutviteUser outviteUser)
    {
        return RunWithErrorHandler(func: () =>
            PostRoomAbandon(requestChatRoomId: requestChatRoomId, outviteUser: outviteUser)
        );
    }

    [HttpPost(template: "chatrooms/{requestChatRoomId:guid}/info")]
    public IActionResult PostRoomAbandonRequest(Guid requestChatRoomId, [FromBody] Info topic)
    {
        return RunWithErrorHandler(func: () =>
            PostRoomChangeTopic(requestChatRoomId: requestChatRoomId, topic: topic)
        );
    }

    private IActionResult PostRoomChangeTopic(Guid requestChatRoomId, Info topic)
    {
        var roomInfo = GetChatRoom(requestChatRoomId: requestChatRoomId);
        var dataRow = roomInfo.Tables[index: 0].Rows[index: 0];
        dataRow[columnName: "Name"] = topic.Topic;
        DataService.Instance.StoreData(
            dataStructureId: OrigamChatRoomDatastructureId,
            data: roomInfo,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
        return Ok();
    }

    private IActionResult CreateRoom(NewChatRoom newRoomJson)
    {
        var newChatRoomId = CreateRoomDatabase(newChatRoom: newRoomJson);
        return Ok(value: newChatRoomId);
    }

    private Guid CreateRoomDatabase(NewChatRoom newChatRoom)
    {
        var profile = SecurityManager.CurrentUserProfile();
        var newChatRoomId = Guid.NewGuid();
        var datasetGenerator = new DatasetGenerator(userDefinedParameters: true);
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var dataStructure = (DataStructure)
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractSchemaItem),
                primaryKey: new ModelElementKey(id: OrigamChatRoomDatastructureId)
            );
        var dataset = datasetGenerator.CreateDataSet(ds: dataStructure);
        var row = dataset.Tables[name: "OrigamChatRoom"].NewRow();
        row[columnName: "Id"] = newChatRoomId;
        row[columnName: "Name"] = newChatRoom.Topic;
        if (
            newChatRoom.ReferenceRecordId.HasValue
            && !string.IsNullOrEmpty(value: newChatRoom.ReferenceCategory)
        )
        {
            row[columnName: "ReferenceId"] = newChatRoom.ReferenceRecordId.Value;
            row[columnName: "ReferenceEntity"] = newChatRoom.ReferenceCategory;
        }
        row[columnName: "RecordCreated"] = DateTime.Now;
        row[columnName: "RecordCreatedBy"] = profile.Id;
        dataset.Tables[name: "OrigamChatRoom"].Rows.Add(row: row);
        DataService.Instance.StoreData(
            dataStructureId: OrigamChatRoomDatastructureId,
            data: dataset,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
        newChatRoom.InviteUsers.Add(item: new InviteUser(userId: profile.Id));
        AddUsersIntoChatRoom(newChatRoomId: newChatRoomId, inviteUsers: newChatRoom.InviteUsers);
        return newChatRoomId;
    }

    private void AddUsersIntoChatRoom(Guid newChatRoomId, List<InviteUser> inviteUsers)
    {
        var profile = SecurityManager.CurrentUserProfile();
        var datasetGenerator = new DatasetGenerator(userDefinedParameters: true);
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var dataStructure = (DataStructure)
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractSchemaItem),
                primaryKey: new ModelElementKey(id: OrigamChatRoomBusinessPartnerId)
            );
        var dataSet = datasetGenerator.CreateDataSet(ds: dataStructure);
        foreach (var inviteUser in inviteUsers)
        {
            var userId = inviteUser.Id;
            var row = dataSet.Tables[name: "OrigamChatRoomBusinessPartner"].NewRow();
            row[columnName: "Id"] = Guid.NewGuid();
            row[columnName: "IsInvited"] = true;
            row[columnName: "refOrigamChatRoomId"] = newChatRoomId;
            row[columnName: "refBusinessPartnerId"] = userId;
            row[columnName: "RecordCreated"] = DateTime.Now;
            row[columnName: "RecordCreatedBy"] = profile.Id;
            dataSet.Tables[name: "OrigamChatRoomBusinessPartner"].Rows.Add(row: row);
        }
        DataService.Instance.StoreData(
            dataStructureId: OrigamChatRoomBusinessPartnerId,
            data: dataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
    }

    private IActionResult PostRoomAbandon(Guid requestChatRoomId, OutviteUser outviteUser)
    {
        var datasetUsersForInvite = GetActiveUserChatRoom(
            requestChatRoomId: requestChatRoomId,
            outviteUser: outviteUser
        );
        if (datasetUsersForInvite != null)
        {
            datasetUsersForInvite.Tables[index: 0].Rows[index: 0].Delete();
            DataService.Instance.StoreData(
                dataStructureId: OrigamChatRoomBusinessPartnerId,
                data: datasetUsersForInvite,
                loadActualValuesAfterUpdate: false,
                transactionId: null
            );
        }
        return Ok();
    }

    private DataSet GetActiveUserChatRoom(Guid requestChatRoomId, OutviteUser outviteUser)
    {
        var parameters = new QueryParameterCollection
        {
            new QueryParameter(
                _parameterName: "OrigamChatRoomBusinessPartner_parBusinessPartnerId",
                value: outviteUser.UserId
            ),
            new QueryParameter(
                _parameterName: "OrigamChatRoomBusinessPartner_parOrigamChatRoomId",
                value: requestChatRoomId
            ),
        };
        var resultData = LoadData(
            dataStructureId: OrigamChatRoomBusinessPartnerId,
            methodId: OrigamChatRoomBusinessPartner_GetBusinessPartnerId,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: parameters
        );
        return resultData.Tables[index: 0].Rows.Count == 0 ? null : resultData;
    }

    private IActionResult PostInviteUser(Guid requestChatRoomId, Guid userId)
    {
        var users = new List<InviteUser> { new InviteUser(userId: userId) };
        AddUsersIntoChatRoom(newChatRoomId: requestChatRoomId, inviteUsers: users);
        return Ok();
    }

    private IActionResult GetUsersToInviteToNewRoom(int limit, int offset, string searchPhrase)
    {
        var methodId = LookupBusinessPartnerGetByIdWithoutMe;
        QueryParameterCollection parameters = new QueryParameterCollection();
        if (!string.IsNullOrEmpty(value: searchPhrase))
        {
            parameters.Add(
                value: new QueryParameter(
                    _parameterName: "BusinessPartner_parSearchText",
                    value: searchPhrase
                )
            );
            methodId = DefaultBusinessPartner;
        }
        if (limit > 0)
        {
            parameters.Add(
                value: new QueryParameter(
                    _parameterName: "BusinessPartner__pageNumber",
                    value: offset
                )
            );
            parameters.Add(
                value: new QueryParameter(_parameterName: "BusinessPartner__pageSize", value: limit)
            );
        }
        var datasetUsersForInvite = LoadData(
            dataStructureId: LookupBusinessPartner,
            methodId: methodId,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: parameters
        );
        return Ok(
            value: OrigamChatBusinessPartner.CreateJson(
                datasetUsersForInvite: datasetUsersForInvite,
                participants: null
            )
        );
    }

    private IActionResult GetOutviteRoomUsers(
        Guid requestChatRoomId,
        int limit,
        int offset,
        string searchPhrase
    )
    {
        var participants = GetChatRoomParticipants(requestChatRoomId: requestChatRoomId);
        var methodId = GetByOrigamChatRoomId;
        var parameters = new QueryParameterCollection
        {
            new QueryParameter(
                _parameterName: "OrigamChatRoomBusinessPartner_parOrigamChatRoomId",
                value: requestChatRoomId
            ),
        };
        if (!string.IsNullOrEmpty(value: searchPhrase))
        {
            parameters.Add(
                value: new QueryParameter(
                    _parameterName: "BusinessPartnerLookup_parSearchText",
                    value: searchPhrase
                )
            );
            methodId = GetByOrigamChatRoomIdSearch;
        }
        if (limit > 0)
        {
            parameters.Add(
                value: new QueryParameter(
                    _parameterName: "BusinessPartner__pageNumber",
                    value: offset
                )
            );
            parameters.Add(
                value: new QueryParameter(_parameterName: "BusinessPartner__pageSize", value: limit)
            );
        }
        var datasetUsersForInvite = LoadData(
            dataStructureId: OrigamChatRoomBusinessPartnerId,
            methodId: methodId,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: parameters
        );
        return Ok(
            value: OrigamChatBusinessPartner.CreateJson(
                datasetUsersForInvite: datasetUsersForInvite,
                participants: participants,
                usersNotExistsInRoom: false
            )
        );
    }

    private IActionResult GetRoomUsers(
        Guid requestChatRoomId,
        int limit,
        int offset,
        string searchPhrase,
        bool usersNotExistsInRoom
    )
    {
        var participants = GetChatRoomParticipants(requestChatRoomId: requestChatRoomId);
        var methodId = usersNotExistsInRoom
            ? LookupBusinessPartnerGetByIdWithoutMe
            : LookupBusinessPartnerGetAllUsers;
        var parameters = new QueryParameterCollection();
        if (!string.IsNullOrEmpty(value: searchPhrase))
        {
            parameters.Add(
                value: new QueryParameter(
                    _parameterName: "BusinessPartner_parSearchText",
                    value: searchPhrase
                )
            );
            methodId = DefaultBusinessPartner;
        }
        if (limit > 0)
        {
            parameters.Add(
                value: new QueryParameter(
                    _parameterName: "BusinessPartner__pageNumber",
                    value: offset
                )
            );
            parameters.Add(
                value: new QueryParameter(_parameterName: "BusinessPartner__pageSize", value: limit)
            );
        }
        var datasetUsersForInvite = LoadData(
            dataStructureId: LookupBusinessPartner,
            methodId: methodId,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: parameters
        );
        return Ok(
            value: OrigamChatBusinessPartner.CreateJson(
                datasetUsersForInvite: datasetUsersForInvite,
                participants: participants,
                usersNotExistsInRoom: usersNotExistsInRoom
            )
        );
    }

    private List<OrigamChatRoom> GetRoomsData()
    {
        var profile = SecurityManager.CurrentUserProfile();
        var parameters = new QueryParameterCollection
        {
            new QueryParameter(
                _parameterName: "OrigamChatRoomBusinessPartner_parBusinessPartnerId",
                value: profile.Id
            ),
            new QueryParameter(
                _parameterName: "OrigamChatRoomBusinessPartner_parIsInvited",
                value: true
            ),
        };
        var chatRooms = LoadData(
            dataStructureId: OrigamChatRoomBusinessPartnerId,
            methodId: GetByBusinessPartnerId_IsInvited,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: parameters
        );
        var unreadMessages = GetUnreadMessages(chatRooms: chatRooms);
        return OrigamChatRoom.CreateJson(
            ChatRoomDataSet: chatRooms,
            unreadMessages: unreadMessages
        );
    }

    private Dictionary<Guid, int> GetUnreadMessages(DataSet chatRooms)
    {
        var unreadMessages = new Dictionary<Guid, int>();
        foreach (DataRow dataRow in chatRooms.Tables[name: "OrigamChatRoomBusinessPartner"].Rows)
        {
            var parameters = new QueryParameterCollection
            {
                new QueryParameter(
                    _parameterName: "OrigamChatMessage_parCreatedDateTime",
                    value: dataRow.Field<DateTime?>(columnName: "LastSeen")
                        ?? new DateTime(year: 1900, month: 1, day: 1)
                ),
                new QueryParameter(
                    _parameterName: "OrigamChatMessage_parOrigamChatRoomId",
                    value: dataRow.Field<Guid>(columnName: "refOrigamChatRoomId")
                ),
            };
            var datasetRoom = LoadData(
                dataStructureId: OrigamChatMessageDataStructureId,
                methodId: OrigamChatMessageDataAfterIdIncludingMethodId,
                defaultSetId: Guid.Empty,
                sortSetId: Guid.Empty,
                transactionId: null,
                parameters: parameters
            );
            unreadMessages.Add(
                key: dataRow.Field<Guid>(columnName: "refOrigamChatRoomId"),
                value: datasetRoom.Tables[name: "OrigamChatMessage"].Rows.Count
            );
        }
        return unreadMessages;
    }

    private List<OrigamChatBusinessPartner> GetLocalUsers()
    {
        var datasetUsersForInvite = LoadData(
            dataStructureId: LookupBusinessPartner,
            methodId: Guid.Empty,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: null
        );
        return OrigamChatBusinessPartner.CreateJson(
            datasetUsersForInvite: datasetUsersForInvite,
            participants: null
        );
    }

    private object GetLocalUser()
    {
        var profile = SecurityManager.CurrentUserProfile();
        return new OrigamChatBusinessPartner(
            guid: profile.Id,
            name: profile.FullName,
            avatarurl: profile.Id.ToString()
        );
    }

    private List<OrigamChatParticipant> GetChatRoomParticipants(Guid requestChatRoomId)
    {
        var parameters = new QueryParameterCollection
        {
            new QueryParameter(
                _parameterName: "OrigamChatRoomBusinessPartner_parOrigamChatRoomId",
                value: requestChatRoomId
            ),
            new QueryParameter(
                _parameterName: "OrigamChatRoomBusinessPartner_parIsInvited",
                value: true
            ),
        };
        var datasetParticipants = LoadData(
            dataStructureId: OrigamChatRoomBusinessPartnerId,
            methodId: OrigamChatRoomBusinessPartnerGetParticipantsId,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: parameters
        );
        var onlineUsers = LoadData(
            dataStructureId: OnlineUsers,
            methodId: Guid.Empty,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: null
        );
        return OrigamChatParticipant.CreateJson(
            datasetParticipants: datasetParticipants,
            onlineUsers: onlineUsers
        );
    }

    private OrigamChatRoom GetChatRoomInfo(Guid requestChatRoomId)
    {
        var datasetGetById = GetChatRoom(requestChatRoomId: requestChatRoomId);
        return OrigamChatRoom.CreateJson(
            ChatRoomDataSet: datasetGetById,
            unreadMessages: new Dictionary<Guid, int>()
        )[index: 0];
    }

    private DataSet GetChatRoom(Guid requestChatRoomId)
    {
        var parameters = new QueryParameterCollection
        {
            new QueryParameter(_parameterName: "OrigamChatRoom_parId", value: requestChatRoomId),
        };
        var datasetRoom = LoadData(
            dataStructureId: OrigamChatRoomDatastructureId,
            methodId: OrigamChatRoomGetById,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: parameters
        );
        return datasetRoom;
    }

    private IActionResult PostMessages(Guid requestChatRoomId, OrigamChatMessage chatMessages)
    {
        var profile = SecurityManager.CurrentUserProfile();
        var datasetGenerator = new DatasetGenerator(userDefinedParameters: true);
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var messageDataStructure = (DataStructure)
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractSchemaItem),
                primaryKey: new ModelElementKey(id: OrigamChatMessageDataStructureId)
            );
        var messageDataset = datasetGenerator.CreateDataSet(ds: messageDataStructure);
        var messageRow = messageDataset.Tables[name: "OrigamChatMessage"].NewRow();
        messageRow[columnName: "Id"] = chatMessages.Id;
        messageRow[columnName: "TextMessage"] = chatMessages.Text;
        messageRow[columnName: "refOrigamChatRoomId"] = requestChatRoomId;
        messageRow[columnName: "RecordCreated"] = DateTime.Now;
        messageRow[columnName: "RecordCreatedBy"] = profile.Id;
        messageRow[columnName: "refBusinessPartnerId"] = profile.Id;
        messageRow[columnName: "Mentions"] = chatMessages.Mentions.Count;
        messageDataset.Tables[name: "OrigamChatMessage"].Rows.Add(row: messageRow);
        DataService.Instance.StoreData(
            dataStructureId: OrigamChatMessageDataStructureId,
            data: messageDataset,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
        var messageBusinessPartnerDataStructure = (DataStructure)
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractSchemaItem),
                primaryKey: new ModelElementKey(id: OrigamChatMessageBusinessPartnerDataStructureId)
            );
        var messageBusinessPartnerDataSet = datasetGenerator.CreateDataSet(
            ds: messageBusinessPartnerDataStructure
        );
        foreach (var messageMention in chatMessages.Mentions)
        {
            var messageBusinessPartnerRow = messageBusinessPartnerDataSet
                .Tables[name: "OrigamChatMessageBusinessPartner"]
                .NewRow();
            messageBusinessPartnerRow[columnName: "Id"] = Guid.NewGuid();
            messageBusinessPartnerRow[columnName: "RecordCreated"] = DateTime.Now;
            messageBusinessPartnerRow[columnName: "RecordCreatedBy"] = profile.Id;
            messageBusinessPartnerRow[columnName: "refBusinessPartnerId"] = messageMention;
            messageBusinessPartnerRow[columnName: "refOrigamChatMessageId"] = chatMessages.Id;
            messageBusinessPartnerDataSet
                .Tables[name: "OrigamChatMessageBusinessPartner"]
                .Rows.Add(row: messageBusinessPartnerRow);
        }
        DataService.Instance.StoreData(
            dataStructureId: OrigamChatMessageBusinessPartnerDataStructureId,
            data: messageBusinessPartnerDataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
        CreateLastSeen(requestChatRoomId: requestChatRoomId);
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
        var participants = GetChatRoomParticipants(requestChatRoomId: requestChatRoomId);
        var activeUser = (OrigamChatBusinessPartner)GetLocalUser();
        if (participants.All(predicate: participant => participant.Id != activeUser.id))
        {
            return StatusCode(statusCode: 403, value: "You are not allowed to join this chatroom.");
        }
        pollData.Add(
            key: "messages",
            value: GetMessages(
                requestChatRoomId: requestChatRoomId,
                limit: limit,
                afterIdIncluding: afterIdIncluding,
                beforeIdIncluding: beforeIdIncluding
            )
        );
        pollData.Add(key: "localUser", value: activeUser);
        pollData.Add(key: "participants", value: participants);
        pollData.Add(key: "info", value: GetChatRoomInfo(requestChatRoomId: requestChatRoomId));
        CreateLastSeen(requestChatRoomId: requestChatRoomId);
        return Ok(value: pollData);
    }

    private void CreateLastSeen(Guid requestChatRoomId)
    {
        var profile = SecurityManager.CurrentUserProfile();
        var inviteUser = new OutviteUser(id: profile.Id);
        var userDataset = GetActiveUserChatRoom(
            requestChatRoomId: requestChatRoomId,
            outviteUser: inviteUser
        );
        var userRow = userDataset.Tables[index: 0].Rows[index: 0];
        userRow[columnName: "LastSeen"] = DateTime.Now;
        DataService.Instance.StoreData(
            dataStructureId: OrigamChatRoomBusinessPartnerId,
            data: userDataset,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
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
                new QueryParameter(_parameterName: "OrigamChatMessage_parId", value: includingId),
            };
            var getRecordCreated = LoadData(
                dataStructureId: OrigamChatMessageDataStructureId,
                methodId: OrigamChatMessageDataGetByIdMethodId,
                defaultSetId: Guid.Empty,
                sortSetId: Guid.Empty,
                transactionId: null,
                parameters: parameters
            );
            parametersMessages.Add(
                value: new QueryParameter(
                    _parameterName: "OrigamChatMessage_parCreatedDateTime",
                    value: getRecordCreated.Tables[index: 0].Rows[index: 0][
                        columnName: "RecordCreated"
                    ]
                )
            );
        }
        parametersMessages.Add(
            value: new QueryParameter(
                _parameterName: "OrigamChatMessage_parOrigamChatRoomId",
                value: requestChatRoomId
            )
        );
        if (limit > 0)
        {
            parametersMessages.Add(
                value: new QueryParameter(_parameterName: "OrigamChatMessage__pageNumber", value: 0)
            );
            parametersMessages.Add(
                value: new QueryParameter(
                    _parameterName: "OrigamChatMessage__pageSize",
                    value: limit
                )
            );
        }
        var messagesDataSet = LoadData(
            dataStructureId: OrigamChatMessageDataStructureId,
            methodId: methodId,
            defaultSetId: Guid.Empty,
            sortSetId: OrigamChatMessageDataOrderByCreatedDateSortSetId,
            transactionId: null,
            parameters: parametersMessages
        );
        return OrigamChatMessage.CreateJson(MessagesDataSet: messagesDataSet, allusers: allUsers);
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
            dataStructureId: dataStructureId,
            methodId: methodId,
            defaultSetId: defaultSetId,
            sortSetId: sortSetId,
            transactionId: transactionId,
            parameters: parameters
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
            log.LogError(exception: ex, message: ex.Message);
            return StatusCode(statusCode: 409, value: ex);
        }
        catch (Exception ex)
        {
            if (ex is IUserException)
            {
                return StatusCode(statusCode: 420, value: ex);
            }
            log.LogOrigamError(ex: ex, message: ex.Message);
            return StatusCode(statusCode: 500, value: ex);
        }
    }
}
