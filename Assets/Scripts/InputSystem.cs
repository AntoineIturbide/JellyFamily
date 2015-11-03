using UnityEngine;
using System.Collections;

public class InputSystem : MonoBehaviour {

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

			UnitSystem startUnit = hit.collider.gameObject.GetComponent<UnitSystem>();
            if (startUnit == null)
                startUnit = hit.collider.transform.GetComponentInParent<UnitSystem>();
            _drag[0] = startUnit.transform.position;
            _drag[0].z = 1;
            Feedback_SetDragColor(startUnit);

            // Check if the start unit can execute an order
            if (!startUnit.IsReadyToExecuteOrder()) {
                // Otherwise, cancel order creation

                // Reset input feedback
                _drag = new Vector3[2] { Vector3.zero, Vector3.zero };
                yield break;
            }

            for (;;) {
                // Check if start unit is still aviable
                if (startUnit == null) {
                    // Otherwise, cancel order creation

                    // Reset input feedback
                    _drag = new Vector3[2] { Vector3.zero, Vector3.zero };
                    yield break;
                }
                //
                Feedback_UpdateDragColor(startUnit);

                // 
                _drag[1] = _debug_mainCamera.ScreenToWorldPoint(Input.mousePosition);
                _drag[1].z = _drag[0].z;
                _drag[1] = _drag[0] + (_drag[1] - _drag[0]).normalized * Mathf.Clamp(Vector3.Distance(_drag[1],_drag[0]), 0, startUnit._currentMaxImpulse * 0.5f);

                if (Input.GetMouseButtonUp(0)){
					hit = Physics2D.Raycast( new Vector2(_debug_mainCamera.ScreenToWorldPoint(Input.mousePosition).x, _debug_mainCamera.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);
					if (/*debug*/ false /* hit.collider != null && hit.collider.tag == "unit" */){
					// Link

					}
					else {
					// Impulse
						Vector3 dragEnd = _debug_mainCamera.ScreenToWorldPoint(Input.mousePosition);
                        dragEnd = startUnit.transform.position + (startUnit.transform.position - dragEnd).normalized * Mathf.Clamp(Vector3.Distance(startUnit.transform.position, dragEnd), startUnit._minImpulse, startUnit._currentMaxImpulse);
                        Vector3 dragDelta = Vector2.zero;
						Vector2 impulse = - new Vector2( (startUnit.transform.position - dragEnd).x,  (startUnit.transform.position - dragEnd).y);

                        // Check if the start unit can execute an order
                        if (!startUnit.IsReadyToExecuteOrder()) {
                            // Otherwise, cancel order creation

                            // Reset input feedback
                            _drag = new Vector3[2] { Vector3.zero, Vector3.zero };
                            yield break;
                        }

                        startUnit.RecieveDirectOrder(new OrderImpulse(impulse * _debug_speed));

                        // Debug.Log("Impulse sent"+ impulse);
                    }

                    // Reset input feedback
                    _drag = new Vector3[2] { Vector3.zero, Vector3.zero };
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

    /* SIGN & FEEDBACK */

    void Feedback_SetDragColor(UnitSystem startUnit) {
        // Color
        Color startColor = Color.white;
        startColor.a = 0f;
        Color endColor = startUnit.Feedback_GetAgeColor();
        _debug_lineDrag.SetColors(startColor, endColor);

        // Size
        _debug_lineDrag.SetWidth(startUnit.Feedback_GetAgeSize()*2f, 0);
    }

    void Feedback_UpdateDragColor(UnitSystem startUnit) {


        // Color
        Color startColor = Color.white;
        startColor.a = 0f;
        Color endColor = startUnit.Feedback_GetAgeColor();


        // Calculate alpha
        Vector3 startPosition = startUnit.transform.position;
        startPosition.z = 0;
        Vector3 endPosition = _debug_mainCamera.ScreenToWorldPoint(Input.mousePosition);
        endPosition.z = 0;
        float alpha = Mathf.Clamp(Vector3.Distance(startPosition, endPosition) - (startUnit.Feedback_GetAgeSize()), 0f, 1f);

        // Set alpha
        endColor.a = alpha;

        // Apply color
        _debug_lineDrag.SetColors(startColor, endColor);
    }

}
