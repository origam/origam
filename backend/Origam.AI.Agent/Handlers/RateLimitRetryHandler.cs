#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.Net;
using System.Text.RegularExpressions;

namespace Origam.AI.Agent.Handlers;

public class RateLimitRetryHandler(ILogger<RateLimitRetryHandler> logger) : DelegatingHandler
{
    private const int MaxAttempts = 9;
    private const string RateLimitCode = "rate_limit_exceeded";
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan FallbackDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan SettlingMargin = TimeSpan.FromSeconds(1);

    private static readonly Regex SuggestedDelayPattern = new(
        pattern: @"try again in ([0-9.]+)(ms|s)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (request.Content is not null)
        {
            await request.Content.LoadIntoBufferAsync();
        }

        for (var attempt = 1; ; attempt++)
        {
            var response = await base.SendAsync(request, cancellationToken);

            TimeSpan? delay = await GetRateLimitDelayAsync(response, attempt, cancellationToken);
            if (delay is null || attempt >= MaxAttempts)
            {
                return response;
            }

            response.Dispose();

            logger.LogWarning(
                message: "OpenAI rate limit hit, waiting {DelaySeconds:0.##}s before retry {Attempt} of {MaxAttempts}.",
                delay.Value.TotalSeconds,
                attempt,
                MaxAttempts - 1
            );

            await Task.Delay(delay.Value, cancellationToken);
        }
    }

    private static async Task<TimeSpan?> GetRateLimitDelayAsync(
        HttpResponseMessage response,
        int attempt,
        CancellationToken cancellationToken
    )
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return GetDelay(response, attempt);
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string body = await ReadBodyAsync(response, cancellationToken);
        return body.Contains(value: RateLimitCode, comparisonType: StringComparison.Ordinal)
            ? ReadSuggestedDelay(body, attempt)
            : null;
    }

    private static async Task<string> ReadBodyAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await response.Content.LoadIntoBufferAsync();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            return string.Empty;
        }
        catch (IOException)
        {
            return string.Empty;
        }
    }

    private static TimeSpan ReadSuggestedDelay(string body, int attempt)
    {
        Match match = SuggestedDelayPattern.Match(body);
        if (!match.Success || !double.TryParse(match.Groups[groupnum: 1].Value, out double amount))
        {
            return Clamp(TimeSpan.FromSeconds(Math.Pow(x: 2, y: attempt)));
        }

        TimeSpan suggested =
            match.Groups[groupnum: 2].Value == "ms"
                ? TimeSpan.FromMilliseconds(amount)
                : TimeSpan.FromSeconds(amount);
        return Clamp(suggested + SettlingMargin);
    }

    private static TimeSpan GetDelay(HttpResponseMessage response, int attempt)
    {
        if (
            response.Headers.TryGetValues(name: "retry-after-ms", out var millisecondValues)
            && double.TryParse(millisecondValues.FirstOrDefault(), out var milliseconds)
        )
        {
            return Clamp(TimeSpan.FromMilliseconds(milliseconds));
        }

        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta)
        {
            return Clamp(delta);
        }

        if (retryAfter?.Date is { } date)
        {
            return Clamp(date - DateTimeOffset.UtcNow);
        }

        return Clamp(TimeSpan.FromSeconds(Math.Pow(x: 2, y: attempt)));
    }

    private static TimeSpan Clamp(TimeSpan delay)
    {
        if (delay <= TimeSpan.Zero)
        {
            return FallbackDelay;
        }

        return delay > MaxDelay ? MaxDelay : delay;
    }
}
