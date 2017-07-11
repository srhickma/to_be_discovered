using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Navigation {

	private LinkedListNode<Building> closestBuilding;
	private readonly HashSet<NavNode> excluded = new HashSet<NavNode>();

	private readonly NavNode start, target;
	private readonly Range targetBuildingRange;
	private bool inTargetBuilding;

	public Navigation(NavNode start, NavNode target){
		this.start = start;
		this.target = target;
		targetBuildingRange = getClosestBuildingRange(target.real.x);
	}

	public LinkedList<NavNode> begin(){
		LinkedList<NavNode> path = traverse(start);
		while(path != null && path.First.Next != null && path.First.Value.y == path.First.Next.Value.y){
			path.RemoveFirst();
		}
		return path;
	}

	private LinkedList<NavNode> traverse(NavNode currentNode){
		if(currentNode.equals(target)){
			LinkedList<NavNode> listStart = new LinkedList<NavNode>();
			listStart.AddFirst(currentNode);
			return listStart;
		}
		if(excluded.Contains(currentNode)){
			Debug.DrawLine(currentNode.real, Vector3.zero, Color.cyan, 1);
			return null;
		}
		excluded.Add(currentNode);
		if(!inTargetBuilding){
			inTargetBuilding = getClosestBuildingRange(currentNode.real.x).equals(targetBuildingRange);
		}
		var nodePriorities = getNodePriorities(currentNode);
		if(nodePriorities == null){
			Debug.DrawLine(currentNode.real, Vector3.zero, Color.magenta, 1);
			return null;
		}
		LinkedList<NavNode> next = null;
		foreach(var node in nodePriorities){
			Debug.DrawLine(currentNode.real, node.real, Color.yellow, 4);
			next = traverse(node);
			if(next != null){
				next.AddFirst(currentNode);
				break;
			}
		}
		return next;
	}

	private List<NavNode> getNodePriorities(NavNode currentNode){
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
			closestBuilding = BuildingGerator.buildingData.First;
		}
		while(closestBuilding.Previous != null && x < CoordinateSystem.toReal(closestBuilding.Value.x) - CoordinateSystem.BLOCK_WIDTH){
			closestBuilding = closestBuilding.Previous;
		}
		while(closestBuilding.Next != null && x > CoordinateSystem.toReal(closestBuilding.Next.Value.x) - CoordinateSystem.BLOCK_WIDTH){
			closestBuilding = closestBuilding.Next;
		}
		return closestBuilding.Value.range;
	}

}
