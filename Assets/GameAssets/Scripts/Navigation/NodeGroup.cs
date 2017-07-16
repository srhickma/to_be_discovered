using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGroup {

	public List<NavNode> nodes = new List<NavNode>();

	public void addNode(NavNode node){
		nodes.Add(node);
	}

	public List<NavNode> getNodes(){
		return nodes;
	}

	public void connectNodes(){
		for(int i = 0; i < nodes.Count - 1; i ++){
			for(int j = i + 1; j < nodes.Count; j++){
					NavNode.join(nodes[i], nodes[j]);
			}
		}
	}

}
