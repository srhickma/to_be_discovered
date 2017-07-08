using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	[SerializeField] private float maxSpeed = 8f;
	[SerializeField] private float jumpForce = 500f;
	[Range(0, 1)] [SerializeField] private float crouchSpeed = 0.25f;
	[SerializeField] private bool airControl = true;
	[SerializeField] private LayerMask whatIsGround;

	public Transform torso, head, arms, armtl, armtr, armbl, armbr, handl, handr, playerRig;

	private Transform groundCheck;
	const float groundedRadius = 0.2f;
	private bool grounded;
	private Transform ceilingCheck;
	const float ceilingRadius = 0.01f;

	private bool facingRight = true;
	private bool jump;
	private int walkDirection = 1;

	public static int JUMP_HEIGHT = 7;
	public static int HEIGHT = 4;
	private static int PLAYER_LAYER = 9;
	private static int FALL_THROUGH_LAYER = 8;
	private static string FALL_THROUGH_TAG = "fall_through";

	private Vector3 fallThroughPosition;
	private List<EdgeCollider2D> fallThroughsIgnored = new List<EdgeCollider2D>();
	private List<EdgeCollider2D> fallThroughsTouching = new List<EdgeCollider2D>();
	public static CapsuleCollider2D collider;
	private CircleCollider2D fallThroughCollider;
	private Animator animator;
	private Rigidbody2D rigidbody;
	private WeaponController weaponController;

	private void Awake(){
		groundCheck = transform.Find("GroundCheck");
		ceilingCheck = transform.Find("CeilingCheck");
		collider = gameObject.GetComponent<CapsuleCollider2D>();
		fallThroughCollider = gameObject.GetComponent<CircleCollider2D>();
		weaponController = playerRig.gameObject.GetComponent<WeaponController>();
		animator = GetComponent<Animator>();
		rigidbody = GetComponent<Rigidbody2D>();
		animator.SetFloat("WalkMultiplier", 1);
	}

	void Update(){
		if(fallThroughsIgnored.Count > 0 && (Vector3.Distance(fallThroughPosition, transform.position) > 2.0f || transform.position.y > fallThroughPosition.y + 0.1f)){
			foreach(EdgeCollider2D fallThrough in fallThroughsIgnored){
				Physics2D.IgnoreCollision(fallThroughCollider, fallThrough, false);
			}
			fallThroughsIgnored.Clear();
		}
		if(Input.GetButton("Crouch")){
			Physics2D.IgnoreLayerCollision(PLAYER_LAYER, FALL_THROUGH_LAYER, true);
			if(fallThroughsTouching.Count > 0){
				foreach(EdgeCollider2D fallThrough in fallThroughsTouching){
					Physics2D.IgnoreCollision(fallThroughCollider, fallThrough, true);
					fallThroughsIgnored.Add(fallThrough);
					fallThroughPosition = transform.position;
				}
			}
		}
		else{
			Physics2D.IgnoreLayerCollision(PLAYER_LAYER, FALL_THROUGH_LAYER, false);
		}
		if(!jump){
			jump = Input.GetButtonDown("Jump");
		}
		if(Input.GetMouseButtonDown(0)){
			weaponController.tryShootSemi();
		}
		else if(Input.GetMouseButton(0)){
			weaponController.tryShootAuto();
		}
	}

	void LateUpdate(){
		alignUpperBody();
	}

	void OnCollisionEnter2D(Collision2D col){
		if(col.gameObject.tag == FALL_THROUGH_TAG){
			fallThroughsTouching.Add(col.gameObject.GetComponent<EdgeCollider2D>());
		}
	}

	void OnCollisionExit2D(Collision2D col){
		if(col.gameObject.tag == FALL_THROUGH_TAG){
			fallThroughsTouching.Remove(col.gameObject.GetComponent<EdgeCollider2D>());
		}
	}

	private void FixedUpdate(){
		grounded = false;

		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
		for(int i = 0; i < colliders.Length; i++){
			if (colliders[i].gameObject != gameObject && !(colliders[i].gameObject.tag == FALL_THROUGH_TAG && Input.GetButton("Crouch")))
				grounded = true;
		}

		animator.SetBool("Ground", grounded);
		animator.SetFloat("vSpeed", rigidbody.velocity.y);

		move(Input.GetAxis("Horizontal"), Input.GetButton("Crouch"), jump);
		jump = false;
	}


	public void move(float move, bool crouch, bool jump){
		if(!grounded){
			crouch = false;
		}
		else if(!crouch && animator.GetBool("Crouch")){
			if (Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround)){
				crouch = true;
			}
		}
			
		animator.SetBool("Crouch", crouch);

		if(grounded || airControl){
			move = crouch ? move * crouchSpeed : move;

			animator.SetFloat("Speed", Mathf.Abs(move));

			rigidbody.velocity = new Vector2(move * maxSpeed, rigidbody.velocity.y);

			if(move != 0){
				if((move > 0) != facingRight){
					animator.SetFloat("WalkMultiplier", -1f);
				}
				else{
					animator.SetFloat("WalkMultiplier", 1f);
				}
			}
		}

		if(grounded && jump && animator.GetBool("Ground")){
			grounded = false;
			animator.SetBool("Ground", false);
			rigidbody.AddForce(new Vector2(0f, jumpForce));
		}
	}

	private void flipStance(){
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void alignUpperBody(){
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3 delta = mousePos - arms.position;
		float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
		if(Mathf.Abs(angle) > 91){
			if(facingRight){
				flipStance();
			}
			angle = (180 - Mathf.Abs(angle)) * Mathf.Sign(angle);
		}
		else if(!facingRight && Mathf.Abs(angle) < 89){
			flipStance();
		}
		arms.localRotation = Quaternion.Euler(0, 0, angle);
		head.localRotation = Quaternion.Euler(0, 0, angle / 3f);
	}

}
