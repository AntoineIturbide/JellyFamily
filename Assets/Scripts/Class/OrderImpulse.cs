using UnityEngine;
using System.Collections;

[System.Serializable]
public class OrderImpulse : Order {

    // Start impulse
	public Vector2 _impulse = Vector2.zero;

    // End velocity
    public Vector3 _endWorldPos = Vector2.zero;

    // Impulse order constructor
    public OrderImpulse(Vector2 impulseValue) {
		// Set order type
		_orderType = OrderType.IMPULSE;
		// Set impulse
		_impulse = impulseValue;
	} 

}
