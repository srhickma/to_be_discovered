using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour {

	private enum Direction {
		LEFT = -1,
		RIGHT = 1,
		STILL = 0
	}

	private float maxSpeed = 4f;

	private Rigidbody2D rigidbody;

	public NavNode target;
	public NavNode next;

	private float navDelta = 1f;
	private float navFallThroughDelta = 3f;

	private GameObject playerGO;
	private Player player;
	private NavAgent playerNavAgent, navAgent;

	private Interval aquireInterval;

	void Awake(){
		rigidbody = gameObject.GetComponent<Rigidbody2D>();

		playerGO = GameObject.Find("Player");
		player = playerGO.GetComponent<Player>();
		playerNavAgent = playerGO.GetComponent<NavAgent>();
		navAgent = GetComponent<NavAgent>();

		aquireInterval = new Interval(aquirePath, 1f);
	}

	void Update(){
		aquireInterval.update();
	}

	bool aquired = false;

	private void FixedUpdate(){
		//move();
	}

	public void aquirePath(){
		if(!playerNavAgent.closestNode.equals(target)){
			target = playerNavAgent.closestNode;
			Debug.Log("AQUIRING_PATH");
			Navigation nav = new Navigation(navAgent.closestNode, target);
			nav.begin();
		}
	}

	private void move(){
		if(next != null && Mathf.Abs(next.x - transform.position.x) > navDelta){
			Direction direction;
			if(next.x > transform.position.x){
				direction = Direction.RIGHT;
			}
			else{
				direction = Direction.LEFT;
			}
			rigidbody.velocity = new Vector2((int)direction * maxSpeed, rigidbody.velocity.y);
		}
	}

}
