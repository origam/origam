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
    }
}