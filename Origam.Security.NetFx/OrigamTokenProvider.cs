#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNet.Identity;
using IDataProtector = Microsoft.Owin.Security.DataProtection.IDataProtector;

namespace Origam.Security.Identity
{
    // based on DataProtectorTokenProvider
    public class OrigamTokenProvider : IUserTokenProvider<OrigamUser, string>
    {
        protected static readonly ILog log 
            = LogManager.GetLogger(typeof(OrigamTokenProvider));
        public IDataProtector Protector { get; private set; }
        public TimeSpan TokenLifespan { get; set; }

        public OrigamTokenProvider(IDataProtector protector)
        {
            if (protector == null)
            {
                throw new ArgumentNullException("protector");
            }
            Protector = protector;
            TokenLifespan = TimeSpan.FromDays(1);
        }

        public async Task<string> GenerateAsync(string purpose, UserManager<OrigamUser, string> manager, OrigamUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            MemoryStream ms = new MemoryStream();
            using (var writer = ms.CreateWriter())
            {
                writer.Write(DateTimeOffset.UtcNow);
                writer.Write(Convert.ToString(user.BusinessPartnerId, CultureInfo.InvariantCulture));
                writer.Write(purpose ?? "");
                string stamp = null;
                if (manager.SupportsUserSecurityStamp)
                {
                    stamp = await manager.GetSecurityStampAsync(user.BusinessPartnerId);
                }
                writer.Write(stamp ?? "");
            }
            byte[] protectedBytes = Protector.Protect(ms.ToArray());
            return Convert.ToBase64String(protectedBytes);
        }

        public Task<bool> IsValidProviderForUserAsync(UserManager<OrigamUser, string> manager, OrigamUser user)
        {
            throw new NotImplementedException();
        }

        public Task NotifyAsync(string token, UserManager<OrigamUser, string> manager, OrigamUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ValidateAsync(
            string purpose, string token, 
            UserManager<OrigamUser, string> manager, OrigamUser user)
        {
            try
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Validating token: {0}", token);
                }
                byte[] unprotectedData = Protector.Unprotect(Convert.FromBase64String(token));
                MemoryStream ms = new MemoryStream(unprotectedData);
                using (var reader = ms.CreateReader())
                {
                    DateTimeOffset creationTime = reader.ReadDateTimeOffset();
                    DateTimeOffset expirationTime = creationTime + TokenLifespan;
                    if (expirationTime < DateTimeOffset.UtcNow)
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug("Token's expired.");
                        }
                        return false;
                    }
                    string userId = reader.ReadString();
                    if (!userId.Equals(user.BusinessPartnerId))
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug("Invalid user id.");
                        }
                        return false;
                    }
                    string purp = reader.ReadString();
                    if (!String.Equals(purp, purpose))
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug("Invalid purpose.");
                        }
                        return false;
                    }
                    string stamp = reader.ReadString();
                    if (reader.PeekChar() != -1)
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug("Invalid token length.");
                        }
                        return false;
                    }
                    if (manager.SupportsUserSecurityStamp)
                    {
                        var expectedStamp = await manager.GetSecurityStampAsync(user.BusinessPartnerId).ConfigureAwait(false);
                        if (stamp != (expectedStamp ?? ""))
                        {
                            if (log.IsDebugEnabled)
                            {
                                log.Debug("Security token doesn't match.");
                            }
                            return false;
                        } else {
                            if (log.IsDebugEnabled)
                            {
                                log.Debug("Token is valid.");
                            }
                            return true;
                        }
                    }
                    else
                    {
                        if (stamp != "")
                        {
                            if (log.IsDebugEnabled)
                            {
                                log.Debug("Security token doesn't match.");
                            }
                            return false;
                        } else {
                            if (log.IsDebugEnabled)
                            {
                                log.Debug("Token is valid.");
                            }
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            { 
                if (log.IsWarnEnabled)
                {
                    log.Warn("Token validation failed.", ex);
                }
            }
            return false;
        }
    }

    // Based on Levi's authentication sample
    internal static class StreamExtensions
    {
        internal static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        public static BinaryReader CreateReader(this Stream stream)
        {
            return new BinaryReader(stream, DefaultEncoding, leaveOpen: true);
        }

        public static BinaryWriter CreateWriter(this Stream stream)
        {
            return new BinaryWriter(stream, DefaultEncoding, leaveOpen: true);
        }

        public static DateTimeOffset ReadDateTimeOffset(this BinaryReader reader)
        {
            return new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
        }

        public static void Write(this BinaryWriter writer, DateTimeOffset value)
        {
            writer.Write(value.UtcTicks);
        }
    }
}
