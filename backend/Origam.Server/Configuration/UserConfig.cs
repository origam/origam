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

namespace Origam.Server.Configuration
{
    public class UserConfig
    {
        public string FromAddress { get; set; }
        public string NewUserRoleId { get; set; }
        public string UserUnlockNotificationSubject { get; set; }
        public string UserUnlockNotificationBodyFileName { get; set; }
        public string UserRegistrationMailSubject { get; set; }
        public string UserRegistrationMailBodyFileName { get; set; }
        public string MailQueueName { get; set; }
        public bool UserRegistrationAllowed { get; set; }
        public string PortalBaseUrl { get; set; }
        public string MultiFactorMailSubject { get; set; }
        public string MultiFactorMailBodyFileName { get; set; }
        public string AllowedUserNameCharacters { get; set; }
    }
}