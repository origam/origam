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

using Microsoft.Extensions.AI;
using Origam.AI.Agent.Services.OpenApi;

namespace Origam.AI.Agent;

public class ToolErrorFilter : IToolInvocationFilter
{
    private const int MaxLength = 4000;
    private const int MaxModelLength = 500;

    private static readonly string[] LineBreakMarkers = { "\\r\\n", "\\n", "\r\n", "\n" };

    private readonly ILogger<ToolErrorFilter> logger;

    public ToolErrorFilter(ILogger<ToolErrorFilter> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<object?> OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        ToolInvocation next,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await next(context, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OpenApiInvocationException exception)
        {
            logger.LogWarning(
                message: "{Function} -> {StatusCode}\n      url: {Url}\n      payload: {Payload}\n      arguments: {Arguments}\n      response: {Response}",
                exception.FunctionName,
                exception.StatusCode,
                exception.RequestUrl,
                Shorten(exception.RequestPayload),
                Shorten(DescribeArguments(context.Arguments)),
                Shorten(exception.ResponseBody)
            );

            return $"The tool call failed with HTTP status {exception.StatusCode}. The server responded: "
                + FirstLine(exception.ResponseBody);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                message: "{Function} threw {Type}\n      arguments: {Arguments}\n      message: {Message}",
                context.Function.Name,
                exception.GetType().Name,
                Shorten(DescribeArguments(context.Arguments)),
                Shorten(exception.Message)
            );

            return $"The tool call failed: {FirstLine(exception.Message)}";
        }
    }

    private static string DescribeArguments(AIFunctionArguments arguments)
    {
        return string.Join(
            separator: ", ",
            arguments.Select(argument => $"{argument.Key}={argument.Value}")
        );
    }

    private static string FirstLine(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "(no response body)";
        }

        var text = value.Trim().Trim('"');
        var cut = text.Length;
        foreach (var marker in LineBreakMarkers)
        {
            var index = text.IndexOf(marker, StringComparison.Ordinal);
            if (index >= 0 && index < cut)
            {
                cut = index;
            }
        }

        text = text.Substring(startIndex: 0, length: cut);
        return text.Length <= MaxModelLength
            ? text
            : text.Substring(startIndex: 0, length: MaxModelLength) + " ... (truncated)";
    }

    private static string Shorten(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "(empty)";
        }

        return value.Length <= MaxLength
            ? value
            : value.Substring(startIndex: 0, length: MaxLength) + " ... (truncated)";
    }
}
