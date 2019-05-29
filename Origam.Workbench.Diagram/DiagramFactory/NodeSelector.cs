using System;
using Microsoft.Msagl.Drawing;

namespace Origam.Workbench.Diagram
{
    public interface INodeSelector
    {
        Node Selected { get; }  
    }

    public class NodeSelector: INodeSelector
    {
        private Node selected;
        public Guid? SelectedNodeId { get; private set; }

        public Node Selected
        {
            get => selected;
            set
            {
                if(Guid.TryParse(value?.Id, out Guid id)){
                    SelectedNodeId = id;
                }
                else
                {
                    SelectedNodeId = null;
                }
                selected = value;
            }
        }
    }
}