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

namespace Origam.AI.Function.Calling;

public delegate ValueTask<object?> ToolInvocation(
    FunctionInvocationContext context,
    CancellationToken cancellationToken
);

public interface IToolInvocationFilter
{
    ValueTask<object?> OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        ToolInvocation next,
        CancellationToken cancellationToken
    );
}

public static class ToolInvocationPipeline
{
    public static ToolInvocation Build(IReadOnlyList<IToolInvocationFilter> filters)
    {
        ToolInvocation next = (context, cancellationToken) =>
            context.Function.InvokeAsync(context.Arguments, cancellationToken);

        for (var index = filters.Count - 1; index >= 0; index--)
        {
            var filter = filters[index];
            var following = next;
            next = (context, cancellationToken) =>
                filter.OnFunctionInvocationAsync(context, following, cancellationToken);
        }

        return next;
    }
}
