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
			if(nodes[i].type != NavNode.Type.RAMP_MIDDLE){
				for(int j = i + 1; j < nodes.Count; j++){
					if(nodes[j].type != NavNode.Type.RAMP_MIDDLE){
						NavNode.join(nodes[i], nodes[j]);
					}
				}
			}
		}
	}

}
