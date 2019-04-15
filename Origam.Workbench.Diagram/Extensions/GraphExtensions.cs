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

	    public static Subgraph FindParentSubGraph(this Graph graph, Node node)
	    {
		    return graph
			    .RootSubgraph
			    .GetSubGraphs()
			    .SingleOrDefault(subgraph => subgraph.Nodes.Contains(node));
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
	    
	    public static IEnumerable<Node> GetAllNodes(this Subgraph subGraph)
	    {
		    foreach (Subgraph childSubGraph in subGraph.Subgraphs)
		    {
			    foreach (Node childNode in GetAllNodes(childSubGraph))
			    {
				    yield return childNode;
			    }
		    }

		    foreach (Node node in subGraph.Nodes)
		    {
			    yield return node;
		    }

		    yield return subGraph;
	    }
    }
}