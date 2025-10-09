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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Drawing;
using Origam.Workbench.Diagram.Graphs;

namespace Origam.Workbench.Diagram.Extensions;
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
	    IEnumerable<Subgraph> blockInnerSubgraphs = graph
		    .SubgraphMap
		    .Select(x => x.Value)
		    .OfType<BlockSubGraph>()
		    .SelectMany(x => x.Subgraphs);
	    return graph
		    .SubgraphMap
		    .Select(x => x.Value)
		    .Concat(new []{graph.RootSubgraph})
		    .Concat(blockInnerSubgraphs)
		    .FirstOrDefault(subgraph => subgraph.Subgraphs.Contains(node) ||
		                                subgraph.Nodes.Any(x => x.Id == node.Id));
    }
    public static Node FindNodeOrSubgraph(this Graph graph, string id)
    {
	    if (string.IsNullOrWhiteSpace(id)) return null;
	    if (graph.RootSubgraph.Id == id) return graph.RootSubgraph;
	    Node node = graph.FindNode(id);
	    if (node != null) return node;
	    return graph.SubgraphMap.ContainsKey(id) 
		    ? graph.SubgraphMap [id]
		    : null;
    }
    public static bool AreRelatives(this Graph graph, Node node1, Node node2)
    {
	    if (node1 == null || node2 == null) return false;
	    if (Equals(node1, node2)) return true;
	    if (node1 is Subgraph subgraph1 && subgraph1.Nodes.Contains(node2))
	    {
		    return true;
	    }
	    if (node2 is Subgraph subgraph2 && subgraph2.Nodes.Contains(node1))
	    {
		    return true;
	    }
	    Subgraph parent = graph.FindParentSubGraph(node1);
	    if (parent == null) return false;
	    if (Equals(parent, node2)) return true;
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
    
    public static IEnumerable<Subgraph> GetAllSubgraphs(this Subgraph subGraph)
    {
	    foreach (Subgraph childSubgraph in subGraph.Subgraphs)
	    {
		    foreach (Subgraph childSubgraph1 in GetAllSubgraphs(childSubgraph))
		    {
			    yield return childSubgraph1;
		    }
		    yield return childSubgraph;
	    }
    }
}
