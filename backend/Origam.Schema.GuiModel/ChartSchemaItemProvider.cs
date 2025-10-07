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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Origam.Schema.GuiModel;

public class ChartSchemaItemProvider : AbstractSchemaItemProvider
{
    public ChartSchemaItemProvider() { }

    #region ISchemaItemProvider Members
    public override string RootItemType => AbstractChart.CategoryConst;
    public override string Group => "UI";
    #endregion
    public List<AbstractChart> Charts(Guid formId, string entity)
    {
        var result = new List<AbstractChart>();
        foreach (var abstractSchemaItem in ChildItems)
        {
            var chart = (AbstractChart)abstractSchemaItem;
            if (
                chart
                    .ChildItemsByType<ChartFormMapping>(ChartFormMapping.CategoryConst)
                    .Any(chartFormMapping =>
                        formId.Equals(chartFormMapping.Screen.Id)
                        && entity.Equals(chartFormMapping.Entity.Name)
                    )
            )
            {
                result.Add(chart);
            }
        }
        return result;
    }

    #region IBrowserNode Members
    public override string Icon =>
        // TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
        "icon_14_charts.png";
    public override string NodeText
    {
        get => "Charts";
        set => base.NodeText = value;
    }
    public override string NodeToolTipText =>
        // TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
        "List of Charts";
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[] { typeof(CartesianChart), typeof(PieChart), typeof(SvgChart) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(CartesianChart))
        {
            itemName = "NewCartesianChart";
        }
        else if (typeof(T) == typeof(PieChart))
        {
            itemName = "NewPieChart";
        }
        else if (typeof(T) == typeof(SvgChart))
        {
            itemName = "NewSvgChart";
        }
        return base.NewItem<T>(schemaExtensionId, group, itemName);
    }
    #endregion
}
