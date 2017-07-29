using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour {
	
	[SerializeField] private LayerMask whatIsGround;

	private enum Direction {
		LEFT = -1,
		RIGHT = 1
	}

	private const float MAX_SPEED = 4f;
	private const float NAV_DELTA = 0.25f;
	private const float MAX_GROUND_RADIUS = 10f;

	private new Rigidbody2D rigidbody;
	
	private Transform groundCheck, navReference;

	public NavNode target;
	private NavNode closestNode;
	private NavNode lastRemoved;
	public LinkedListNode<NavNode> nextNode;
	private LinkedList<NavNode> path;
	
	private GameObject playerGO;
	private Player player;
	private NavAgent playerNavAgent, navAgent;

	private Vector2 targetPosition;
	private Vector2 destPosition = new Vector2(0, 0);

	private Direction moveDirection;

	private Interval aquireInterval;
	private Interval overMoveInterval;
	private Interval trimInterval;

	public bool onRamp, onFallThrough;
	private bool overMoving;
	
	public bool canMove;

	private void Awake(){
		rigidbody = gameObject.GetComponent<Rigidbody2D>();
		groundCheck = transform.Find("GroundCheck");
		navReference = transform.Find("NavReference");
		playerGO = GameObject.Find("Player");
		player = playerGO.GetComponent<Player>();
		playerNavAgent = playerGO.GetComponent<NavAgent>();
		navAgent = GetComponent<NavAgent>();

		aquireInterval = new Interval(aquirePath, 0.5f);
		overMoveInterval = new Interval(() => {
			overMoving = false;
			path.RemoveFirst();
		}, 0.1f);
		trimInterval = new Interval(trimPath, 0.1f);
	}

	private void Update(){
		targetPosition = playerGO.transform.position;
		
		aquireInterval.update();
		if(overMoving){
			overMoveInterval.update();
		}
		trimInterval.update();
		
//		NavNode lastNode = null;
//		foreach(var node in path){
//			if(lastNode != null){
//				Debug.DrawLine(node.real + Vector2.up * 2, lastNode.real + Vector2.up * 2, Color.red, 1);
//			}
//			lastNode = node;
//		}

		nextNode = null;
		if(path != null){
			if(path.First == null){
				aquirePath();
			}
			nextNode = path.First;
		}
	}
	
	private void FixedUpdate(){
		destPosition = nextNode == null ? targetPosition : nextNode.Value.real;
		if(canMove){
			Physics2D.IgnoreLayerCollision(Constants.ZOMBIE_LAYER, Constants.FALL_THROUGH_LAYER, navReference.position.y - destPosition.y > 2f);
			Physics2D.IgnoreLayerCollision(Constants.ZOMBIE_LAYER, Constants.FALL_THROUGH_RAMP_LAYER, !(destPosition.y - navReference.position.y > 0.1f || nextNode == null && player.onRamp && floorInBuilding(transform.position.y) == floorInBuilding(targetPosition.y)));
			
			move();
		}
	}

	public void aquirePath(){
		if(navAgent.closestNode.equals(playerNavAgent.closestNode)){
			nextNode = null;
			return;
		}
		if(!(playerNavAgent.closestNode.equals(target) && navAgent.closestNode.equals(closestNode) && (closestNode.equals(lastRemoved) || closestNode.equals(path.First.Value)))){
			target = playerNavAgent.closestNode;
			closestNode = navAgent.closestNode;
			Navigation nav = new Navigation(closestNode, target);
			path = nav.begin();
		}
		trimPath();
	}
	
	private void trimPath(){
		if(path == null){
			nextNode = null;
			return;
		}

		if(path.First.Next != null){
			RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, -Vector2.up, MAX_GROUND_RADIUS, whatIsGround);
			onRamp = hit.collider != null && hit.collider.gameObject.CompareTag(Constants.FALL_THROUGH_RAMP_TAG);
			onFallThrough = hit.collider != null && hit.collider.gameObject.CompareTag(Constants.FALL_THROUGH_TAG);
			overMoving = onFallThrough && (onRamp || path.First.Value.y > path.First.Next.Value.y);
			if(Mathf.Abs(path.First.Next.Value.real.y - navReference.position.y) < 2f || onRamp && path.First.Value.y != path.First.Next.Value.y || onFallThrough && path.First.Value.y > path.First.Next.Value.y){
				lastRemoved = path.First.Value;
				path.RemoveFirst();
			}
		}
		nextNode = path.First;
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

}
