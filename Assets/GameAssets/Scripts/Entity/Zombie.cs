using UnityEngine;

public class Zombie : MonoBehaviour {

	private enum Direction {
		LEFT = -1,
		RIGHT = 1
	}

	private const float MAX_SPEED = 4f;
	private const float NAV_DELTA = 0.25f;

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

		aquireInterval = new Interval(() => navAgent.aquirePath(playerNavAgent), 0.5f);
		overMoveInterval = new Interval(() => overMoving = false, 0.1f);
	}

	private void Update(){
		targetPosition = playerGO.transform.position;
		
		aquireInterval.update();
		if(overMoving){
			overMoveInterval.update();
		}
	}
	
	private void FixedUpdate(){
		destPosition = dest == null ? targetPosition : dest.real;
		
		if(canMove){
			Physics2D.IgnoreLayerCollision(Constants.ZOMBIE_LAYER, Constants.FALL_THROUGH_LAYER, navReference.position.y - destPosition.y > 2f);
			Physics2D.IgnoreLayerCollision(Constants.ZOMBIE_LAYER, Constants.FALL_THROUGH_RAMP_LAYER, 
				!(destPosition.y - navReference.position.y > 0.1f || 
				  dest == null && player.onRamp && floorInBuilding(transform.position.y) == floorInBuilding(targetPosition.y)
			));
			
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

}
