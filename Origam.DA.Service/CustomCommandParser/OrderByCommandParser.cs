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
using System.Collections.Generic;
using System.Linq;

namespace Origam.DA.Service.CustomCommandParser
{
    public class OrderByCommandParser : ICustomCommandParser
    {
        private readonly ColumnOrderingRenderer columnOrderingRenderer;
        private readonly List<Ordering> orderingsInput;

        public OrderByCommandParser(List<Ordering> orderingsInput)
        {
            this.orderingsInput = orderingsInput ?? new List<Ordering>();
            columnOrderingRenderer 
                = new ColumnOrderingRenderer();
        }

        public string[] Columns => orderingsInput 
            .Select(ordering => ordering.ColumnName)
            .ToArray();
        
        public void SetColumnExpressionsIfMissing(string columnName, string[] expressions)
        {
            columnOrderingRenderer.SetColumnExpressionIfMissing(columnName, expressions);
        }

        public string Sql => columnOrderingRenderer.ToSqlOrderBy(orderingsInput);
    }
    
    class ColumnOrderingRenderer
    {
        private readonly Dictionary<string, string[]> columnExpressions = new Dictionary<string, string[]>();
        
        public void SetColumnExpressionIfMissing(string columnName, string[] expressions)
        {
            if (!columnExpressions.ContainsKey(columnName))
            {
                columnExpressions[columnName] = expressions;
            }
        }

        internal string ToSqlOrderBy(List<Ordering> orderings)
        {
            if (orderings == null) return "";
            return string.Join(", ", orderings.Select(ToSql)
            );
        }

        private string ToSql(Ordering ordering)
        {
            string directionSql = DirectionToSQLName(ordering.Direction);
            if (!columnExpressions.ContainsKey(ordering.ColumnName))
            {
                throw new Exception($"No expression was set for {ordering.ColumnName}");
            }

            var orderByExpressions = columnExpressions[ordering.ColumnName]
                .Select(expression => $"{expression} {directionSql}");
            return string.Join(", ", orderByExpressions);
        }

        private string DirectionToSQLName(string orderingName)
        {
            switch (orderingName.ToLower())
            {
                case "asc": return "ASC";
                case "desc": return "DESC";
                default: throw new NotImplementedException(orderingName);
            }
        }
    }
}