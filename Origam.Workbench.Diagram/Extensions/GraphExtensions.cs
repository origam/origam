using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Drawing;

namespace Origam.Workbench.Diagram.Extensions
{
    public static class GraphExtensions
    {

	    public static void RemoveNodeEverywhere(this Graph graph, Node node)
	    {
		    graph.RemoveNode(node);
		    foreach (Subgraph subGraph in graph.RootSubgraph.GetSubGraphs())
		    {
			    if (subGraph.Nodes.Contains(node))
			    {
				    subGraph.RemoveNode(node);
				    return;
			    }
		    }
	    }

	    public static IEnumerable<Subgraph> GetSubGraphs(this Subgraph subGraph)
        {
        	foreach (Subgraph childSubGraph1 in subGraph.Subgraphs)
        	{
        		foreach (Subgraph childSubGraph2 in GetSubGraphs(childSubGraph1))
        		{
        			yield return childSubGraph2;
        		}
        	}
        	yield return subGraph;
        }
    }
}