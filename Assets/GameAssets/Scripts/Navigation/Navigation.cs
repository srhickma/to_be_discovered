using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation {

	private Dictionary<Vector2, bool> excluded = new Dictionary<Vector2, bool>();

	private NavNode start, target;


	public Navigation(NavNode start, NavNode target){
		this.start = start;
		this.target = target;
	}

	public void begin(){
		traverse(start);
	}

	private NavNode traverse(NavNode currentNode){
		//Debug.DrawLine(currentNode.real, new Vector3(0, 0, 0), Color.red, 10000);
		if(currentNode.equals(target)){
			Debug.Log("HIT TARGET");
			return currentNode;
		}
		if(excluded.ContainsKey(currentNode.real)){
			return null;
		}
		excluded.Add(currentNode.real, false);
		List<NavNode> nodePriorities = getNodePriorities(currentNode);
		if(nodePriorities == null){
			return null;
		}
		NavNode next = null;
		foreach(NavNode node in nodePriorities){
			next = traverse(node);
			if(next != null){
				break;
			}
		}
		return next;
	}

	private List<NavNode> getNodePriorities(NavNode currentNode){
		List<NavNode> included = new List<NavNode>();
		int count = 0;
		foreach(NavNode node in currentNode.nodes){
			if(!excluded.ContainsKey(node.real)){
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

}
