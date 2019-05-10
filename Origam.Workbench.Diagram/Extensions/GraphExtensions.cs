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
		    foreach ( var keyValPair in graph.SubgraphMap)
		    {
			    Subgraph subGraph = keyValPair.Value;
			    if (subGraph.Nodes.Contains(node))
			    {
				    subGraph.RemoveNode(node);
				    return;
			    }
		    }
	    }

	    public static Subgraph FindParentSubGraph(this Graph graph, Node node)
	    {
		    if (node == null) return null;
		    return graph
			    .SubgraphMap
			    .Select(x => x.Value)
			    .FirstOrDefault(subgraph => Equals(node, subgraph) ||
			                                 subgraph.Subgraphs.Contains(node) ||
			                                 subgraph.Nodes.Any(x => x.Id == node.Id));
	    }

	    public static Node FindNodeOrSubgraph(this Graph graph, string id)
	    {
		    if (string.IsNullOrWhiteSpace(id)) return null;
		    Node node = graph.FindNode(id);
		    if (node != null) return node;
		    return graph.SubgraphMap.ContainsKey(id) 
			    ? graph.SubgraphMap [id]
			    : null;
	    }

	    public static bool AreRelatives(this Graph graph, Node node1, Node node2)
	    {
		    if (node1 == null || node2 == null) return false;
		    if (node1 == node2) return true;
		    if (node1 is Subgraph subgraph1 && subgraph1.Nodes.Contains(node2))
		    {
			    return true;
		    }
		    if (node2 is Subgraph subgraph2 && subgraph2.Nodes.Contains(node1))
		    {
			    return true;
		    }
		    Subgraph parent = graph.FindParentSubGraph(node1);
		    if (parent == node2) return true;
		    if(parent.Nodes.Contains(node2)) return true;
		    return false;
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