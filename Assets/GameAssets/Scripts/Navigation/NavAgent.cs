using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour {

	[SerializeField] private LayerMask whatIsGround;
	public Transform refrerenceTransform, rayCastTransform;
	
	private const float MAX_GROUND_RADIUS = 10f;
	private const int OLAP_COMPENSATION = 3;
	
	private LinkedListNode<Building> closestBuilding;
	private Floor closestFloor;
	public NavNode closestNode{ get; private set; }
	private NavNode LACNode;
	private NavNode LATNode;
	private NavPath path = NavPath.empty();

	public bool onRamp{ get; private set; }
	private bool canAquire = true;

	private Interval updateInterval;

	private void Awake(){
		updateInterval = new Interval(updateClosestNode, 0.5f);
	}

	private void Start(){
		updateClosestBuilding();
	}

	private void Update(){
		updateInterval.update();
	}

	public void aquirePath(NavAgent targetAgent){
		aquirePath(targetAgent.closestNode);
	}

	public void aquirePath(int targetX, int targetY){
		aquirePath(getTargetNode(targetX, targetY));
	}
	
	private void aquirePath(NavNode targetNode){
		updateClosestNode();
		if(targetNode == null || closestNode.equals(targetNode)){
			path.clear();
			return;
		}
		if(canAquire && !(targetNode.equals(LATNode) && closestNode.equals(LACNode) && closestNode.equals(path.first()))){
			LATNode = targetNode;
			LACNode = closestNode;
			canAquire = false;

			CompletableFuture<Navigation, NavPath> navFuture = CompletableFuture<Navigation, NavPath>
				.run(navigation => navigation.compute())
				.with(Navigation.track(closestNode, targetNode))
				.then(aquirePathCallback);
			
			NavComputeObject.enqueueNavigation(navFuture);
		}
		
		//TODO(SH) Remove in producition
		NavNode lastNode = null;
		foreach(var node in path.linkedPath()){
			if(lastNode != null){
				Debug.DrawLine(node.real + Vector2.up * 2, lastNode.real + Vector2.up * 2, Color.cyan, 0.1f);
			}
			lastNode = node;
		}
	}

	public void aquirePathCallback(NavPath path){
		this.path = path;
		trimPath(false);
		canAquire = true;
	}
	
	public NavNode getDest(){
		trimPath(true);
		return path.first();
	}
	
	public Building getClosestBuilding(){
		return closestBuilding.Value;
	}

	private void updateClosestBuilding(){
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

	private void updateClosestFloor(){
		updateClosestBuilding();
		closestFloor = closestBuilding.Value.getClosestFloor(CoordinateSystem.fromReal(refrerenceTransform.position.y));
	}
	
	private void updateClosestNode(){
		updateClosestFloor();
		int currentX = (int)CoordinateSystem.fromReal(refrerenceTransform.position.x);
		NavNode closest = null;
		foreach(var node in closestFloor.getNodes()){
			if(closestFloor.noWallsBetween(new Range(node.x, currentX)) && 
			   (closest == null || Vector2.Distance(refrerenceTransform.position, node.real) < Vector2.Distance(refrerenceTransform.position, closest.real))){
				closest = node;
			}
		}
		closestNode = closest;
	}
	
	private NavNode getTargetNode(int x, int y){
		LinkedListNode<Building> targetBuilding = closestBuilding ?? BuildingGenerator.buildingData.First;
		while(targetBuilding.Previous != null && x < targetBuilding.Value.x - 1){
			targetBuilding = targetBuilding.Previous;
		}
		while(targetBuilding.Next != null && x > targetBuilding.Next.Value.x - 1){
			targetBuilding = targetBuilding.Next;
		}
		Floor targetFloor = targetBuilding.Value.getClosestFloor(y);
		NavNode closest = null;
		Vector2 targetPosition = CoordinateSystem.toReal(x, y);
		foreach(var node in targetFloor.getNodes()){
			if(targetFloor.noWallsBetween(new Range(node.x, x)) && 
			   (closest == null || Vector2.Distance(targetPosition, node.real) < Vector2.Distance(targetPosition, closest.real))){
				closest = node;
			}
		}
		return closest;
	}
	private void trimPath(bool updateFloor){
		rayCast();
		if(updateFloor){
			updateClosestFloor();
		}
		LinkedList<NavNode> linkedPath = path.linkedPath();
		if(linkedPath.First == null){
			return;
		}
		while(linkedPath.First.Next != null && canReachNode(linkedPath.First.Value, linkedPath.First.Next.Value)){
			path.trim();
		}
		
		Range xRange = new Range((int) CoordinateSystem.fromReal(transform.position.x), linkedPath.First.Value.x);
		if(linkedPath.First.Next == null && closestFloor.y == linkedPath.First.Value.y && closestFloor.noWallsBetween(xRange)){
			path.trim();
		}
	}

	private void rayCast(){
		RaycastHit2D hit = Physics2D.Raycast(rayCastTransform.position, -Vector2.up, MAX_GROUND_RADIUS, whatIsGround);
		onRamp = hit.collider != null && hit.collider.gameObject.CompareTag(Constants.FALL_THROUGH_RAMP_TAG);
	}
	
	private bool canReachNode(NavNode from, NavNode to){
		int x = Mathf.RoundToInt(CoordinateSystem.fromReal(refrerenceTransform.position.x));
		int y = Mathf.RoundToInt(CoordinateSystem.fromReal(refrerenceTransform.position.y));
		int floorHeight = closestBuilding.Value.floorHeight;
		if(closestFloor.y == to.y - floorHeight){ //Up
			Range xRange = new Range(from.x, to.x);
			return onRamp && xRange.contains(x) && to.type == NavNode.Type.RAMP_TOP;
		}
		if(closestFloor.y == to.y + floorHeight){ //Down
			int signedCompensation = (to.x < from.x ? 1 : -1) * OLAP_COMPENSATION;
			Range xRange = new Range(to.x + signedCompensation, from.x);
			return xRange.contains(x) && to.type == NavNode.Type.RAMP_BOTTOM;
		}
		return closestFloor.y == to.y && y < to.y + floorHeight - 2; //Horizontal
	}

}
