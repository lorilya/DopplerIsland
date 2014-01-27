using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterMotor))]
public class MouseLookCamera : MonoBehaviour {
	CharacterMotor motor;
	public Camera attachedCamarea;

	// Use this for initialization
	void Start () {
		motor = GetComponent<CharacterMotor>();
	}
	
	// Update is called once per frame
	void Update () {
		attachedCamarea.transform.eulerAngles = new Vector3(motor.control.look.x, motor.control.look.y, 0.0f);
	}
}
