# Application startup

The app opens by navigating to:

`#/chatroom?chatroomId=...&referenceX=...&referenceY=...&...`

## Joining an existing chatroom

When `chatroomId` is given, the chat thread interface is displayed and the user can start chatting. All the endpoints given in protocol description - when meaningful - are given the `chatroomId` to bind the operations and queries to the specific chatroom.

## Creating new chatroom

When there is no user in the chatroom the app expects to be started with `chatroomId` empty or not given at all. When this happens the app:

 1. displays a dialog, where the user is expected to fill in the name of chatroom to be created and select users he or she wants to invite to the chatroom from a list obtained by calling:
 
    `GET users/listToInvite?referenceX=...&referenceY=...&...` 

    All the query parameters, beginning with `reference` prefix are passed to the endpoint as they were given when the app was started so that the list of the users can possibly be e.g. serverside-filtered according to the object the chatroom shall be bound to. The result is expected to be in the exact same form as for `GET chatrooms/:chatroomId/usersToInvite` endpoint. The output is expected not to contain the current user.

 2. calls `POST /chatrooms/create` after user submits the dialog. It is expected to:
    - Create new chatroom on server in the app domain (whatever it means in practice)
    - Set the chatroom topic/name
    - Invite current user to the chatroom so that he or she can then read/send messages and otherwise interact with the chatroom
    - Invite users given in the request body so the can do the same
  
    The method returns the newly created `chatroomId`.

 3. App then redirects itself to `#/chatroom?chatroomId=...` with references stripped away and `chatroomId` set as given in the previous point. The proccess then continues as stated in *Joining an existing chatroom* section.

# Hashtag functionality

The application allows inserting links into messages being sent and also joining them to the chatroom itself. The links, when navigated, will open linked view / record in the main client.

## The hashtag dialog

Choosing of the entity / record being linked is performed in a dialog screen, basically it is a screen with two data views, the top one allows for choosing a *category*, which is usually an entity type to be linked and the bottom one is an *object* which offers rows of given entity to be linked.

The top view receives rows filtered by given category search phrase, limited and offsetted as needed.

The bottom view receives rows filtered by given search phrase, limited and offsetted as well as filtered by a category choosen from the top view. The bottom view essentially uses different metadata (e.g. displayed columns) for the category choosen.

## Hashtag HTTP API

### Getting hashtag dialog metadata

`GET hashtag/metadata`

Return value:

 - `categoriesMetadata` - Data sources / combobox description similarily as available in InitUI 
 - `objectsMetadata` - Metadata mapping from categoryId to metadata record given similarily as in InitUI screen description
   - *categoryId* - Metadata for given category

### Getting available categories

`GET hashtag/categories`

Query parameters:

 - `searchPhrase`- *(optional)* - string used to fulltext constrain the query to just users of interrest.
 - `limit` - *(optional)* number limiting number of items returned.
 - `offset` - *(optional)* number offset where list should begin.

Return value: JSON object:

 - `data` - 2D array of arbitrary content, representing a set of categories, complying the query and metadata


### Getting available objects for given category

`GET hashtag/objects`

Query parameters

 - `categoryId` - string used to filter the result to contain only items from specified category
 - `searchPhrase`- *(optional)* - string used to fulltext constrain the query to just users of interrest.
 - `limit` - *(optional)* number limiting number of items returned.
 - `offset` - *(optional)* number offset where list should begin.

Return value: JSON object:

 - `data` - 2D array of arbitrary content, representing a set of objects, complying the query and metadata



### Composing a link ???

`POST hashtag/link`

POST body:

 - `objects` [

   - `categoryId` - category id of link to be created
   - `objectId` - object id of link to be created
 - ] 

Return value: JSON object:

 - `objects` [
   - `categoryId` - category id of created link
   - `objectId` - object id of created link
   - `text` - textual representation of created link
   - `url` - ??? url of the link
 - ]

### Adding a link to the chatroom

`POST chatrooms/:chatroomId/setLinks`

POST body:

 - `objects` [

   - `categoryId` - category id of link to be linked
   - `objectId` - object id of link to be linked
 - ]



## UNRESOLVED

 - Link text change? (getLookupLabels...)




# Origam chat HTTP protocol description


`POST chatrooms/create`

Description:
> Create a chatroom, set its name and invite users to it.

Request body: JSON object:


  
  - `topic` - GUID identifier of the message 
  - `references` - References passed from application url (all the fields beginning by `reference` prefix)
    - `referenceA` - ...
    - `referenceB` - ...
  - `inviteUsers`- array of users to invite to the chatroom [

    - `id` - GUID user identifier.

  - ] 

Return value: JSON object:

 - `chatroomId` - GUID of the newly created chatroom.

Example:

App started by url:

`#/chatroom?referenceId=abc123&referenceEntity=SomeEntityName&referenceOther=other_reference_data`

will consequently make request semantically similar to:

```
POST chatrooms/create

{
  "topic" : "Bugs in version 2029.13",
  "references": {
    "referenceId": "abc123",
    "referenceEntity": "SomeEntityName",
    "referenceOther": "other_reference_data"
  },
  "inviteUsers": [
    "guid-01-ab-cd",
    "guid-02-ef-gh"
  ]
}

```

***

`GET users/listToInvite`


Description:
> Lists users which can be invited to the given channel, filtering them by given phrase, limiting the returned item count. 

Query parameters:

 - `referenceA`, `referenceB`, ... - strings passed as given in the app bootstrap url
 - `limit` - *(optional)* number limiting number of users returned.
 - `offset` - *(optional)* number offset where list should begin.
 - `searchPhrase` - *(optional)* - string used to fultext constrain the query to just users of interrest.

Return value: flat JSON array:

[

   - `id` - GUID user identifier.
   - `name` - string user name.
   - `avatarUrl` - string url of an image to display as the participant's avatar.

]



***




`GET chatrooms/:chatroomId/messages`

Description:
> The method queries given chatroom for the messages. Messages are returned in ascending order, ordered by the time when they were sent.

URL parameters:
 
 - `chatroomId` - GUID chatroom id from which to return the messages.

Query parameters:
 
 - `limit` - *(optional)* number limiting number of messages returned.
 - `afterIdIncluding` - *(optional)* GUID filtering returned messages to contain only those as old as or older than the one with given id.
 - `beforeIdIncluding` - *(optional)* GUID filtering returned messages to contain only those as old as or newer than the one with given id.

Return value: JSON array:

[
   
   - `id` - GUID message identifier.
   - `authorId` - GUID identifier of message sender.
   - `authorName` - string human readable text to display as a message sender's name.
   - `authorAvatarUrl` - string url of a picture used as a user's avatar in the chat log.
   - `timeSent` - string ISO time when the message was received by the server.
   - `text` - string textual content of the message.
   - `mentions`- array of users mentioned in the message [
      
      - `id` - GUID user identifier.
      - `name` - string user name.
      - `avatarUrl` - string url of an image to display as the participant's avatar.

   - ] 

]

***



`POST chatrooms/:chatroomId/messages`

Description:
> Posts the message into given chatroom.

URL parameters:

 - `chatroomId` - GUID chatroom id to which the message should be inserted.

Request body: JSON object:


  
  - `id` - GUID identifier of the message
  - `text` - Textual context of the message 
  - `mentions`- array of users to mention in the message [

    - `id` - GUID user identifier.

  - ] 



 ***



 `GET chatrooms/:chatroomId/info`

Description:
> Obtains information about the chatroom.

URL parameters:

 - `chatroomId` - GUID chatroom id to get the information about.

Return value: JSON object:

 - `topic` - string human readable text to display as the chatroom's name / title
 - `links` [
   - `categoryId` - Linked category id
   - `objectId` - Linked object id
   - `url` - URL of the link
   - `text` - Textual representation of the link
 - ]

***



 `PATCH chatrooms/:chatroomId/info`

Description:
> Changes information about the chatroom.

URL parameters:

 - `chatroomId` - GUID chatroom id to update the information about.

Request body: JSON object:

 - `topic` - string human readable text to display as the chatroom's name / title

***



`GET chatrooms/:chatroomId/participants`

Description:
> Get list of people involved in the chatroom.

URL parameters:

 - `chatroomId` - GUID chatroom id to which the message should be inserted.

Return value: flat JSON array:

[

   - `id` - GUID user identifier.
   - `name` - string user name.
   - `avatarUrl` - string url of an image to display as the participant's avatar.
   - `status` - string status specifier. One of `online`, `away`, `offline`, `none` .

]

***



`POST chatrooms/:chatroomId/inviteUser`

Description:
> Invite given user to the chatroom

URL parameters:

 - `chatroomId` - GUID chatroom id to which the user should be invited.

Request body: JSON object:

 - `userId` - GUID identifier of user to be invited.

***



`POST chatrooms/:chatroomId/inviteUsers`

Description:
> Invite given users to the chatroom

URL parameters:

 - `chatroomId` - GUID chatroom id to which the user should be invited.

Request body: array od JSON objects:

[

  - `userId` - GUID identifier of user to be invited.

]

***



`GET chatrooms/:chatroomId/usersToInvite`

Description:
> Lists users which can be invited to the given channel, filtering them by given phrase, limiting the returned item count. 

URL parameters:

 - `chatroomId` - GUID chatroom id where the users are searched to invite to.

Query parameters:

 - `limit` - *(optional)* number limiting number of users returned.
 - `offset` - *(optional)* number offset where list should begin.
 - `searchPhrase` - *(optional)* - string used to fultext constrain the query to just users of interrest.

Return value: flat JSON array:

[

   - `id` - GUID user identifier.
   - `name` - string user name.
   - `avatarUrl` - string url of an image to display as the participant's avatar.

]

***



`POST chatrooms/:chatroomId/abandon`

Description:
> Abandon given chatroom, clearing the user's invitation. User is no longer able to enter the chatroom unless invited again.

URL parameters:

 - `chatroomId` - GUID chatroom id which the user wants to abandon.

***



`GET chatrooms/:chatroomId/usersToMention`

Description:
> Lists users which can be mentioned in the chatroom given. 

URL parameters:

 - `chatroomId` - GUID chatroom id where the users are searched to be mentioned in.

Query parameters:

 - `limit` - *(optional)* number limiting number of users returned.
 - `offset` - *(optional)* number offset where list should begin.
 - `searchPhrase` - *(optional)* - string used to fultext constrain the query to just users of interrest.

Return value: flat JSON array:

[

   - `id` - GUID user identifier.
   - `name` - string user name.
   - `avatarUrl` - string url of an image to display as the participant's avatar.

]

***



`GET localUser`

Description:
> Obtain information about the user chatting in this session. The user's requests are correlated by a session id cookie or an authentication token.

Return value: JSON object:

 - `id` - GUID identifier of the user.
 - `name` - string human readable name of the user.
 - `avatarUrl` - string url of an image to be used as the user's avatar.

***



`GET chatrooms/:chatroomId/polledData`

Description:
> The method is used to poll for various chatroom data. It returns merged data from following endpoints:

 - `GET chatrooms/:chatroomId/messages`
 - `GET chatrooms/:chatroomId/info`
 - `GET chatrooms/:chatroomId/participants`
 - `GET localUser`

The method's query and url parameters have the same meaning as for the original endpoints, see the endpoints' descriptions.

Return value: JSON object:

 - `messages` - contains the response of corresponding endpoint
 - `info` - contains the response of corresponding endpoint
 - `participants` - contains the response of corresponding endpoint
 - `localUser` - contains the response of corresponding endpoint
