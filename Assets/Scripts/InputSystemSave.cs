using UnityEngine;
using System.Collections;

public class InputSystemSave : MonoBehaviour {

	public Camera _debug_mainCamera;
	float _debug_cameraDragSpeed = 0.50f;
	public LineRenderer _debug_lineDrag;

	Vector3[] _drag = new Vector3[2] { Vector3.zero, Vector3.zero };
	public float _resetSpeed = 1;

	float _debug_speed = 2f;

	void Update(){
		// Get mouse
		if (Input.GetMouseButtonDown(0)){
			StartCoroutine(CreateMouseOrder());
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            Application.LoadLevel(Application.loadedLevel);
        }


        // Get touch
    }

	void OnGUI(){
		_debug_lineDrag.SetPosition(0,_drag[0]);
		_debug_lineDrag.SetPosition(1,_drag[1]);
	}

	IEnumerator CreateMouseOrder() {
		// Get order type
		RaycastHit2D hit = Physics2D.Raycast( new Vector2(_debug_mainCamera.ScreenToWorldPoint(Input.mousePosition).x, _debug_mainCamera.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);
		if (hit.collider != null && hit.collider.tag == "unit"){
			// Impulse or link
			// Debug.Log("Impulse/Link order started");

			CharacterSystem startUnit = hit.collider.gameObject.GetComponent<CharacterSystem>() ;
			for(;;){
				if (Input.GetMouseButtonUp(0)){
					hit = Physics2D.Raycast( new Vector2(_debug_mainCamera.ScreenToWorldPoint(Input.mousePosition).x, _debug_mainCamera.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);
					if (hit.collider != null && hit.collider.tag == "unit"){
					// Link

					}
					else {
					// Impulse
						Vector3 dragEnd = _debug_mainCamera.ScreenToWorldPoint(Input.mousePosition);
						Vector3 dragDelta = Vector2.zero;
						Vector2 impulse = new Vector2( (startUnit.transform.position - dragEnd).x,  (startUnit.transform.position - dragEnd).y);
						startUnit.RecieveDirectOrder(new OrderImpulse(impulse * _debug_speed));
						// Debug.Log("Impulse sent"+ impulse);
					}
					yield break;
				}
				yield return null;
			}
		}
		else {
			// Camera drag
			Vector3 dragStart = _debug_mainCamera.ScreenToWorldPoint(Input.mousePosition);
			Vector3 cameraStart = _debug_mainCamera.transform.position;
			// Debug.Log("Camera drag order started");
			for(;;){
				Vector3 dragEnd = _debug_mainCamera.ScreenToWorldPoint(Input.mousePosition);
				Vector3 dragDelta = dragStart - dragEnd;
				dragDelta.z = 0;
				_debug_mainCamera.transform.position = cameraStart + dragDelta * _debug_cameraDragSpeed;

				if (Input.GetMouseButtonUp(0)){
					yield break;
				}
				yield return null;
			}
			yield return null;
		}
	}

}
