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

using System.Runtime.CompilerServices;
using Microsoft.Agents.AI;

namespace Origam.AI.Agent.Chat;

public sealed class IdleTimeoutStream(TimeSpan idleTimeout, string stalledMessage)
{
    public Exception? Failure { get; private set; }

    public async IAsyncEnumerable<AgentResponseUpdate> ReadAsync(
        Func<CancellationToken, IAsyncEnumerable<AgentResponseUpdate>> source,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        using var streamCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken
        );
        var updates = source(streamCancellation.Token).GetAsyncEnumerator(streamCancellation.Token);

        try
        {
            while (true)
            {
                AgentResponseUpdate update;
                try
                {
                    var moveNext = updates.MoveNextAsync().AsTask();
                    if (
                        await WaitForUpdateAsync(moveNext, streamCancellation).ConfigureAwait(false)
                    )
                    {
                        Failure = new TimeoutException(stalledMessage);
                        break;
                    }

                    if (!await moveNext.ConfigureAwait(false))
                    {
                        break;
                    }
                    update = updates.Current;
                }
                catch (Exception exception)
                {
                    Failure = exception;
                    break;
                }

                yield return update;
            }
        }
        finally
        {
            try
            {
                await updates.DisposeAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
        }
    }

    private async Task<bool> WaitForUpdateAsync(
        Task<bool> moveNext,
        CancellationTokenSource streamCancellation
    )
    {
        using var idleTimeoutCancellation = new CancellationTokenSource();
        var completed = await Task.WhenAny(
                moveNext,
                Task.Delay(idleTimeout, idleTimeoutCancellation.Token)
            )
            .ConfigureAwait(false);

        if (completed == moveNext)
        {
            await idleTimeoutCancellation.CancelAsync().ConfigureAwait(false);
            return false;
        }

        await streamCancellation.CancelAsync().ConfigureAwait(false);
        try
        {
            await moveNext.ConfigureAwait(false);
        }
        catch (Exception)
        {
            // ignored
        }

        return true;
    }
}
