using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]

public class UnitSystem : MonoBehaviour {

    /* DEBUG */
    
	// [Debug] If set to false, allow the character to ignore gravity
	public bool _debug_useGravity = true;

    /* SYSTEM */

    // Age system
    [Header("Age system")]
    [Range(0, 1)]
    public float _age = 0;
    public float _agingSpeed = 0.01f;

    // Birth system
    [Header("Child system")]
    // [HideInInspector]
    public byte _childCount = 0;
    bool _generatedChilds = false;

    // Order system
    public enum OrderState {
        IDLE,
        INITIALISING,
        EXECUTING,
        FINALISING_BY_NODE,
        FINALISING_BY_COLLISION
    }

    [Header("Order system")]
    public OrderState _orderState;
    public float _minImpulse;
    public float _maxImpulse;
    public AnimationCurve _impulseOverLifetime;

    public float _currentMaxImpulse {
        get { return Mathf.Lerp(_minImpulse, _maxImpulse, _impulseOverLifetime.Evaluate(_age)); }
    }

    // OrderCorrection
    public Node _currentNode = null;
    public Node _lastNode = null;
    public float _distanceCorrectSpeed = 1f;
    public float _minimumCorrectSpeed = 1f;


	// Character physics system
	[HideInInspector]
	public CharacterPhysic _physic;

    /* SYSTEM */

	public bool RecieveDirectOrder(Order order) {
        // Check if the order is valide
        if (order == null || order._orderType == Order.OrderType.NULL) {
            Debug.Log("Error - " + this + " : RecieveDirectOrder() was called but the order given is invalide.");
            return false;
        }

        if (_currentNode != null && !_currentNode.AskExit(this)) {
            Debug.Log("Error - " + this + " : RecieveDirectOrder() was called but the unit couldn't exit it's current node.");
        }

        ExecuteOrder(order);
        return true;
    }

	public bool RecieveOrder(Order order) {
        // Check if the order is valide
        if (order == null || order._orderType == Order.OrderType.NULL) {
            Debug.Log("Error - " + this + " : LinkToBacktNode() not implemented yet.");
            return false;
        }

        ExecuteOrder(order);
        return true;
    }

	public void ExecuteOrder(Order order) {
        StopCoroutine("CorrectPosition");

        if (_currentNode != null)
            transform.position = _currentNode.transform.position;

		switch (order._orderType){
		    case Order.OrderType.IMPULSE :
                OrderImpulse orderImpulse = order as OrderImpulse;
                /*
			    _debug_useGravity = true;
			    _physic.AddImpulse(orderImpulse._impulse);
                */
                StartCoroutine(ExecuteImpulseOrder(orderImpulse));
                break;
		}
    }

    public void FixedAge() {
        _age = Mathf.MoveTowards(_age, 1, _agingSpeed * Time.fixedDeltaTime);
        
        /*
        // Unit have child if midlife
        if (_age >= 0.5f && !_generatedChilds) {
            _childCount = (byte)Random.Range(1, 4);
            _generatedChilds = true;
        }
        */

        if (_age >= 1) {
            Die();
        }
    }

    public void Die() {
        if (_currentNode != null) {
            _currentNode.AskExit(this);
        }
        Destroy(gameObject);
    }

    /* COOROUTINES */

    IEnumerator ExecuteImpulseOrder(OrderImpulse order)
    {
        // Start order execution

        // Initialise oder
        _orderState = OrderState.INITIALISING;

        // Execute order
        _debug_useGravity = true;
        _physic.AddImpulse(order._impulse);

        // Wait until order is executed
        _orderState = OrderState.EXECUTING;

        // Wait for a base delay
        float delayMin = 0.1f;
        while (delayMin > 0 && _orderState != OrderState.FINALISING_BY_NODE) {
            delayMin -= Time.deltaTime;
            yield return null;
        }

        // If not stop on node during base delay
        if(_orderState != OrderState.FINALISING_BY_NODE) {
            // Wait for the unit to stop
            _orderState = OrderState.EXECUTING;
            while (!(_orderState == OrderState.FINALISING_BY_COLLISION || _orderState == OrderState.FINALISING_BY_NODE)) {
                yield return null;
            }
        }

        // Reset gravity
        _physic.ResetGravity();

        // Reset all impulses
        _physic.ResetImpulses();

        // Stop applying gravity
        _debug_useGravity = false;

        if (order._endWorldPos == Vector3.zero) {
            order._endWorldPos = transform.position;
        }

        // Open a new path if the travel do not finish on an already existing node
        if(_currentNode == null && _lastNode != null) {
            _lastNode.AddFrontNode(this, order);
        } else {
            StartCoroutine("CorrectPosition");
        }

        // End order execution
        _orderState = OrderState.IDLE;
        yield return null;
    }

    IEnumerator CorrectPosition() {
        // Correction max delay
        float delayMax = 0.5f;
        while (_currentNode != null && (delayMax >= 0 || transform.position == _currentNode.transform.position)) {

            if (_orderState == OrderState.EXECUTING)
                break;

            delayMax -= Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _currentNode.transform.position, (Vector3.Distance(transform.position, _currentNode.transform.position) * _distanceCorrectSpeed + _minimumCorrectSpeed) * Time.fixedDeltaTime);
            yield return null;
        }
        // After that delay correct the position
        if(_currentNode != null && _orderState != OrderState.EXECUTING)
            transform.position = _currentNode.transform.position;
        yield return null;
    }

    /* UNITY */

    Collider2D _collider;

    void Awake() {
        Physics2D.IgnoreLayerCollision(8, 8);

        // Set the character's rigidbody reference
        _physic._rigidbody = this.gameObject.GetComponent<Rigidbody2D>();

        _collider = gameObject.GetComponent<Collider2D>();


        /* Sign & Feedback */
        _feedback_ageFeedbackRangeTransform = transform.Find("AgeRangeFeedback");
        _feedback_ageFeedbackRangeSpriteRenderer = _feedback_ageFeedbackRangeTransform.GetComponent<SpriteRenderer>();
        _feedback_ageSpriteRenderer = transform.Find("AgeFeedback").GetComponent<SpriteRenderer>();

    }

    void Update() {
        // Update overlaping depending of age
        Vector3 newPosition = transform.position;
        newPosition.z = -_age;
        transform.position = newPosition;

        // Sign & Feedback
        Feedback_UpdateAgeFeedback();
    }


    /* PHYSIC */

    void FixedUpdate() {
        // Make the unite age
        FixedAge();

        // Stop physic
        _physic._rigidbody.velocity = Vector3.zero;
        bool applyPhysic = false;
        // _physic._rigidbody.isKinematic = true;

        // Apply physic only if not IDLE
        if (_orderState != OrderState.IDLE) {
            applyPhysic = true;
            _physic._rigidbody.isKinematic = false;
        }

        // Physic
        if (applyPhysic) {
            // Apply physic

            // Apply gravity to the conserved velocity
            if (_debug_useGravity) _physic.ApplyGravity();

            // Create new velocity based on the conserved velocity
            Vector3 newVelocity = _physic._conservedVelocity;

            foreach (Vector3 impulse in _physic._impulses) {
                newVelocity += impulse;
            }

            // Apply new velocity
            _physic._rigidbody.velocity = newVelocity;
        }
	}

	void OnCollisionEnter2D(Collision2D collision){
        // Debug.Log("Collision stay");
        _physic._timeAtLastColision = Time.fixedTime;
        if (_orderState == OrderState.EXECUTING)
            _orderState = OrderState.FINALISING_BY_COLLISION;
        _physic.ResetGravity();
	}

	void OnCollisionStay2D(Collision2D collision){
        // Debug.Log("Collision stay");
        _physic._timeAtLastColision = Time.fixedTime;
        if (_orderState == OrderState.EXECUTING)
            _orderState = OrderState.FINALISING_BY_COLLISION;
        _physic.ResetGravity();
	}

	void OnCollisionExit2D(Collision2D collision){
		// Set last collision (used to detect if on ground)
		_physic._lastCollision = collision;
	}

    public bool IsReadyToExecuteOrder() {
        return _orderState == OrderState.IDLE;
    }


    /* SIGN & FEEDBACK */


    // Age

    Transform _feedback_ageFeedbackRangeTransform;
    SpriteRenderer _feedback_ageFeedbackRangeSpriteRenderer;
    SpriteRenderer _feedback_ageSpriteRenderer;

    [Header("Sign & Feedback")]
    public float _feedback_ageSizeMin = 0.75f;
    public float _feedback_ageSizeMax = 1.25f;
    public AnimationCurve _feedback_ageSizeCurve;
    public Gradient _feedback_ageGradient;
    
    void Feedback_UpdateAgeFeedback() {
        // Update size
        float newSize = _feedback_ageSizeMin + (_feedback_ageSizeCurve.Evaluate(_age) * (_feedback_ageSizeMax - _feedback_ageSizeMin));
        _feedback_ageFeedbackRangeTransform.localScale = Vector3.one * newSize;

        // Update color
        _feedback_ageFeedbackRangeSpriteRenderer.color = _feedback_ageGradient.Evaluate(_age);

        _feedback_ageSpriteRenderer.color = _feedback_ageGradient.Evaluate(_age);
    }

    public Color Feedback_GetAgeColor() {
        return _feedback_ageGradient.Evaluate(_age);
    }

    public float Feedback_GetAgeSize() {
        return _feedback_ageSizeMin + (_feedback_ageSizeCurve.Evaluate(_age) * (_feedback_ageSizeMax - _feedback_ageSizeMin));
    }

}
