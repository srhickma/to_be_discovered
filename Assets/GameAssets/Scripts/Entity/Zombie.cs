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

	public NavNode target;
	public LinkedListNode<NavNode> nextNode;
	private LinkedList<NavNode> path;
	
	private GameObject playerGO;
	private Player player;
	private NavAgent playerNavAgent, navAgent;

	private Direction moveDirection;

	private Interval aquireInterval;
	private Interval overMoveInterval;

	public bool overMoving;
	public bool canMove;

	private void Awake(){
		rigidbody = gameObject.GetComponent<Rigidbody2D>();

		playerGO = GameObject.Find("Player");
		player = playerGO.GetComponent<Player>();
		playerNavAgent = playerGO.GetComponent<NavAgent>();
		navAgent = GetComponent<NavAgent>();

		aquireInterval = new Interval(aquirePath, 1f);
		overMoveInterval = new Interval(() => overMoving = false, 0.1f);
	}

	private void Update(){
		aquireInterval.update();
		if(overMoving){
			overMoveInterval.update();
		}
		
		//float destinationY = nextNode == null ? playerGO.transform.position.y : nextNode.Value.real.y;
		//Physics2D.IgnoreLayerCollision(Constants.ZOMBIE_LAYER, Constants.FALL_THROUGH_LAYER,
		//	navAgent.getClosestBuilding().getFloor(destinationY) <
		//	navAgent.getClosestBuilding().getFloor(transform.position.y));
	}

	private void FixedUpdate(){
		if(canMove){
			move();
		}
	}
	
	private void OnCollisionEnter2D(Collision2D col){
		if(col.collider.CompareTag(Constants.FALL_THROUGH_TAG)){
			
		}
	}

	private void OnCollisionExit2D(Collision2D col){
		if(col.collider.CompareTag(Constants.FALL_THROUGH_TAG)){
			
		}
	}

	public void aquirePath(){
		if(navAgent.closestNode.equals(playerNavAgent.closestNode)){
			nextNode = null;
			return;
		}
		if(!playerNavAgent.closestNode.equals(target)){
			target = playerNavAgent.closestNode;
			Navigation nav = new Navigation(navAgent.closestNode, target);
			path = nav.begin();
			nextNode = path == null ? null : path.First;
		}
		
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
		float x = playerGO.transform.position.x;
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

}
