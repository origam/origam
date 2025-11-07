// #region license
//
// /*
// Copyright 2005 - 2023 Advantage Solutions, s. r. o.
//
// This file is part of ORIGAM (http://www.origam.org).
//
// ORIGAM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// ORIGAM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
// */
//
// #endregion
//
// using System;
// using System.Collections;
// using System.Linq;
// using System.Net.Http;
// using System.Threading.Tasks;
// using IdentityModel.Client;
// using Microsoft.Extensions.Configuration;
// using Origam.Server.Configuration;
// using Origam.Service.Core;
//
// namespace Origam.Server.ClientAuthentication;
//
// public class ResourceOwnerPasswordAuthenticationProvider : IClientAuthenticationProvider
// {
//     private ResourceOwnerPasswordAuthenticationProviderConfig providerConfig;
//     private AccessToken accessToken;
//     private readonly object lockObj = new();
//
//     public bool TryAuthenticate(string url, Hashtable headers)
//     {
//         bool canHandle = providerConfig.UrlsToBeAuthenticated.Any(url.StartsWith);
//         if (!canHandle)
//         {
//             return false;
//         }
//
//         lock (lockObj)
//         {
//             if (accessToken == null || accessToken.IsExpired)
//             {
//                 Task<AccessToken> tokenTask = GetAccessToken();
//                 tokenTask.Wait();
//                 accessToken = tokenTask.Result;
//             }
//         }
//
//         headers.Add("Authorization", $"Bearer {accessToken.Value}");
//         return true;
//     }
//
//     private async Task<AccessToken> GetAccessToken()
//     {
//         var client = new HttpClient();
//         var discovery = await client.GetDiscoveryDocumentAsync(providerConfig.AuthServerUrl);
//         if (discovery.IsError)
//         {
//             throw new Exception(discovery.Error);
//         }
//
//         var response = await client.RequestPasswordTokenAsync(
//             new PasswordTokenRequest
//             {
//                 Address = discovery.TokenEndpoint,
//                 ClientId = providerConfig.ClientId,
//                 ClientSecret = providerConfig.ClientSecret,
//                 UserName = providerConfig.UserName,
//                 Password = providerConfig.Password,
//             }
//         );
//
//         if (response.IsError)
//         {
//             throw new Exception(response.Error);
//         }
//
//         var expiresIn = GetValue("expires_in", response);
//         if (!int.TryParse(expiresIn, out var expirationSeconds))
//         {
//             throw new Exception($"Cannot parse expires_in value \"{expiresIn}\" to integer");
//         }
//
//         return new AccessToken(
//             value: GetValue("access_token", response),
//             lifeTime: TimeSpan.FromSeconds(expirationSeconds)
//         );
//     }
//
//     private string GetValue(string key, TokenResponse response)
//     {
//         var accessJToken = response.Json[key];
//         if (accessJToken == null)
//         {
//             throw new Exception(
//                 $"{key} was not found in response from {providerConfig.AuthServerUrl}"
//             );
//         }
//
//         return accessJToken.ToString();
//     }
//
//     public void Configure(IConfiguration configuration)
//     {
//         providerConfig = new ResourceOwnerPasswordAuthenticationProviderConfig(configuration);
//     }
// }
//
// class AccessToken
// {
//     public string Value { get; }
//     public DateTime Expiration { get; }
//     private readonly TimeSpan lifeTime;
//
//     public AccessToken(string value, TimeSpan lifeTime)
//     {
//         this.lifeTime = lifeTime;
//         Value = value;
//         Expiration = DateTime.Now.Add(lifeTime);
//     }
//
//     public bool IsExpired =>
//         lifeTime < TimeSpan.FromMinutes(10)
//             ? DateTime.Now - Expiration < lifeTime.Divide(4)
//             : DateTime.Now - Expiration < TimeSpan.FromMinutes(10);
// }
