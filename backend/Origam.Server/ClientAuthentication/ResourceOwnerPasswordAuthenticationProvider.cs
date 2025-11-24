#region license

/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Origam.Server.Configuration;
using Origam.Service.Core;

namespace Origam.Server.ClientAuthentication;

public class ResourceOwnerPasswordAuthenticationProvider : IClientAuthenticationProvider
{
    private ResourceOwnerPasswordAuthenticationProviderConfig providerConfig;
    private AccessToken accessToken;
    private readonly object lockObj = new();

    public bool TryAuthenticate(string url, Hashtable headers)
    {
        bool canHandle = providerConfig.UrlsToBeAuthenticated.Any(url.StartsWith);
        if (!canHandle)
        {
            return false;
        }

        lock (lockObj)
        {
            if (accessToken == null || accessToken.IsExpired)
            {
                Task<AccessToken> tokenTask = GetAccessToken();
                tokenTask.Wait();
                accessToken = tokenTask.Result;
            }
        }

        headers.Add("Authorization", $"Bearer {accessToken.Value}");
        return true;
    }

    private async Task<AccessToken> GetAccessToken()
    {
        using var client = new HttpClient();
        var discoveryUrl =
            providerConfig.AuthServerUrl.TrimEnd('/') + "/.well-known/openid-configuration";

        using var discoveryResponse = await client.GetAsync(discoveryUrl);
        var discoveryContent = await discoveryResponse.Content.ReadAsStringAsync();

        if (!discoveryResponse.IsSuccessStatusCode)
        {
            throw new Exception(
                string.Format(
                    Resources.ErrorDiscoveryDocumentRetrieval,
                    discoveryUrl,
                    (int)discoveryResponse.StatusCode,
                    discoveryResponse.ReasonPhrase,
                    discoveryContent
                )
            );
        }

        string tokenEndpoint;
        try
        {
            using var discoveryJson = JsonDocument.Parse(discoveryContent);
            var root = discoveryJson.RootElement;

            if (!root.TryGetProperty("token_endpoint", out var tokenEndpointElement))
            {
                throw new Exception(
                    string.Format(
                        Resources.ErrorTokenEndpointNotFoundInDiscovery,
                        discoveryUrl
                    )
                );
            }

            tokenEndpoint = tokenEndpointElement.GetString();
        }
        catch (JsonException ex)
        {
            throw new Exception(
                string.Format(
                    Resources.ErrorDiscoveryDocumentParseFailed,
                    discoveryUrl
                ),
                ex
            );
        }

        if (string.IsNullOrWhiteSpace(tokenEndpoint))
        {
            throw new Exception(
                string.Format(Resources.ErrorTokenEndpointNullOrEmpty, discoveryUrl)
            );
        }

        var body = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = providerConfig.ClientId,
            ["client_secret"] = providerConfig.ClientSecret,
            ["username"] = providerConfig.UserName,
            ["password"] = providerConfig.Password,
            ["scope"] = "internal_api",
        };

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(body),
        };

        using var tokenResponse = await client.SendAsync(tokenRequest);
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new Exception(
                string.Format(
                    Resources.ErrorPasswordTokenRequest,
                    tokenEndpoint,
                    (int)tokenResponse.StatusCode,
                    tokenResponse.ReasonPhrase,
                    tokenContent
                )
            );
        }

        try
        {
            using var tokenJson = JsonDocument.Parse(tokenContent);
            var root = tokenJson.RootElement;

            if (!root.TryGetProperty("access_token", out var accessTokenElement))
            {
                throw new Exception(
                    string.Format(
                        Resources.ErrorAccessTokenNotFoundInResponse,
                        providerConfig.AuthServerUrl
                    )
                );
            }

            var accessTokenValue = accessTokenElement.GetString();
            if (string.IsNullOrWhiteSpace(accessTokenValue))
            {
                throw new Exception(Resources.ErrorAccessTokenNullOrEmpty);
            }

            int expiresInSeconds = 3600;
            if (root.TryGetProperty("expires_in", out var expiresInElement))
            {
                if (expiresInElement.ValueKind == JsonValueKind.Number)
                {
                    if (!expiresInElement.TryGetInt32(out expiresInSeconds))
                    {
                        throw new Exception(
                            string.Format(
                                Resources.ErrorExpiresInParseNumber,
                                expiresInElement
                            )
                        );
                    }
                }
                else if (expiresInElement.ValueKind == JsonValueKind.String)
                {
                    var str = expiresInElement.GetString();
                    if (!int.TryParse(str, out expiresInSeconds))
                    {
                        throw new Exception(
                            string.Format(Resources.ErrorExpiresInParseString, str)
                        );
                    }
                }
            }

            return new AccessToken(
                value: accessTokenValue,
                lifeTime: TimeSpan.FromSeconds(expiresInSeconds)
            );
        }
        catch (JsonException ex)
        {
            throw new Exception(
                string.Format(
                    Resources.ErrorTokenResponseParseFailed,
                    tokenEndpoint,
                    tokenContent
                ),
                ex
            );
        }
    }

    public void Configure(IConfiguration configuration)
    {
        providerConfig = new ResourceOwnerPasswordAuthenticationProviderConfig(configuration);
    }
}

class AccessToken
{
    public string Value { get; }
    public DateTime Expiration { get; }
    private readonly TimeSpan lifeTime;

    public AccessToken(string value, TimeSpan lifeTime)
    {
        this.lifeTime = lifeTime;
        Value = value;
        Expiration = DateTime.Now.Add(lifeTime);
    }

    public bool IsExpired =>
        lifeTime < TimeSpan.FromMinutes(10)
            ? DateTime.Now - Expiration < lifeTime.Divide(4)
            : DateTime.Now - Expiration < TimeSpan.FromMinutes(10);
}
