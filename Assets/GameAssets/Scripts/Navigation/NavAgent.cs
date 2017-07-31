using System;
using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour {

	private LinkedListNode<Building> closestBuilding;
	public NavNode closestNode { get; set; }
	public Transform refrerenceTransform;

	private Interval updateInterval;

	private void Awake(){
		updateInterval = new Interval(updateClosestNode, 0.5f);
	}

	private void Update(){
		updateInterval.update();
	}

	public void updateClosestBuilding(){
		if(closestBuilding == null){
			closestBuilding = BuildingGenerator.buildingData.First;
		}
		while(closestBuilding.Previous != null && refrerenceTransform.position.x < CoordinateSystem.toReal(closestBuilding.Value.x) - CoordinateSystem.BLOCK_WIDTH){
			closestBuilding = closestBuilding.Previous;
		}
		while(closestBuilding.Next != null && refrerenceTransform.position.x > CoordinateSystem.toReal(closestBuilding.Next.Value.x) - CoordinateSystem.BLOCK_WIDTH){
			closestBuilding = closestBuilding.Next;
		}
	}

	private void updateClosestNode(){
		updateClosestBuilding();
		Floor floor = closestBuilding.Value.getClosestFloor(CoordinateSystem.fromReal(refrerenceTransform.position.y));
		int currentX = (int)CoordinateSystem.fromReal(refrerenceTransform.position.x);
		NavNode closest = null;
		foreach(var node in floor.getNodes()){
			if(floor.noWallsBetween(new Range(node.x, currentX)) && 
			   (closest == null || Vector2.Distance(refrerenceTransform.position, node.real) < Vector2.Distance(refrerenceTransform.position, closest.real))){
				closest = node;
			}
		}
		closestNode = closest;
		
		Debug.DrawLine(refrerenceTransform.position, closestNode.real, Color.blue, 0.01f);
	}

	public Building getClosestBuilding(){
		return closestBuilding.Value;
	}

}
