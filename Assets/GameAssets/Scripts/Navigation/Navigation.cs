using System.Collections.Generic;
using UnityEngine;

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
		LinkedList<NavNode> path = traverse(start, new HashSet<NavNode>()).getPath();
		while(path != null && path.First.Next != null && path.First.Value.y == path.First.Next.Value.y){
			path.RemoveFirst();
		}
		return path ?? new LinkedList<NavNode>();
	}

	private NavPath traverse(NavNode currentNode, HashSet<NavNode> excluded){
		if(currentNode.equals(target)){
			return new NavPath(currentNode);
		}
		if(excluded.Contains(currentNode)){
			return null;
		}
		excluded.Add(currentNode);
		if(!inTargetBuilding){
			inTargetBuilding = getClosestBuildingRange(currentNode.real.x).equals(targetBuildingRange);
		}
		var nodePriorities = getNodePriorities(currentNode, excluded);
		if(nodePriorities == null){
			return null;
		}
		NavPath shortestPath = null;
		foreach(var node in nodePriorities){
			NavPath path = traverse(node, new HashSet<NavNode>(excluded));
			if(path != null){
				path.addNode(currentNode);
				if(shortestPath == null || path.isShorterThan(shortestPath)){
					shortestPath = path;
				}
			}
		}
		return shortestPath;
	}

	private List<NavNode> getNodePriorities(NavNode currentNode, HashSet<NavNode> excluded){
		var included = new List<NavNode>();
		int count = 0;
		foreach(var node in currentNode.nodes){
			if((!inTargetBuilding || getClosestBuildingRange(node.real.x).equals(targetBuildingRange)) && !excluded.Contains(node)){
				included.Add(node);
				count ++;
			}
		}
		if(count == 0){
			return null;
		}

		included.Sort((nn1, nn2) => nn1.distanceTo(target).CompareTo(nn2.distanceTo(target)));
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

	private class NavPath {
		
		private readonly LinkedList<NavNode> path = new LinkedList<NavNode>();
		private double distance;

		public NavPath(NavNode end){
			path.AddFirst(end);
			distance = 0;
		}

		public void addNode(NavNode node){
			distance += Vector2.Distance(path.First.Value.real, node.real);
			path.AddFirst(node);
		}
		
		public LinkedList<NavNode> getPath(){
			return path;
		}

		public bool isShorterThan(NavPath path){
			Debug.Log(distance + "::" + path.distance);
			return distance < path.distance;
		}

	}

}
