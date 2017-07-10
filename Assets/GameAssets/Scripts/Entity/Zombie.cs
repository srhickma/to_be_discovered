using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour {

	private enum Direction {
		LEFT = -1,
		RIGHT = 1
	}

	private const float MAX_SPEED = 4f;
	private const float NAV_DELTA = 0.25f;
	private const float NAV_FALL_THROUGH_DELTA = 3f;

	private new Rigidbody2D rigidbody;
	
	private Transform groundCheck;

	public NavNode target;
	public LinkedListNode<NavNode> nextNode;
	private LinkedList<NavNode> path;
	
	private GameObject playerGO;
	private Player player;
	private NavAgent playerNavAgent, navAgent;

	private Vector2 targetPosition;

	private Direction moveDirection;

	private Interval aquireInterval;
	private Interval overMoveInterval;

	public bool onRamp;
	private bool overMoving;
	
	public bool canMove;

	private void Awake(){
		rigidbody = gameObject.GetComponent<Rigidbody2D>();
		groundCheck = transform.Find("GroundCheck");
		playerGO = GameObject.Find("Player");
		player = playerGO.GetComponent<Player>();
		playerNavAgent = playerGO.GetComponent<NavAgent>();
		navAgent = GetComponent<NavAgent>();

		aquireInterval = new Interval(aquirePath, 1f);
		overMoveInterval = new Interval(() => {
			overMoving = false;
			path.RemoveFirst();
		}, 0.1f);
	}

	private void Update(){
		targetPosition = playerGO.transform.position;
		
		
		aquireInterval.update();
		if(overMoving){
			overMoveInterval.update();
		}

		float destY = nextNode == null ? targetPosition.y : nextNode.Value.real.y;
		int destFloor = floorInBuilding(destY);
		int currFloor = floorInBuilding(transform.position.y);
		Physics2D.IgnoreLayerCollision(Constants.ZOMBIE_LAYER, Constants.FALL_THROUGH_LAYER, destFloor < currFloor);
		Physics2D.IgnoreLayerCollision(Constants.ZOMBIE_LAYER, Constants.FALL_THROUGH_RAMP_LAYER, !(destFloor > currFloor || nextNode != null || player.onRamp));
	}

	private void FixedUpdate(){
		if(canMove){
			move();
		}
	}

	public void aquirePath(){
		navAgent.updateClosestBuilding();
		if(navAgent.closestNode.equals(playerNavAgent.closestNode)){
			nextNode = null;
			return;
		}
		//if(!playerNavAgent.closestNode.equals(target)){
			target = playerNavAgent.closestNode;
			Navigation nav = new Navigation(navAgent.closestNode, target);
			path = nav.begin();
		//}
		nextNode = getNextNode();
		
		NavNode lastNode = null;
		foreach(var node in path){
			if(lastNode != null){
				Debug.DrawLine(node.real + Vector2.up * 2, lastNode.real + Vector2.up * 2, Color.red, 1);
			}
			lastNode = node;
		}
	}

	private void move(){
		Debug.DrawLine(transform.position, (Vector2)transform.position + Vector2.right * (int)moveDirection, Color.blue, 0.1f);

		if(!overMoving){
			getDirection();
		}
		rigidbody.velocity = new Vector2((int)moveDirection * MAX_SPEED, rigidbody.velocity.y);
	}

	private void getDirection(){
		float x = targetPosition.x;
		if(nextNode != null){
			if(Mathf.Abs(nextNode.Value.real.x - transform.position.x) < NAV_DELTA){
				nextNode = nextNode.Next;
				overMoving = true;
				return;
			}
			x = nextNode.Value.real.x;
		}
		moveDirection = x > transform.position.x ? Direction.RIGHT : Direction.LEFT;
	}

	private LinkedListNode<NavNode> getNextNode(){
		if(path == null){
			return null;
		}
		if(path.First.Next != null && (path.First.Value.y == path.First.Next.Value.y || 
		   path.First.Value.type == NavNode.Type.RAMP_BOTTOM && path.First.Next.Value.type == NavNode.Type.RAMP_TOP && 
		   onRamp && groundCheck.position.y > floorRealY(groundCheck.position.y) + 0.25f)){
			path.RemoveFirst();
		}
		return path.First;
	}

	private int floorInBuilding(float realY){
		return navAgent.getClosestBuilding().getFloorFromReal(realY);
	}

	private float floorRealY(float realY){
		Building building = navAgent.getClosestBuilding();
		int floor = building.getFloorFromReal(realY);
		return CoordinateSystem.toReal(floor * building.floorHeight);
	}
	
	private void OnCollisionEnter2D(Collision2D col){
		if(col.collider.CompareTag(Constants.FALL_THROUGH_RAMP_TAG)){
			onRamp = true;
		}
	}

	private void OnCollisionExit2D(Collision2D col){
		if(col.collider.CompareTag(Constants.FALL_THROUGH_RAMP_TAG)){
			onRamp = false;
		}
	}

}
