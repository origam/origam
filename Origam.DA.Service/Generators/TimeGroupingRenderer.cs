using System;

namespace Origam.DA.Service.Generators
{
    public class TimeGroupingRenderer
    {

        private readonly Func<ColumnRenderData, string> columnDataToSql;
        private readonly ColumnRenderData columnRenderData;
        private readonly string groupingUnit;

        public TimeGroupingRenderer(ColumnRenderData columnRenderData,
            Func<ColumnRenderData, string> columnDataToSql, string groupingUnit)
        {
            this.columnRenderData = columnRenderData;
            this.columnDataToSql = columnDataToSql;
            this.groupingUnit = groupingUnit;
        }

        public string RenderWithAliases()
        {
            return string.Join(", ", RenderExpression(
                new SelectExpressionRenderer(columnDataToSql), columnRenderData, groupingUnit));
        }

        public string[] RenderWithoutAliases()
        {
            return RenderExpression(
                new DatePartRenderer(), columnRenderData, groupingUnit);
        }

        public static string[] GetColumnNames(string columnName, string groupingUnit)
        {
            return RenderExpression(
                new ColumnNameRenderer(),
                new ColumnRenderData{Alias = columnName},
                groupingUnit);
        }

        private static string[] RenderExpression(IColumnRenderer columnRenderer, ColumnRenderData columnRenderData, string groupingUnit)
        {
            string[] expressions;
            switch (groupingUnit)
            {
                case "year":
                    expressions = new[]
                    {
                        columnRenderer.Render(columnRenderData, "year")
                    };
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
                    throw new NotImplementedException("Cannot render columns for "+groupingUnit);
            }

            return expressions;
        }
    }

    interface IColumnRenderer
    {
        string Render(ColumnRenderData columnRenderData, string groupingUnit);
    }
    
    class SelectExpressionRenderer: IColumnRenderer
    {
        private readonly Func<ColumnRenderData, string> columnDataToSql;
        private readonly DatePartRenderer datePartRenderer = new DatePartRenderer();

        public SelectExpressionRenderer(Func<ColumnRenderData, string> columnDataToSql)
        {
            this.columnDataToSql = columnDataToSql;
        }

        public string Render(ColumnRenderData columnRenderData, string groupingUnit)
        {
            return columnDataToSql(
                new ColumnRenderData{
                    Expression = datePartRenderer.Render(columnRenderData, groupingUnit), 
                    Alias = $"{columnRenderData.Alias}_{groupingUnit}"
                }
            );
        }
    }    
    class DatePartRenderer: IColumnRenderer
    {
        public string Render(ColumnRenderData columnRenderData, string groupingUnit)
        {
            return $"DATEPART({groupingUnit.ToUpper()}, {columnRenderData.Expression})";
        }
    }  
    
    class ColumnNameRenderer: IColumnRenderer
    {
        public string Render(ColumnRenderData columnRenderData, string groupingUnit)
        {
            return $"{columnRenderData.Alias}_{groupingUnit}";
        }
    }
}