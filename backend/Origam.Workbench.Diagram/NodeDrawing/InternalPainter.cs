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

using System.Drawing;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.Workbench.Diagram.Graphs;

namespace Origam.Workbench.Diagram.NodeDrawing;

internal class InternalPainter
{
    public readonly int LabelTopMargin = 8;
    public readonly double LabelSideMargin = 20;
    public readonly int ImageTopMargin = 8;
    public readonly int ImageRightMargin = 3;
    public readonly int ImageLeftMargin = 5;
    public readonly int HeadingBackgroundHeight = 30;
    public readonly int Margin = 3;
    public readonly int TextSideMargin = 15;
    public readonly Font Font = new Font("Arial", 10);
    public readonly StringFormat DrawFormat = new StringFormat();
    private readonly Graphics measurementGraphics = new Control().CreateGraphics();
    private Pen BoldBlackPen = new Pen(System.Drawing.Color.Black, 2);
    public Pen BlackPen { get; } = new Pen(System.Drawing.Color.Black, 1);
    public readonly SolidBrush BlackBrush = new SolidBrush(System.Drawing.Color.Black);
    public readonly SolidBrush LightGreyBrush = new SolidBrush(System.Drawing.Color.LightGray);
    public readonly SolidBrush DarkGreyBrush = new SolidBrush(System.Drawing.Color.DarkGray);
    public readonly SolidBrush GreenBrush = new SolidBrush(
        System.Drawing.Color.FromArgb(0, 154, 41)
    );
    public readonly SolidBrush RedBrush = new SolidBrush(
        System.Drawing.Color.FromArgb(255, 73, 61)
    );
    public readonly Brush WhiteBrush = new SolidBrush(System.Drawing.Color.White);
    public readonly int NodeHeaderHeight = 25;
    private readonly GViewer gViewer;
    internal INodeSelector NodeSelector { get; }

    public InternalPainter(INodeSelector nodeSelector, GViewer gViewer)
    {
        NodeSelector = nodeSelector;
        this.gViewer = gViewer;
    }

    internal Pen GetActiveBorderPen(Node node)
    {
        bool markAsSelected =
            Equals(NodeSelector.Selected, node)
            || (
                node is IWorkflowSubgraph subgraph
                && NodeSelector.Selected is IWorkflowSubgraph selectedSubgraph
                && Equals(subgraph.WorkflowItemId, selectedSubgraph.WorkflowItemId)
            );
        return markAsSelected ? BoldBlackPen : BlackPen;
    }

    internal Size CalculateMinHeaderBorder(Node node)
    {
        var nodeData = (INodeData)node.UserData;
        SizeF stringSize = measurementGraphics.MeasureString(node.LabelText, Font);
        int totalWidth = (int)(
            Margin + NodeHeaderHeight + TextSideMargin + stringSize.Width + TextSideMargin
        );
        if (nodeData.SecondaryImage != null)
        {
            totalWidth += NodeHeaderHeight;
        }

        return new Size(totalWidth, NodeHeaderHeight);
    }

    internal SizeF MeasureString(string nodeLabelText)
    {
        return measurementGraphics.MeasureString(nodeLabelText, Font);
    }

    internal float GetLabelWidth(Node node)
    {
        var nodeData = (INodeData)node.UserData;
        Image image = nodeData.PrimaryImage;
        SizeF stringSize = MeasureString(node.LabelText);
        var labelWidth = stringSize.Width + ImageRightMargin + image?.Width ?? 0;
        return labelWidth;
    }

    public Brush GetTextBrush(bool isFromActivePackage)
    {
        return isFromActivePackage ? BlackBrush : DarkGreyBrush;
    }
}
