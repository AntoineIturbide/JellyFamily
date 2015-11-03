using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterJump {

	/* ATTRIBUTES */

	// Jump stregh
	public float _jumpStrengh = 10f;

	// Possible number of in air jump(s)
	public int _jumpInAirCount = 0;

	// Remaining in air jump(s)
	[HideInInspector] public int _jumpInAirRemaining = 0;

}
