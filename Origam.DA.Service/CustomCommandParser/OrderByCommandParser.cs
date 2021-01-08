#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

        public OrderByCommandParser(List<Ordering> orderingsInput,
            string nameLeftBracket, string nameRightBracket)
        {
            this.orderingsInput = orderingsInput ?? new List<Ordering>();
            columnOrderingRenderer 
                = new ColumnOrderingRenderer(nameLeftBracket, nameRightBracket);
        }

        public string[] Columns => orderingsInput 
            .Select(ordering => ordering.ColumnName)
            .ToArray();
        
        public void SetColumnExpression(string columnName, string expression)
        {
            columnOrderingRenderer.SetColumnExpression(columnName, expression);
        }

        public string Sql =>columnOrderingRenderer.ToSqlOrderBy(orderingsInput);
    }
    
    class ColumnOrderingRenderer
    {
        private readonly string nameLeftBracket;
        private readonly string nameRightBracket;
        private Dictionary<string, string> columnExpressions = new Dictionary<string, string>();

        public ColumnOrderingRenderer(string nameLeftBracket, string nameRightBracket)
        {
            this.nameLeftBracket = nameLeftBracket;
            this.nameRightBracket = nameRightBracket;
        }
        
        public void SetColumnExpression(string columnName, string expression)
        {
            columnExpressions[columnName] = expression;
        }

        internal string ToSqlOrderBy(List<Ordering> orderings)
        {
            if (orderings == null) return "";
            return string.Join(", ", orderings.Select(ToSql)
            );
        }

        private string ToSql(Ordering ordering)
        {
            string orderingSql = OrderingToSQLName(ordering.Direction);
            if (ordering.LookupId == Guid.Empty)
            {
                return $"{nameLeftBracket}{ordering.ColumnName}{nameRightBracket} {orderingSql}";
            }

            if (!columnExpressions.ContainsKey(ordering.ColumnName))
            {
                throw new InvalidOperationException($"Lookup expression for {ordering.ColumnName} was not set");
            }
            return $"{columnExpressions[ordering.ColumnName]} {orderingSql}";
        }

        private string OrderingToSQLName(string orderingName)
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