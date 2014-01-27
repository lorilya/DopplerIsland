using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour {
	protected CharacterMotor motor;
	
	public float healthPoints {
		get;
		set;
	}
	
	public bool IsDead {
		get { return healthPoints <= 0.0f; }
	}
	
	
	public CharacterController characterController;
	
	// Use this for initialization
	protected virtual void Start () {
		SetInitialProperties();
		characterController = GetComponent<CharacterController>();
		motor = GetComponent<CharacterMotor>();
	}
	
	void SetInitialProperties() {
		healthPoints = 100;
	}
}
