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
        XmlElement chartElement = doc.CreateElement("Chart");
        chartElement.SetAttribute("Type", chart.GetType().Name);
        chartElement.SetAttribute("Name", chart.Caption);
        CartesianChart cartesianChart = chart as CartesianChart;
        PieChart pieChart = chart as PieChart;
        SvgChart svgChart = chart as SvgChart;
        if (cartesianChart != null)
        {
            BuildCartesianChart(doc, chartElement, cartesianChart, table);
        }
        else if (pieChart != null)
        {
            BuildPieChart(doc, chartElement, pieChart, table);
        }
        else if (svgChart != null)
        {
            BuildSvgChart(doc, chartElement, svgChart, table);
        }
        else
        {
            throw new ArgumentOutOfRangeException("chart", chart, "Unknown chart type.");
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
        XmlElement axesElement = doc.CreateElement("Axes");
        chartElement.AppendChild(axesElement);
        XmlElement seriesElement = doc.CreateElement("Series");
        chartElement.AppendChild(seriesElement);
        foreach (ISchemaItem item in chart.ChildItems)
        {
            if (item.GetType() == typeof(CartesianChartHorizontalAxis))
            {
                BuildCartesianChartHorizontalAxis(
                    axesElement,
                    item as CartesianChartHorizontalAxis,
                    table
                );
            }
            else if (item.GetType() == typeof(CartesianChartVerticalAxis))
            {
                BuildCartesianChartVerticalAxis(axesElement, item as CartesianChartVerticalAxis);
            }
            else if (item.GetType() == typeof(ColumnSeries))
            {
                BuildColumnSeries(seriesElement, item as ColumnSeries, table);
            }
            else if (item.GetType() == typeof(LineSeries))
            {
                BuildLineSeries(seriesElement, item as LineSeries, table);
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
        XmlElement seriesElement = doc.CreateElement("Series");
        chartElement.AppendChild(seriesElement);
        foreach (ISchemaItem item in chart.ChildItems)
        {
            if (item.GetType() == typeof(PieSeries))
            {
                BuildPieSeries(seriesElement, item as PieSeries, table);
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
        chartElement.SetAttribute("SvgFileName", chart.SvgFileName);
        chartElement.SetAttribute("SvgChartType", chart.Type.ToString());
        chartElement.SetAttribute("TitleField", chart.TitleField);
        chartElement.SetAttribute("SvgObjectField", chart.SvgObjectField);
        chartElement.SetAttribute("ValueField", chart.ValueField);
        string titleLookupId = GetLookupId(table, chart.TitleField, chart);
        if (titleLookupId != null)
        {
            chartElement.SetAttribute("TitleFieldLookupId", titleLookupId);
        }

        string svgObjectLookupId = GetLookupId(table, chart.SvgObjectField, chart);
        if (svgObjectLookupId != null)
        {
            chartElement.SetAttribute("SvgObjectFieldLookupId", svgObjectLookupId);
        }
    }

    private static void BuildCartesianChartHorizontalAxis(
        XmlElement axesElement,
        CartesianChartHorizontalAxis item,
        DataTable table
    )
    {
        XmlElement axis = axesElement.OwnerDocument.CreateElement("HorizontalAxis");
        axesElement.AppendChild(axis);
        axis.SetAttribute("Id", item.Name);
        axis.SetAttribute("Caption", item.Caption);

        axis.SetAttribute("Field", item.Field);
        string fieldLookupId = GetLookupId(table, item.Field, item);
        if (fieldLookupId != null)
        {
            axis.SetAttribute("FieldLookupId", fieldLookupId);
        }

        axis.SetAttribute("AggregationType", item.AggregationType.ToString());
    }

    private static string GetLookupId(DataTable table, string fieldName, ISchemaItem item)
    {
        if (fieldName != "" && fieldName != null)
        {
            if (!table.Columns.Contains(fieldName))
            {
                throw new ArgumentOutOfRangeException(
                    "fieldName",
                    fieldName,
                    "Field not found in the data source for " + item.Path
                );
            }
            DataColumn col = table.Columns[fieldName];
            if (col.ExtendedProperties.Contains(Origam.DA.Const.DefaultLookupIdAttribute))
            {
                return col.ExtendedProperties[Origam.DA.Const.DefaultLookupIdAttribute].ToString();
            }
        }
        return null;
    }

    private static void BuildCartesianChartVerticalAxis(
        XmlElement axesElement,
        CartesianChartVerticalAxis item
    )
    {
        XmlElement axis = axesElement.OwnerDocument.CreateElement("VerticalAxis");
        axesElement.AppendChild(axis);
        axis.SetAttribute("Id", item.Name);
        axis.SetAttribute("Caption", item.Caption);
        if (item.ApplyMinLimit)
        {
            axis.SetAttribute("Min", item.Min.ToString());
        }

        if (item.ApplyMaxLimit)
        {
            axis.SetAttribute("Max", item.Max.ToString());
        }
    }

    private static XmlElement BuildBasicSeries(
        XmlElement seriesElement,
        AbstractSeries item,
        DataTable table
    )
    {
        XmlElement series = seriesElement.OwnerDocument.CreateElement("Series");
        seriesElement.AppendChild(series);
        series.SetAttribute("Id", item.Name);
        series.SetAttribute("Caption", item.Caption);
        series.SetAttribute("Field", item.Field);
        string fieldLookupId = GetLookupId(table, item.Field, item);
        if (fieldLookupId != null)
        {
            series.SetAttribute("FieldLookupId", fieldLookupId);
        }

        series.SetAttribute("Aggregation", item.Aggregation.ToString());
        if (item.ColorsLookup != null)
        {
            series.SetAttribute("ColorsLookupId", item.ColorsLookupId.ToString());
        }

        return series;
    }

    private static void BuildColumnSeries(
        XmlElement seriesElement,
        ColumnSeries item,
        DataTable table
    )
    {
        XmlElement series = BuildBasicSeries(seriesElement, item, table);
        series.SetAttribute("ColumnSeriesType", item.Type.ToString());
        series.SetAttribute("ZAxisField", item.ZAxisField);
        string zAxisFieldLookupId = GetLookupId(table, item.ZAxisField, item);
        if (zAxisFieldLookupId != null)
        {
            series.SetAttribute("ZAxisFieldLookupId", zAxisFieldLookupId);
        }
    }

    private static void BuildLineSeries(XmlElement seriesElement, LineSeries item, DataTable table)
    {
        XmlElement series = BuildBasicSeries(seriesElement, item, table);
        series.SetAttribute("Type", item.GetType().Name);
        series.SetAttribute("LineSeriesForm", item.Form.ToString());
        series.SetAttribute("ZAxisField", item.ZAxisField);
        string zAxisFieldLookupId = GetLookupId(table, item.ZAxisField, item);
        if (zAxisFieldLookupId != null)
        {
            series.SetAttribute("ZAxisFieldLookupId", zAxisFieldLookupId);
        }
    }

    private static void BuildPieSeries(XmlElement seriesElement, PieSeries item, DataTable table)
    {
        XmlElement series = BuildBasicSeries(seriesElement, item, table);
        series.SetAttribute("Type", item.GetType().Name);
        series.SetAttribute("NameField", item.NameField);
        string nameFieldLookupId = GetLookupId(table, item.NameField, item);
        if (nameFieldLookupId != null)
        {
            series.SetAttribute("NameFieldLookupId", nameFieldLookupId);
        }
    }
}
