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
        bool canHandle = providerConfig.UrlsToBeAuthenticated.Any(predicate: url.StartsWith);
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

        headers.Add(key: "Authorization", value: $"Bearer {accessToken.Value}");
        return true;
    }

    private async Task<AccessToken> GetAccessToken()
    {
        using var client = new HttpClient();
        var discoveryUrl =
            providerConfig.AuthServerUrl.TrimEnd(trimChar: '/')
            + "/.well-known/openid-configuration";

        using var discoveryResponse = await client.GetAsync(requestUri: discoveryUrl);
        var discoveryContent = await discoveryResponse.Content.ReadAsStringAsync();

        if (!discoveryResponse.IsSuccessStatusCode)
        {
            throw new Exception(
                message: string.Format(
                    format: Resources.ErrorDiscoveryDocumentRetrieval,
                    args:
                    [
                        discoveryUrl,
                        (int)discoveryResponse.StatusCode,
                        discoveryResponse.ReasonPhrase,
                        discoveryContent,
                    ]
                )
            );
        }

        string tokenEndpoint;
        try
        {
            using var discoveryJson = JsonDocument.Parse(json: discoveryContent);
            var root = discoveryJson.RootElement;

            if (
                !root.TryGetProperty(
                    propertyName: "token_endpoint",
                    value: out var tokenEndpointElement
                )
            )
            {
                throw new Exception(
                    message: string.Format(
                        format: Resources.ErrorTokenEndpointNotFoundInDiscovery,
                        arg0: discoveryUrl
                    )
                );
            }

            tokenEndpoint = tokenEndpointElement.GetString();
        }
        catch (JsonException ex)
        {
            throw new Exception(
                message: string.Format(
                    format: Resources.ErrorDiscoveryDocumentParseFailed,
                    arg0: discoveryUrl
                ),
                innerException: ex
            );
        }

        if (string.IsNullOrWhiteSpace(value: tokenEndpoint))
        {
            throw new Exception(
                message: string.Format(
                    format: Resources.ErrorTokenEndpointNullOrEmpty,
                    arg0: discoveryUrl
                )
            );
        }

        var body = new Dictionary<string, string>
        {
            [key: "grant_type"] = "password",
            [key: "client_id"] = providerConfig.ClientId,
            [key: "client_secret"] = providerConfig.ClientSecret,
            [key: "username"] = providerConfig.UserName,
            [key: "password"] = providerConfig.Password,
            [key: "scope"] = "internal_api",
        };

        using var tokenRequest = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: tokenEndpoint
        )
        {
            Content = new FormUrlEncodedContent(nameValueCollection: body),
        };

        using var tokenResponse = await client.SendAsync(request: tokenRequest);
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new Exception(
                message: string.Format(
                    format: Resources.ErrorPasswordTokenRequest,
                    args:
                    [
                        tokenEndpoint,
                        (int)tokenResponse.StatusCode,
                        tokenResponse.ReasonPhrase,
                        tokenContent,
                    ]
                )
            );
        }

        try
        {
            using var tokenJson = JsonDocument.Parse(json: tokenContent);
            var root = tokenJson.RootElement;

            if (
                !root.TryGetProperty(
                    propertyName: "access_token",
                    value: out var accessTokenElement
                )
            )
            {
                throw new Exception(
                    message: string.Format(
                        format: Resources.ErrorAccessTokenNotFoundInResponse,
                        arg0: providerConfig.AuthServerUrl
                    )
                );
            }

            var accessTokenValue = accessTokenElement.GetString();
            if (string.IsNullOrWhiteSpace(value: accessTokenValue))
            {
                throw new Exception(message: Resources.ErrorAccessTokenNullOrEmpty);
            }

            int expiresInSeconds = 3600;
            if (root.TryGetProperty(propertyName: "expires_in", value: out var expiresInElement))
            {
                if (expiresInElement.ValueKind == JsonValueKind.Number)
                {
                    if (!expiresInElement.TryGetInt32(value: out expiresInSeconds))
                    {
                        throw new Exception(
                            message: string.Format(
                                format: Resources.ErrorExpiresInParseNumber,
                                arg0: expiresInElement
                            )
                        );
                    }
                }
                else if (expiresInElement.ValueKind == JsonValueKind.String)
                {
                    var str = expiresInElement.GetString();
                    if (!int.TryParse(s: str, result: out expiresInSeconds))
                    {
                        throw new Exception(
                            message: string.Format(
                                format: Resources.ErrorExpiresInParseString,
                                arg0: str
                            )
                        );
                    }
                }
            }

            return new AccessToken(
                value: accessTokenValue,
                lifeTime: TimeSpan.FromSeconds(value: expiresInSeconds)
            );
        }
        catch (JsonException ex)
        {
            throw new Exception(
                message: string.Format(
                    format: Resources.ErrorTokenResponseParseFailed,
                    arg0: tokenEndpoint,
                    arg1: tokenContent
                ),
                innerException: ex
            );
        }
    }

    public void Configure(IConfiguration configuration)
    {
        providerConfig = new ResourceOwnerPasswordAuthenticationProviderConfig(
            configuration: configuration
        );
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
        Expiration = DateTime.Now.Add(value: lifeTime);
    }

    public bool IsExpired =>
        lifeTime < TimeSpan.FromMinutes(value: 10)
            ? DateTime.Now - Expiration < lifeTime.Divide(divisor: 4)
            : DateTime.Now - Expiration < TimeSpan.FromMinutes(value: 10);
}
