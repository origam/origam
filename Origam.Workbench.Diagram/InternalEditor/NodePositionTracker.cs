using System;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.Workbench.Diagram.Extensions;

namespace Origam.Workbench.Diagram.InternalEditor
{
    class NodePositionTracker
    {
        private readonly GViewer gViewer;
        private readonly PlaneTransformation originalTransformation;
        private readonly string nodeId;
        private readonly Point pointOnScreen;
        private readonly Node originalNode;
        private Node currentNode;

        private Point CurrentSourcePoint => currentNode.GeometryNode.Center;

        public bool NodeWasNotNodeWasResized =>
            Math.Abs(originalNode.BoundingBox.Size.Height - currentNode.BoundingBox.Size.Height) < 0.01 &&
            Math.Abs(originalNode.BoundingBox.Size.Width - currentNode.BoundingBox.Size.Width) < 0.01;

        public bool NodeDoesNotExist =>
            string.IsNullOrWhiteSpace(nodeId) || currentNode ==null;

        public PlaneTransformation UpdatedTransformation =>
            new PlaneTransformation(
                originalTransformation[0, 0],
                originalTransformation[0, 1],
                pointOnScreen.X - CurrentSourcePoint.X,
                originalTransformation[1, 0],
                originalTransformation[1, 1],
                pointOnScreen.Y + CurrentSourcePoint.Y);

        public NodePositionTracker( GViewer gViewer, string nodeId)
        {
            this.gViewer = gViewer;
            this.nodeId = nodeId;
            originalNode = gViewer.Graph.FindNodeOrSubgraph(nodeId);
            originalTransformation = gViewer.Transform;
            if (string.IsNullOrWhiteSpace(this.nodeId)) return;
            pointOnScreen = gViewer.Transform * originalNode.GeometryNode.Center;
        }
			

        public void LoadUpdatedState()
        {
            currentNode = gViewer.Graph.FindNodeOrSubgraph(nodeId);
        }
    }
}