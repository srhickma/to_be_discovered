using UnityEngine;

public class Zombie : MonoBehaviour {

	private enum Direction {
		LEFT = -1,
		RIGHT = 1
	}

	private const float MAX_SPEED = 4f;
	private const float NAV_DELTA = 0.25f;
	private const int MAX_PLAYER_DISTANCE = 300;
	private const int MAX_IDLE_MOVE_RADIUS = 80;
	private static Range IDLE_MOVE_WAIT_SEC;

	private Vector2 idleTargetPosition;
	private int nextIdleMoveWaitSec;
	private float idleMoveWaitElapsed;

	private new Rigidbody2D rigidbody;
	
	private Transform groundCheck, navReference;

	private NavNode dest;
	
	private GameObject playerGO;
	private Player player;
	private NavAgent playerNavAgent, navAgent;

	private Vector2 targetPosition;
	private Vector2 destPosition = new Vector2(0, 0);

	private Direction moveDirection;

	private Interval aquireInterval;
	private Interval overMoveInterval;
	private Interval idleMoveInterval;
	
	private readonly RandomGenerator randomGenerator = new RandomGenerator();

	private bool overMoving;
	private bool needMove;
	public bool alerted;

	private void Awake(){
		rigidbody = gameObject.GetComponent<Rigidbody2D>();
		groundCheck = transform.Find("GroundCheck");
		navReference = transform.Find("NavReference");
		playerGO = GameObject.Find("Player");
		player = playerGO.GetComponent<Player>();
		playerNavAgent = playerGO.GetComponent<NavAgent>();
		navAgent = GetComponent<NavAgent>();

		aquireInterval = new Interval(() => navAgent.aquirePath(playerNavAgent), 0.5f);
		overMoveInterval = new Interval(() => overMoving = false, 0.1f);
		idleMoveInterval = new Interval(() => needMove = false, 10f);
		
		IDLE_MOVE_WAIT_SEC = new Range(4, 20);
	}

	private void Update(){
		updateMoveTarget();

		if(overMoving){
			overMoveInterval.update();
		}

		if(CoordinateSystem.fromReal(Mathf.Abs(transform.position.x - playerGO.transform.position.x)) > MAX_PLAYER_DISTANCE){
			despawn();
		}
	}

	private void updateMoveTarget(){
		if(alerted){
			aquireInterval.update();
			needMove = true;
			targetPosition = playerGO.transform.position;
		}
		else{
			if(!needMove){
				idleMoveWaitElapsed += Time.deltaTime;
				if(idleMoveWaitElapsed > nextIdleMoveWaitSec){
					int y = (int) CoordinateSystem.fromReal(transform.position.y);
					int x = (int) CoordinateSystem.fromReal(transform.position.x);
					int targetY = 0;
					int targetX;
					
					Building closestBuilding = navAgent.getClosestBuilding();
					int currentFloor = closestBuilding.getClosestFloor(y).floor;
					Range floorLimiter = Range.surrounding(currentFloor, 1);
					Range floors = new Range(0, closestBuilding.floors.Length - 1);
					Floor targetFloor = closestBuilding.floors[randomGenerator.nextInt(floors, floorLimiter)];
					
					if(closestBuilding.range.contains(x) && targetFloor.y != 0){
						targetY = targetFloor.y;
						targetX = closestBuilding.getTraversibleXOnFloor(targetFloor, randomGenerator);
					}
					else{
						targetX = randomGenerator.nextInt(Range.surrounding(x, MAX_IDLE_MOVE_RADIUS));
					}
					
					navAgent.aquirePath(targetX, targetY);
					idleTargetPosition = CoordinateSystem.toReal(targetX, targetY);
					
					idleMoveWaitElapsed = 0f;
					nextIdleMoveWaitSec = randomGenerator.nextInt(IDLE_MOVE_WAIT_SEC);
					needMove = true;
				}
			}
			else{
				if(Mathf.Abs(transform.position.x - idleTargetPosition.x) < 2f){
					idleMoveInterval.fire();
				}
				idleMoveInterval.update(!navAgent.onRamp);
			}
			
			targetPosition = idleTargetPosition;
		}
	}
	
	private void FixedUpdate(){
		if(needMove){
			destPosition = dest == null ? targetPosition : dest.real;

			bool ignoreFallThrough = navReference.position.y - destPosition.y > 2f;
			bool ignoreRamp = !(destPosition.y - navReference.position.y > 0.1f ||
			                    dest == null && player.onRamp &&
			                    floorInBuilding(transform.position.y) == floorInBuilding(targetPosition.y));

			if(ignoreFallThrough){
				setLayer(Constants.ZOMBIE_LAYER_IGNORE_ALL);
			}
			else if(ignoreRamp){
				setLayer(Constants.ZOMBIE_LAYER_IGNORE_RAMP);
			}
			else{
				setLayer(Constants.ZOMBIE_LAYER);
			}

			move();
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
		dest = navAgent.getDest();
		float x = targetPosition.x;
		if(dest != null){
			if(Mathf.Abs(dest.real.x - transform.position.x) < NAV_DELTA){
				overMoving = true;
				return;
			}
			x = dest.real.x;
		}
		moveDirection = x > transform.position.x ? Direction.RIGHT : Direction.LEFT;
	}

	private int floorInBuilding(float realY){
		return navAgent.getClosestBuilding().getFloorFromReal(realY);
	}

	private void setLayer(int layer){
		gameObject.layer = layer;
	}

	private void despawn(){
		ZombieSpawner.removeZombie(this);
		Destroy(gameObject);
	}

}
