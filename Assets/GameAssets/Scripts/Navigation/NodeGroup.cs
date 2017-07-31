using System.Collections.Generic;
using System.Linq;

public class NodeGroup {

	public List<NavNode> nodes = new List<NavNode>();

	public void addNode(NavNode node){
		nodes.Add(node);
	}

	public List<NavNode> getNodes(){
		return nodes;
	}

	public void connectNodes(List<int> walls){
		nodes.Sort((n1, n2) => n1.x.CompareTo(n2.x));
		for(int i = 0; i < nodes.Count - 1; i ++){
			Range joinRange = new Range(nodes[i].x, nodes[i + 1].x);
			if(walls.All(x => !joinRange.contains(x))){
				NavNode.join(nodes[i], nodes[i + 1]);
			}
		}
	}

}
