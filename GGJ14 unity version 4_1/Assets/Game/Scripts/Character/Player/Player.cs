using UnityEngine;
using System.Collections;

public class Player : Character {
	private Color currentLightColour;
	
	public Light[] managedLights = {};
	
	protected override void Start () {
		base.Start();
	}
	
	// FixedUpdate is called once per game tick
	void Update () {
		UpdateLightBasedOnVelocity();
	}
	
	void UpdateLightBasedOnVelocity() {
		float speed = Mathf.Clamp(characterController.velocity.magnitude - motor.move.runSpeed + 0.1f, 0.0f, float.PositiveInfinity);
		float redBlueRatio = 1/(speed / 10.0f + 1);
		Color color = Color.red * redBlueRatio + Color.blue * (1 - redBlueRatio);
		foreach(Light light in managedLights) {
			float changeRate = 0.125f * Time.deltaTime;
			float dR = color.r - light.color.r;
			float dG = color.g - light.color.g;
			float dB = color.b - light.color.b;
			float r = color.r;
			float g = color.g;
			float b = color.b;
			
			if(Mathf.Abs(dR) > changeRate) {
				r = light.color.r + changeRate * Mathf.Sign(dR);
			}
			if(Mathf.Abs(dG) > changeRate) {
				g = light.color.g + changeRate * Mathf.Sign(dG);
			}
			if(Mathf.Abs(dB) > changeRate) {
				b = light.color.b + changeRate * Mathf.Sign(dB);
			} 
			light.color = new Color(r, g, b);
		}
	}
}
