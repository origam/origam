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
using System.Data;
using System.Xml;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for ChartBuilder.
/// </summary>
public class ChartBuilder
{
    public static XmlElement Build(XmlDocument doc, AbstractChart chart, DataTable table)
    {
        XmlElement chartElement = doc.CreateElement(name: "Chart");
        chartElement.SetAttribute(name: "Type", value: chart.GetType().Name);
        chartElement.SetAttribute(name: "Name", value: chart.Caption);
        CartesianChart cartesianChart = chart as CartesianChart;
        PieChart pieChart = chart as PieChart;
        SvgChart svgChart = chart as SvgChart;
        if (cartesianChart != null)
        {
            BuildCartesianChart(
                doc: doc,
                chartElement: chartElement,
                chart: cartesianChart,
                table: table
            );
        }
        else if (pieChart != null)
        {
            BuildPieChart(doc: doc, chartElement: chartElement, chart: pieChart, table: table);
        }
        else if (svgChart != null)
        {
            BuildSvgChart(doc: doc, chartElement: chartElement, chart: svgChart, table: table);
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                paramName: "chart",
                actualValue: chart,
                message: "Unknown chart type."
            );
        }
        return chartElement;
    }

    private static void BuildCartesianChart(
        XmlDocument doc,
        XmlElement chartElement,
        CartesianChart chart,
        DataTable table
    )
    {
        XmlElement axesElement = doc.CreateElement(name: "Axes");
        chartElement.AppendChild(newChild: axesElement);
        XmlElement seriesElement = doc.CreateElement(name: "Series");
        chartElement.AppendChild(newChild: seriesElement);
        foreach (ISchemaItem item in chart.ChildItems)
        {
            if (item.GetType() == typeof(CartesianChartHorizontalAxis))
            {
                BuildCartesianChartHorizontalAxis(
                    axesElement: axesElement,
                    item: item as CartesianChartHorizontalAxis,
                    table: table
                );
            }
            else if (item.GetType() == typeof(CartesianChartVerticalAxis))
            {
                BuildCartesianChartVerticalAxis(
                    axesElement: axesElement,
                    item: item as CartesianChartVerticalAxis
                );
            }
            else if (item.GetType() == typeof(ColumnSeries))
            {
                BuildColumnSeries(
                    seriesElement: seriesElement,
                    item: item as ColumnSeries,
                    table: table
                );
            }
            else if (item.GetType() == typeof(LineSeries))
            {
                BuildLineSeries(
                    seriesElement: seriesElement,
                    item: item as LineSeries,
                    table: table
                );
            }
        }
    }

    private static void BuildPieChart(
        XmlDocument doc,
        XmlElement chartElement,
        PieChart chart,
        DataTable table
    )
    {
        XmlElement seriesElement = doc.CreateElement(name: "Series");
        chartElement.AppendChild(newChild: seriesElement);
        foreach (ISchemaItem item in chart.ChildItems)
        {
            if (item.GetType() == typeof(PieSeries))
            {
                BuildPieSeries(seriesElement: seriesElement, item: item as PieSeries, table: table);
            }
        }
    }

    private static void BuildSvgChart(
        XmlDocument doc,
        XmlElement chartElement,
        SvgChart chart,
        DataTable table
    )
    {
        chartElement.SetAttribute(name: "SvgFileName", value: chart.SvgFileName);
        chartElement.SetAttribute(name: "SvgChartType", value: chart.Type.ToString());
        chartElement.SetAttribute(name: "TitleField", value: chart.TitleField);
        chartElement.SetAttribute(name: "SvgObjectField", value: chart.SvgObjectField);
        chartElement.SetAttribute(name: "ValueField", value: chart.ValueField);
        string titleLookupId = GetLookupId(table: table, fieldName: chart.TitleField, item: chart);
        if (titleLookupId != null)
        {
            chartElement.SetAttribute(name: "TitleFieldLookupId", value: titleLookupId);
        }

        string svgObjectLookupId = GetLookupId(
            table: table,
            fieldName: chart.SvgObjectField,
            item: chart
        );
        if (svgObjectLookupId != null)
        {
            chartElement.SetAttribute(name: "SvgObjectFieldLookupId", value: svgObjectLookupId);
        }
    }

    private static void BuildCartesianChartHorizontalAxis(
        XmlElement axesElement,
        CartesianChartHorizontalAxis item,
        DataTable table
    )
    {
        XmlElement axis = axesElement.OwnerDocument.CreateElement(name: "HorizontalAxis");
        axesElement.AppendChild(newChild: axis);
        axis.SetAttribute(name: "Id", value: item.Name);
        axis.SetAttribute(name: "Caption", value: item.Caption);

        axis.SetAttribute(name: "Field", value: item.Field);
        string fieldLookupId = GetLookupId(table: table, fieldName: item.Field, item: item);
        if (fieldLookupId != null)
        {
            axis.SetAttribute(name: "FieldLookupId", value: fieldLookupId);
        }

        axis.SetAttribute(name: "AggregationType", value: item.AggregationType.ToString());
    }

    private static string GetLookupId(DataTable table, string fieldName, ISchemaItem item)
    {
        if (fieldName != "" && fieldName != null)
        {
            if (!table.Columns.Contains(name: fieldName))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "fieldName",
                    actualValue: fieldName,
                    message: "Field not found in the data source for " + item.Path
                );
            }
            DataColumn col = table.Columns[name: fieldName];
            if (col.ExtendedProperties.Contains(key: Origam.DA.Const.DefaultLookupIdAttribute))
            {
                return col.ExtendedProperties[key: Origam.DA.Const.DefaultLookupIdAttribute]
                    .ToString();
            }
        }
        return null;
    }

    private static void BuildCartesianChartVerticalAxis(
        XmlElement axesElement,
        CartesianChartVerticalAxis item
    )
    {
        XmlElement axis = axesElement.OwnerDocument.CreateElement(name: "VerticalAxis");
        axesElement.AppendChild(newChild: axis);
        axis.SetAttribute(name: "Id", value: item.Name);
        axis.SetAttribute(name: "Caption", value: item.Caption);
        if (item.ApplyMinLimit)
        {
            axis.SetAttribute(name: "Min", value: item.Min.ToString());
        }

        if (item.ApplyMaxLimit)
        {
            axis.SetAttribute(name: "Max", value: item.Max.ToString());
        }
    }

    private static XmlElement BuildBasicSeries(
        XmlElement seriesElement,
        AbstractSeries item,
        DataTable table
    )
    {
        XmlElement series = seriesElement.OwnerDocument.CreateElement(name: "Series");
        seriesElement.AppendChild(newChild: series);
        series.SetAttribute(name: "Id", value: item.Name);
        series.SetAttribute(name: "Caption", value: item.Caption);
        series.SetAttribute(name: "Field", value: item.Field);
        string fieldLookupId = GetLookupId(table: table, fieldName: item.Field, item: item);
        if (fieldLookupId != null)
        {
            series.SetAttribute(name: "FieldLookupId", value: fieldLookupId);
        }

        series.SetAttribute(name: "Aggregation", value: item.Aggregation.ToString());
        if (item.ColorsLookup != null)
        {
            series.SetAttribute(name: "ColorsLookupId", value: item.ColorsLookupId.ToString());
        }

        return series;
    }

    private static void BuildColumnSeries(
        XmlElement seriesElement,
        ColumnSeries item,
        DataTable table
    )
    {
        XmlElement series = BuildBasicSeries(
            seriesElement: seriesElement,
            item: item,
            table: table
        );
        series.SetAttribute(name: "ColumnSeriesType", value: item.Type.ToString());
        series.SetAttribute(name: "ZAxisField", value: item.ZAxisField);
        string zAxisFieldLookupId = GetLookupId(
            table: table,
            fieldName: item.ZAxisField,
            item: item
        );
        if (zAxisFieldLookupId != null)
        {
            series.SetAttribute(name: "ZAxisFieldLookupId", value: zAxisFieldLookupId);
        }
    }

    private static void BuildLineSeries(XmlElement seriesElement, LineSeries item, DataTable table)
    {
        XmlElement series = BuildBasicSeries(
            seriesElement: seriesElement,
            item: item,
            table: table
        );
        series.SetAttribute(name: "Type", value: item.GetType().Name);
        series.SetAttribute(name: "LineSeriesForm", value: item.Form.ToString());
        series.SetAttribute(name: "ZAxisField", value: item.ZAxisField);
        string zAxisFieldLookupId = GetLookupId(
            table: table,
            fieldName: item.ZAxisField,
            item: item
        );
        if (zAxisFieldLookupId != null)
        {
            series.SetAttribute(name: "ZAxisFieldLookupId", value: zAxisFieldLookupId);
        }
    }

    private static void BuildPieSeries(XmlElement seriesElement, PieSeries item, DataTable table)
    {
        XmlElement series = BuildBasicSeries(
            seriesElement: seriesElement,
            item: item,
            table: table
        );
        series.SetAttribute(name: "Type", value: item.GetType().Name);
        series.SetAttribute(name: "NameField", value: item.NameField);
        string nameFieldLookupId = GetLookupId(table: table, fieldName: item.NameField, item: item);
        if (nameFieldLookupId != null)
        {
            series.SetAttribute(name: "NameFieldLookupId", value: nameFieldLookupId);
        }
    }
}
