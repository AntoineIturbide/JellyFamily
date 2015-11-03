using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CharacterPhysic  {

	/* ATTRIBUTES */

	// Linked character rigidbody
	[HideInInspector]
    public Rigidbody2D _rigidbody;

	// Velocity conserved between frames
	[HideInInspector]
    public Vector3 _conservedVelocity = Vector3.zero;

	// Last collision (used to detect if character's hitbox is on ground)
	public Collision2D _lastCollision = new Collision2D();

	// List of all _impulses;
	public List<Vector3> _impulses = new List<Vector3>();

    //[HideInInspector]
    public float _timeAtLastColision = 0f;

	/* REFERENCES */

	// World's gravity vector
	public Vector3 _gravity {
		get{ return Physics.gravity; }
	}

	// Character's mass
	public float _mass {
		get{ return _rigidbody.mass; }
	}

    // Return if the player is on ground
    [HideInInspector]
    public float _onGroundAccuracy = 0.05f;
	public bool _onGround {
		get { return (Time.fixedTime - _timeAtLastColision) <= _onGroundAccuracy; }
	}



	/* METHODS */

	public void AddImpulse(Vector3 impulse){
		_impulses.Add(impulse);
	}

	// Apply gravity to the conserved velocity
	public void ApplyGravity(){
		// Calculate how to aply gravity based on character's mass, the gravity vector and time
		_conservedVelocity += _mass * _gravity * Time.fixedDeltaTime;
	}

	// Apply jump to the conserved velocity
	public void ApplyJump(CharacterJump jump){
		// Calculate how to aply a jum based on character's mass, the gravity vector and time
		_conservedVelocity += jump._jumpStrengh * -_gravity.normalized;
	}

	// Reset character's applied gravity
	public void ResetGravity(){
		// If the character is going in the same direction as gravity is pushing him to
		if(Vector3.Project(_conservedVelocity,_gravity.normalized).y < 0){
			// Substract the momuntum in that direction
			_conservedVelocity -= Vector3.Project(_conservedVelocity,_gravity.normalized);
		}
	}

	public void ResetImpulses(){
		_impulses.Clear();
	}

	// Return if a contact is connected to the ground
	public bool CheckContactWithGround(ContactPoint contact){
		return Vector3.Project(contact.normal,_gravity.normalized).y > 0.5f;
	}

	// Return if a contact is connected to the ground
	public bool CheckContactWithGround(ContactPoint2D contact){
		return contact.normal.y < 0.5f;
	}
}
