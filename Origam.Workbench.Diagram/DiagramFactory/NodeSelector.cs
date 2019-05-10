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
        public Node Selected { get; set; }
        public Guid SelectedNodeId {
            get
            {
                if(Guid.TryParse(Selected?.Id, out Guid id)){
                    return id;
                }
                return Guid.Empty;
            }
        }
    }
}