using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterMotor))]
public class PlayerInputController : MonoBehaviour {
	protected CharacterMotor motor;
		
	// Use this for initialization
	void Start () {
		motor = GetComponent<CharacterMotor>();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Jump"))
			motor.control.jump = true;
		if(Input.GetButtonUp("Jump"))
			motor.control.jump = false;
		
		if(Input.GetButtonDown("Wall Run"))
			motor.control.wallRun = true;
		if(Input.GetButtonUp("Wall Run"))
			motor.control.wallRun = false;
		
		motor.control.move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
		
		motor.control.look.y += Input.GetAxis("Mouse X");
		if(motor.control.look.y > 360)
			motor.control.look.y -= 360;
		else if(motor.control.look.y < 0)
			motor.control.look.y += 360;
		
		motor.control.look.x -= Input.GetAxis("Mouse Y");
		motor.control.look.x = Mathf.Clamp(motor.control.look.x, -90f, 90f);
	}
}
