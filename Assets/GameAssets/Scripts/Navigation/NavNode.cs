using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavNode {

	public enum Type {
		WALL,
		DOOR, 
		RAMP_TOP,
		RAMP_BOTTOM
	}

	public int x { get; set; }
	public int y { get; set; }
	public Vector2 real { get; set; }
	public Type type { get; set; }
	public List<NavNode> nodes { get; set; }
	private const double delta = 0.001;

	public NavNode(int x, int y, Type type){
		this.x = x;
		this.y = y;
		this.type = type;
		real = CoordinateSystem.toReal(x, y);
		nodes = new List<NavNode>();
	}

	public void addNode(NavNode node){
		nodes.Add(node);
		//Debug.DrawLine(CoordinateSystem.toReal(x, y), CoordinateSystem.toReal(node.x, node.y), Color.green, 1000);
	}

	public static void join(NavNode nodeA, NavNode nodeB){
		nodeA.addNode(nodeB);
		nodeB.addNode(nodeA);
	}

	public bool equals(NavNode node){
		return node != null && Mathf.Abs(real.x - node.real.x) < delta &&
			Mathf.Abs(real.y - node.real.y) < delta && type == node.type;
	}

	public double distanceTo(NavNode node){
		return Vector2.Distance(real, node.real);
	}

	public bool isJoinedWith(NavNode otherNode){
		return nodes.Any(otherNode.equals);
	}

}
