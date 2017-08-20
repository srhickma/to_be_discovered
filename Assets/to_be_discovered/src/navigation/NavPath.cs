using System.Collections.Generic;

public class NavPath {

	private readonly LinkedList<NavNode> path = new LinkedList<NavNode>();

	public NavPath(NavNode end){
		addNode(end);
	}

	private NavPath(){}

	public static NavPath empty(){
		return new NavPath();
	}

	public void addNode(NavNode node){
		path.AddFirst(node);
	}
		
	public LinkedList<NavNode> linkedPath(){
		return path;
	}

	public void trim(){
		path.RemoveFirst();
	}

	public void clear(){
		path.Clear();
	}

	public NavNode first(){
		return path.First == null ? null : path.First.Value;
	}
	
}
