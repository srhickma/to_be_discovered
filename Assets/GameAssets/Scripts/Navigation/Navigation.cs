using System.Collections.Generic;

public class Navigation {

	private LinkedListNode<Building> closestBuilding;

	private readonly NavNode start, target;
	private readonly Range targetBuildingRange;
	private bool inTargetBuilding;

	public Navigation(NavNode start, NavNode target){
		this.start = start;
		this.target = target;
		targetBuildingRange = getClosestBuildingRange(target.real.x);
	}

	public LinkedList<NavNode> begin(){
		LinkedList<NavNode> path = traverse().getPath();
		while(path != null && path.First.Next != null && path.First.Value.y == path.First.Next.Value.y){
			path.RemoveFirst();
		}
		return path ?? new LinkedList<NavNode>();
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

		return null;
	}

	private NavPath extractNavPath(TraversalNode finalNode){
		NavPath path = new NavPath(finalNode.node);
		for(TraversalNode currentNode = finalNode.prev; currentNode != null; currentNode = currentNode.prev){
			path.addNode(currentNode.node);
		}
		return path;
	}

	private List<TraversalNode> getConnectedNodes(TraversalNode currentNode, HashSet<NavNode> excluded){
		var included = new List<TraversalNode>();
		foreach(var node in currentNode.node.nodes){
			if((!inTargetBuilding || getClosestBuildingRange(node.real.x).equals(targetBuildingRange)) && !excluded.Contains(node)){
				included.Add(new TraversalNode(node, currentNode));
			}
		}
		return included;
	}
	
	private Range getClosestBuildingRange(float x){
		if(closestBuilding == null){
			closestBuilding = BuildingGenerator.buildingData.First;
		}
		while(closestBuilding.Previous != null && x < CoordinateSystem.toReal(closestBuilding.Value.x) - CoordinateSystem.BLOCK_WIDTH){
			closestBuilding = closestBuilding.Previous;
		}
		while(closestBuilding.Next != null && x > CoordinateSystem.toReal(closestBuilding.Next.Value.x) - CoordinateSystem.BLOCK_WIDTH){
			closestBuilding = closestBuilding.Next;
		}
		return closestBuilding.Value.range;
	}
	
	private class TraversalNode {

		public NavNode node { get; set; }
		public TraversalNode prev { get; set; }

		public TraversalNode(NavNode node, TraversalNode prev){
			this.node = node;
			this.prev = prev;
		}

	}

	private class NavPath {
		
		private readonly LinkedList<NavNode> path = new LinkedList<NavNode>();

		public NavPath(NavNode end){
			path.AddFirst(end);
		}

		public void addNode(NavNode node){
			path.AddFirst(node);
		}
		
		public LinkedList<NavNode> getPath(){
			return path;
		}

	}

}
