using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterController))]
public class CharacterMotor : MonoBehaviour {
	protected CharacterController character;
	protected bool jumpedEarly = false;
	
	public class Control {
		public Vector2 move = Vector2.zero;
		public Vector2 look = Vector2.zero;
		public bool jumpInput = false;
		public float delayJump = 0.0f;
		public bool jump {
			get { return jumpInput && delayJump <= 0.0f; }
			set { jumpInput = value; }
		}
		public bool wallRun;
	}
	public Control control = new Control();
	
	[System.Serializable]
	public class Movement {
		public float runSpeed = 10.0f; //Running speed in units per second
		public float sqrunSpeed { get { return runSpeed * runSpeed; } }
		public float groundFriction = 40.0f; //Deceleration of grounded player going above run speed in units per second per second
		public float drag = 1 / 500.0f; //Players drag in the air. Multiply by 1/2 velocity squared to get deceleration
		public float gravity = 10.5f;
	}
	public Movement move = new Movement();
	
	[System.Serializable]
	public class Sliding {
		public float slidingSpeed = 10.1f; //Running speed in units per second
		public float sqrSlidingSpeed { get { return slidingSpeed * slidingSpeed; } }
		public float stopRate = 5.0f; //Rate of deceleration when player opposes movement above sliding speed in units per second per second
	}
	public Sliding slide = new Sliding();
	
	[System.Serializable]
	public class Jumping {
		public float verticalJumpImpulse = 6.0f; //Players vertical change in speed when jumping up in units per second
		public float directionalJumpImpulse = 3f; //Players horizontal change in speed when jumping in a direction in units per second
		public float directionalJumpVerticalImpulse = 4.2f; //Players vertical change in speed when jumping in a direction in units per second
		public float earlyJumpPenalty = 0.1f; //Players horizontal loss of speed for jumping early as a fraction
	}
	public Jumping jump = new Jumping();
	
	[System.Serializable]
	public class WallRunning {
		public float minRisingGravity = 0.5f; //Minimum ratio of gravity that must be applied when wallrunning and rising
		public float risingGravityReductionSlope = 0.075f; //Rate of gravity reduction increase
		public float minFallingGravity = 0.25f;  //Minimum ratio of gravity that must be applied when wallrunning and falling
		public float fallingGravityReductionSlope = 0.075f; //Minimum ratio of gravity that must be applied when wallrunning and rising
		public float friction = 0.0f; //Deceleration of grounded player going above run speed in units per second per second
	}
	public WallRunning wallrun = new WallRunning();
	
	// Use this for initialization
	void Start () {
		character = GetComponent<CharacterController>();
		Vector3 euler = transform.eulerAngles;
		control.look = new Vector2(euler.x, euler.y);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(control.delayJump > 0.0f)
			control.delayJump -= Time.deltaTime;
		
		Vector3 velocity = character.velocity;
		//apply drag
		velocity -= velocity.normalized * Mathf.Clamp(velocity.sqrMagnitude/2 * move.drag * Time.deltaTime, 0.0f, velocity.magnitude);
		
		transform.eulerAngles = new Vector3(0.0f, control.look.y, 0.0f);
		
		if(character.isGrounded) {
			velocity.y = 0;
			
			if(jumpedEarly) {
				velocity *= (1 - jump.earlyJumpPenalty);
				jumpedEarly = false;
			}
			
			//calculate our movement input direction in world space
			Vector3 worldMove = new Vector3(control.move.x, 0, control.move.y);
			worldMove = transform.rotation * worldMove;
			
			if(velocity.sqrMagnitude <= slide.sqrSlidingSpeed) {
				velocity = move.runSpeed * worldMove;
			} else {
				Vector3 velDir = velocity.normalized;
				Vector3 accel = Vector3.zero;
				accel -= velDir * move.groundFriction;
				accel += Mathf.Clamp(Vector3.Dot(worldMove, velDir), -1f, 0f) * velDir;
				velocity += accel * Time.deltaTime;
			}
			if(control.jump) {
				if(velocity != Vector3.zero)
					velocity -= Mathf.Clamp(Vector3.Dot(velocity, worldMove) / velocity.magnitude, -1f, 0f) * worldMove;
				
				float directionality = control.move.magnitude;
				velocity += (1 - directionality) * Vector3.up * jump.verticalJumpImpulse;
				velocity += directionality * (Vector3.up * jump.directionalJumpVerticalImpulse + worldMove * jump.directionalJumpImpulse);
				
				control.jump = false;
			}
		} else {
			bool wallRunning = false;
			if(control.wallRun) {
				Vector3 hVel = new Vector3(velocity.x, 0.0f, velocity.z);
				Vector3 side = new Vector3(-velocity.z, 0.0f, velocity.x).normalized;
				Vector3 p1 = transform.position + character.center + Vector3.up * (-character.height*0.5f);
				Vector3 p2 = p1 + Vector3.up * character.height;
				RaycastHit info1;
				RaycastHit info2;
				bool hit1 = Physics.CapsuleCast(p1, p2, character.radius, side, out info1, 0.1f);
				bool hit2 = Physics.CapsuleCast(p1, p2, character.radius, -side, out info2, 0.1f);
				hit1 = hit1 && info1.collider.tag == "Structure";
				hit2 = hit2 && info2.collider.tag == "Structure";
				if(hit1 || hit2) {
					wallRunning = true;
					//apply friction
					if(wallrun.friction * Time.deltaTime > velocity.magnitude)
						velocity = Vector3.zero;
					else {
						Vector3 velDir = velocity.normalized;
						Vector3 accel = Vector3.zero;
						accel -= velDir * wallrun.friction;
						velocity += accel * Time.deltaTime;
					}
					
					//apply gravity
					float minGravity = velocity.y > 0 ? wallrun.minRisingGravity : wallrun.minFallingGravity;
					float gReductSlope = velocity.y > 0 ? wallrun.risingGravityReductionSlope : wallrun.fallingGravityReductionSlope;
					
					//not gonna explain why this works
					//just google or wulfrumalpha 0.25/(1 + 0.25 - 1/(x*0.075 + 1)) to see curve
					//look at x >= 0
					float reductionFactor = minGravity/(1 + minGravity - 1/(hVel.magnitude*gReductSlope + 1));
					velocity.y -= move.gravity * Time.deltaTime * reductionFactor;
					if(control.jump) {
						if(hit1 && (!hit2 || info1.distance < info2.distance))
							side *= -1;
						velocity += 0.75f * Vector3.up * jump.verticalJumpImpulse;
						velocity += 0.25f * (Vector3.up * jump.directionalJumpVerticalImpulse + side * jump.directionalJumpImpulse);
						
						control.jump = false;
						control.delayJump = 0.5f;
					}
				}
			} 
			if (!wallRunning){
				velocity.y -= move.gravity * Time.deltaTime;
				if(control.jump) {
					jumpedEarly = true;
				}
			}
		}
		
		character.Move(velocity * Time.deltaTime);
		Debug.Log(velocity.magnitude);
	}
}
