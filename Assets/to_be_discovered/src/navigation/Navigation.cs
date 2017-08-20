using System.Collections.Generic;

public class Navigation {
	
	private readonly NavNode start, target;

	private Navigation(NavNode start, NavNode target){
		this.start = start;
		this.target = target;
	}
	
	public static Navigation track(NavNode start, NavNode target){
		return new Navigation(start, target);
	}

	public NavPath compute(){
		NavPath path = traverse();
		LinkedList<NavNode> linkedPath = path.linkedPath();
		if(linkedPath.First == null){
			return path;
		}
		while(linkedPath.First.Next != null && linkedPath.First.Value.y == linkedPath.First.Next.Value.y){
			path.trim();
		}
		return path;
	}

	private NavPath traverse(){
		HashSet<NavNode> excluded = new HashSet<NavNode>();
		SortedLinkedList<TraversalNode> options = new SortedLinkedList<TraversalNode>((a, b) => a.node.distanceTo(target).CompareTo(b.node.distanceTo(target)));
		
		TraversalNode currentNode = new TraversalNode(start, null);
		options.add(getConnectedNodes(currentNode, excluded));
		while(!options.isEmpty()){
			if(currentNode.node.equals(target)){
				return extractNavPath(currentNode);
			}
			excluded.Add(currentNode.node);

			currentNode = options.popFirst();
			options.add(getConnectedNodes(currentNode, excluded));
		}

		return NavPath.empty();
	}

	private static NavPath extractNavPath(TraversalNode finalNode){
		NavPath path = new NavPath(finalNode.node);
		for(TraversalNode currentNode = finalNode.prev; currentNode != null; currentNode = currentNode.prev){
			path.addNode(currentNode.node);
		}
		return path;
	}

	private static List<TraversalNode> getConnectedNodes(TraversalNode currentNode, HashSet<NavNode> excluded){
		var included = new List<TraversalNode>();
		foreach(var node in currentNode.node.nodes){
			if(!excluded.Contains(node)){
				included.Add(new TraversalNode(node, currentNode));
			}
		}
		return included;
	}
	
	private class TraversalNode {

		public NavNode node { get; private set; }
		public TraversalNode prev { get; private set; }

		public TraversalNode(NavNode node, TraversalNode prev){
			this.node = node;
			this.prev = prev;
		}

	}

}
