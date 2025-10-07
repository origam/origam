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

using System;

namespace Origam.DA.Service.Generators;

public class TimeGroupingRenderer
{
    private readonly Func<ColumnRenderData, string> columnDataToSql;
    private readonly ColumnRenderData columnRenderData;
    private readonly string groupingUnit;
    private readonly DatePartRenderer datePartRenderer;

    public TimeGroupingRenderer(
        ColumnRenderData columnRenderData,
        Func<ColumnRenderData, string> columnDataToSql,
        string groupingUnit,
        SqlRenderer sqlRenderer
    )
    {
        this.columnRenderData = columnRenderData;
        this.columnDataToSql = columnDataToSql;
        this.groupingUnit = groupingUnit;
        datePartRenderer = new DatePartRenderer(sqlRenderer);
    }

    public string RenderWithAliases()
    {
        return string.Join(
            ", ",
            RenderExpression(
                new SelectExpressionRenderer(columnDataToSql, datePartRenderer),
                columnRenderData,
                groupingUnit
            )
        );
    }

    public string[] RenderWithoutAliases()
    {
        return RenderExpression(datePartRenderer, columnRenderData, groupingUnit);
    }

    public static string[] GetColumnNames(string columnName, string groupingUnit)
    {
        return RenderExpression(
            new ColumnNameRenderer(),
            new ColumnRenderData { Alias = columnName },
            groupingUnit
        );
    }

    private static string[] RenderExpression(
        IColumnRenderer columnRenderer,
        ColumnRenderData columnRenderData,
        string groupingUnit
    )
    {
        string[] expressions;
        switch (groupingUnit)
        {
            case "year":
                expressions = new[] { columnRenderer.Render(columnRenderData, "year") };
                break;
            case "month":
                expressions = new[]
                {
                    columnRenderer.Render(columnRenderData, "year"),
                    columnRenderer.Render(columnRenderData, "month"),
                };
                break;
            case "day":
                expressions = new[]
                {
                    columnRenderer.Render(columnRenderData, "year"),
                    columnRenderer.Render(columnRenderData, "month"),
                    columnRenderer.Render(columnRenderData, "day"),
                };
                break;
            case "hour":
                expressions = new[]
                {
                    columnRenderer.Render(columnRenderData, "year"),
                    columnRenderer.Render(columnRenderData, "month"),
                    columnRenderer.Render(columnRenderData, "day"),
                    columnRenderer.Render(columnRenderData, "hour"),
                };
                break;
            case "minute":
                expressions = new[]
                {
                    columnRenderer.Render(columnRenderData, "year"),
                    columnRenderer.Render(columnRenderData, "month"),
                    columnRenderer.Render(columnRenderData, "day"),
                    columnRenderer.Render(columnRenderData, "hour"),
                    columnRenderer.Render(columnRenderData, "minute"),
                };
                break;
            default:
                throw new NotImplementedException("Cannot render columns for " + groupingUnit);
        }
        return expressions;
    }
}

interface IColumnRenderer
{
    string Render(ColumnRenderData columnRenderData, string groupingUnit);
}

class SelectExpressionRenderer : IColumnRenderer
{
    private readonly Func<ColumnRenderData, string> columnDataToSql;
    private readonly DatePartRenderer datePartRenderer;

    public SelectExpressionRenderer(
        Func<ColumnRenderData, string> columnDataToSql,
        DatePartRenderer datePartRenderer
    )
    {
        this.columnDataToSql = columnDataToSql;
        this.datePartRenderer = datePartRenderer;
    }

    public string Render(ColumnRenderData columnRenderData, string groupingUnit)
    {
        return columnDataToSql(
            new ColumnRenderData
            {
                Expression = datePartRenderer.Render(columnRenderData, groupingUnit),
                Alias = $"{columnRenderData.Alias}_{groupingUnit}",
            }
        );
    }
}

class DatePartRenderer : IColumnRenderer
{
    private readonly SqlRenderer sqlRenderer;

    public DatePartRenderer(SqlRenderer sqlRenderer)
    {
        this.sqlRenderer = sqlRenderer;
    }

    public string Render(ColumnRenderData columnRenderData, string groupingUnit)
    {
        return sqlRenderer.DatePart(groupingUnit.ToUpper(), columnRenderData.Expression);
    }
}

class ColumnNameRenderer : IColumnRenderer
{
    public string Render(ColumnRenderData columnRenderData, string groupingUnit)
    {
        return $"{columnRenderData.Alias}_{groupingUnit}";
    }
}
