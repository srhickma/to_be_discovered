﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour {

	private LinkedListNode<Building> closestBuilding;
	public NavNode closestNode { get; set; }

	private Interval updateInterval;

	void Awake(){
		updateInterval = new Interval(updateClosestNode, 0.5f);
	}

	void Update(){
		updateInterval.update();
	}

	private void updateClosestBuilding(){
		if(closestBuilding == null){
			closestBuilding = BuildingGerator.buildingData.First;
		}
		while(closestBuilding.Previous != null && transform.position.x < CoordinateSystem.toReal(closestBuilding.Value.x) - CoordinateSystem.BLOCK_WIDTH){
			closestBuilding = closestBuilding.Previous;
		}
		while(closestBuilding.Next != null && transform.position.x > CoordinateSystem.toReal(closestBuilding.Next.Value.x) - CoordinateSystem.BLOCK_WIDTH){
			closestBuilding = closestBuilding.Next;
		}
	}

	private void updateClosestNode(){
		updateClosestBuilding();
		var closestNodeGroup = closestBuilding.Value.getClosestNodeGroup(
			CoordinateSystem.fromReal(transform.position.y)
		);
		var nodes = closestNodeGroup.getNodes();
		NavNode closest = null;
		foreach(var node in nodes){
			if(closest == null || Vector2.Distance(transform.position, node.real) < Vector2.Distance(transform.position, closest.real)){
				closest = node;
			}
		}
		closestNode = closest;
		
		Debug.DrawLine(transform.position, closestNode.real, Color.blue, 0.01f);
	}

}
