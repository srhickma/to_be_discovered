using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building {

	public int x { get; set; }
	public int width { get; set; }
	public int floorHeight { get; set; }
	public int floors { get; set; }
	public double windowFreq { get; set; }
	public Range range { get; set; }
	public GameObject parent { get; set; }
	public NavNode baseNodeL { get; set; }
	public NavNode baseNodeR { get; set; }
	private readonly NodeGroup[] nodeGroups;

	public Building(int x, int width, int floorHeight, int floors, double windowFreq){
		this.x = x;
		this.width = width;
		this.floorHeight = floorHeight;
		this.floors = floors;
		this.windowFreq = windowFreq;
		range = new Range(x, x + width - 1);
		nodeGroups = new NodeGroup[floors + 1];
		for(int i = 0; i < nodeGroups.Length; i ++){
			nodeGroups[i] = new NodeGroup();
		}
	}

	public NavNode createNavNode(int x, int y, NavNode.Type type){
		NavNode newNode = new NavNode(x, y, type);
		nodeGroups[getFloor(y)].addNode(newNode);
		return newNode;
	}

	public void createRampNavNodes(int startX, int endX, int y, bool topRight){
		int topY = y + floorHeight;
		NavNode nodeL, nodeR;
		if(topRight){
			nodeL = createNavNode(startX - 1, y, NavNode.Type.RAMP_BOTTOM);
			nodeR = createNavNode(endX + 1, topY, NavNode.Type.RAMP_TOP);
		}
		else{
			nodeL = createNavNode(startX - 1, topY, NavNode.Type.RAMP_TOP);
			nodeR = createNavNode(endX + 1, y, NavNode.Type.RAMP_BOTTOM);
		}
		NavNode.join(nodeL, nodeR);
	}

	public void connectNavNodes(){
		foreach(NodeGroup ng in nodeGroups){
			ng.connectNodes();
		}
	}

	public NodeGroup getClosestNodeGroup(float y){
		int floor = getFloor(y);
		if(floor < 0){
			floor = 0;
		}
		else if(floor > floors){
			floor = floors;
		}
		return nodeGroups[floor];
	}

	private int getFloor(int floorY){
		return floorY / floorHeight;
	}

	public int getFloor(float floorY){
		return (int)(floorY / floorHeight);
	}

}
