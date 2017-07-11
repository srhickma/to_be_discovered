using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Zombie : MonoBehaviour {
	
	[SerializeField] private LayerMask rampLayerMask;

	private enum Direction {
		LEFT = -1,
		RIGHT = 1
	}

	private const float MAX_SPEED = 4f;
	private const float NAV_DELTA = 0.25f;
	private const float NAV_FALL_THROUGH_DELTA = 3f;
	private const float RAMP_DELTA = 5f;

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

	private Collider2D onRampCollider;
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

		aquireInterval = new Interval(aquirePath, 0.1f);
		overMoveInterval = new Interval(() => {
			overMoving = false;
			path.RemoveFirst();
		}, 0.1f);
	}

	private void Update(){
		targetPosition = playerGO.transform.position;
		
		aquireInterval.update();
		
		NavNode lastNode = null;
		foreach(var node in path){
			if(lastNode != null){
				Debug.DrawLine(node.real + Vector2.up * 2, lastNode.real + Vector2.up * 2, Color.red, 1);
			}
			lastNode = node;
		}
		
		if(overMoving){
			overMoveInterval.update();
		}

		nextNode = null;
		if(path != null){
			if(path.First == null){
				aquirePath();
			}
			nextNode = path.First;
		}

		float destY = nextNode == null ? targetPosition.y : nextNode.Value.real.y;
		if(nextNode == null){
			Debug.Log("Down" + (path == null) + (path != null && path.First == null));
		}
		int destFloor = floorInBuilding(destY);
		int currFloor = floorInBuilding(transform.position.y);
		Physics2D.IgnoreLayerCollision(Constants.ZOMBIE_LAYER, Constants.FALL_THROUGH_LAYER, destFloor < currFloor);
		//Physics2D.IgnoreLayerCollision(Constants.ZOMBIE_LAYER, Constants.FALL_THROUGH_RAMP_LAYER, !(destFloor > currFloor || nextNode == null && player.onRamp && floorInBuilding(transform.position.y) == floorInBuilding(targetPosition.y)));
	}
	
	private void FixedUpdate(){
		if(canMove){
			move();
		}
	}

	public void aquirePath(){
		if(navAgent.closestNode.equals(playerNavAgent.closestNode)){
			nextNode = null;
			return;
		}
		//if(!playerNavAgent.closestNode.equals(target)){
			target = playerNavAgent.closestNode;
			Navigation nav = new Navigation(navAgent.closestNode, target);
			path = nav.begin();
		//}
		trimPath();
	}
	
	private void trimPath(){
		if(path == null){
			return;
		}
		if(path.First.Next != null && (path.First.Value.y == path.First.Next.Value.y || onRamp || floorInBuilding(groundCheck.position.y) != floorInBuilding(path.First.Value.real.y))){
			path.RemoveFirst();
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
				traverse();
				overMoving = true;
				return;
			}
			x = nextNode.Value.real.x;
		}
		moveDirection = x > transform.position.x ? Direction.RIGHT : Direction.LEFT;
	}

	private int floorInBuilding(float realY){
		return navAgent.getClosestBuilding().getFloorFromReal(realY);
	}

	private void traverse(){
		if(nextNode != null){
			path.RemoveFirst();
			nextNode = path.First;
		}
	}
	
	private void OnCollisionEnter2D(Collision2D col){
		if(col.collider.CompareTag(Constants.FALL_THROUGH_RAMP_TAG)){
			Vector2 destPosition = nextNode == null ? targetPosition : nextNode.Value.real;
			ContactPoint2D contact = col.contacts[0];
			Debug.DrawLine(Vector2.zero, destPosition, Color.green, 10000);
			Debug.Log(nextNode == null);
			if(col.contacts.Length > 0 && Vector3.Dot(contact.normal, Vector3.up) > 0.5){
				onRamp = true;
				onRampCollider = col.collider;
			}
		}
	}

	private void OnCollisionExit2D(Collision2D col){
		if(col.collider.CompareTag(Constants.FALL_THROUGH_RAMP_TAG)){
			if(col.collider == onRampCollider){
				onRamp = false;
				onRampCollider = null;
			}
		}
	}

}
