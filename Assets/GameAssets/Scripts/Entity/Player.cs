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
	private const float groundedRadius = 0.2f;
	private bool grounded;
	public bool onRamp { get; set; }
	private Transform ceilingCheck;
	private const float ceilingRadius = 0.01f;

	private bool facingRight = true;
	private bool jump;
	private int walkDirection = 1;

	public static int JUMP_HEIGHT = 7;
	public static int HEIGHT = 4;

	private Vector3 fallThroughPosition;
	private readonly List<EdgeCollider2D> fallThroughsIgnored = new List<EdgeCollider2D>();
	private readonly List<EdgeCollider2D> fallThroughsTouching = new List<EdgeCollider2D>();
	public new static CapsuleCollider2D collider;
	private CircleCollider2D fallThroughCollider;
	private Animator animator;
	private new Rigidbody2D rigidbody;
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

	private void Update(){
		if(fallThroughsIgnored.Count > 0 && (Vector3.Distance(fallThroughPosition, transform.position) > 2.0f || transform.position.y > fallThroughPosition.y + 0.1f)){
			foreach(EdgeCollider2D fallThrough in fallThroughsIgnored){
				Physics2D.IgnoreCollision(fallThroughCollider, fallThrough, false);
			}
			fallThroughsIgnored.Clear();
		}
		bool crouchPressed = Input.GetButton("Crouch");
		Physics2D.IgnoreLayerCollision(Constants.PLAYER_LAYER, Constants.FALL_THROUGH_LAYER, crouchPressed);
		Physics2D.IgnoreLayerCollision(Constants.PLAYER_LAYER, Constants.FALL_THROUGH_RAMP_LAYER, crouchPressed);
		if(crouchPressed){
			if(fallThroughsTouching.Count > 0){
				foreach(EdgeCollider2D fallThrough in fallThroughsTouching){
					Physics2D.IgnoreCollision(fallThroughCollider, fallThrough, true);
					fallThroughsIgnored.Add(fallThrough);
					fallThroughPosition = transform.position;
				}
			}
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

	private void LateUpdate(){
		alignUpperBody();
	}

	private void OnCollisionEnter2D(Collision2D col){
		if(col.collider.CompareTag(Constants.FALL_THROUGH_TAG)){
			fallThroughsTouching.Add(col.gameObject.GetComponent<EdgeCollider2D>());
		}
		else if(col.collider.CompareTag(Constants.FALL_THROUGH_RAMP_TAG)){
			fallThroughsTouching.Add(col.gameObject.GetComponent<EdgeCollider2D>());
			onRamp = true;
		}
	}

	private void OnCollisionExit2D(Collision2D col){
		if(col.collider.CompareTag(Constants.FALL_THROUGH_TAG)){
			fallThroughsTouching.Remove(col.gameObject.GetComponent<EdgeCollider2D>());
		}
		else if(col.collider.CompareTag(Constants.FALL_THROUGH_RAMP_TAG)){
			fallThroughsTouching.Remove(col.gameObject.GetComponent<EdgeCollider2D>());
			onRamp = false;
		}
	}

	private void FixedUpdate(){
		grounded = false;

		foreach(Collider2D collider in Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround)){
			if(collider.gameObject != gameObject &&
			   !(collider.CompareTag(Constants.FALL_THROUGH_TAG) && Input.GetButton("Crouch"))){
				grounded = true;
			}
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
		else if(!crouch && animator.GetBool("Crouch") && Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround)){
			crouch = true;
		}
		animator.SetBool("Crouch", crouch);

		if(grounded || airControl){
			move = crouch ? move * crouchSpeed : move;

			animator.SetFloat("Speed", Mathf.Abs(move));

			rigidbody.velocity = new Vector2(move * maxSpeed, rigidbody.velocity.y);

			if(move != 0){
				if(move > 0 != facingRight){
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
