using UnityEngine;
using System.Collections;

[System.Serializable]
public class Order {

    // Typo of order
	public enum OrderType {
		NULL,
		IMPULSE,
		CAMERA_DRAG
	}
	public OrderType _orderType = OrderType.NULL;

    // Travel time
    public float _travelTime = 1f;

    void Destroy(){
		_orderType = OrderType.NULL;
	}
}
