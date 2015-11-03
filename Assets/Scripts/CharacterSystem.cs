using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]

public class CharacterSystem : MonoBehaviour {

	// [Debgug] Character's rotation speed
	float _debug_rotationSpeed = 200;

	// [Debug] If set to false, allow the character to ignore gravity
	public bool _debug_useGravity = true;

	bool _boolean;


	/* ATTRIBUTES */

	// Character linked transform
	Transform _transform;

    [Header("Order system")]

    // Currently executing order
    public Order _currentOrder = new Order();
    // Storred order containing last action
    public Order _storredOrder = new Order();
    // List of order systems pending to transmit there order
    public List<OrderSystem> _pendingOrder = new List<OrderSystem>();
    // Order transmiter prefab used to transmit orders between units
    public GameObject _orderTransmiterPrefab;
    // Trigger when a unit end an order by reaching a static position
    public bool _orderCompleated = false;



    [Header("Correction system")]
    // Correction system
    public Vector3 _correctPosition;
    public float _correctSpeed = 1f;


    [Header("Linked units")]

    // Multiple units that are one order ahead of this unit
    public List<CharacterSystem> _frontUnits;

	// Single unit following this unit
	public CharacterSystem _backUnit;

	// Character physics system
	[HideInInspector]
	public CharacterPhysic _physic;

	/* METHODS */

	void Awake(){
		Physics2D.IgnoreLayerCollision(8,8);

		// Set the character's transform reference
		_transform = this.gameObject.transform;

		// Set the character's rigidbody reference
		_physic._rigidbody = this.gameObject.GetComponent<Rigidbody2D>();

        _correctPosition = transform.position;
    }

    void Update() {
        if(_currentOrder == null || _currentOrder._orderType == Order.OrderType.NULL) {
            _debug_useGravity = false;
            _physic.ResetImpulses();
            _physic.ResetGravity();
            transform.position = Vector3.MoveTowards(transform.position, _correctPosition, _correctSpeed * Time.deltaTime);
        }
    }

	public void RecieveDirectOrder(Order order) {
        // Check if the order is valide
        if (order == null || order._orderType == Order.OrderType.NULL) {
            if (_pendingOrder.Count > 0) {
                _pendingOrder.RemoveAt(0);
            }
            return;
        }

        OpenNewPath();
        ExecuteOrder(order);
	}

	public void RecieveOrder(Order order, CharacterSystem newFrontUnit) {
        // 
        /*
        if(!_frontUnits.Contains(newFrontUnit) && newFrontUnit != this)
            _frontUnits.Add(newFrontUnit);
            */

        // Check if the order is valide
        if (order == null || order._orderType == Order.OrderType.NULL) {
            if (_pendingOrder.Count > 0) {
                _pendingOrder.RemoveAt(0);
            }
            return;
        }
        ContinuePath(newFrontUnit);
        ExecuteOrder(order);
    }

	public void ExecuteOrder(Order order) {
        transform.position = _correctPosition;

        _currentOrder = order;
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

    public void StoreOrder(Order order) {
        _physic.ResetImpulses();
        _debug_useGravity = false;

        if (_pendingOrder.Count > 0)
            _pendingOrder.RemoveAt(0);

        _storredOrder = order;
    }

    /* COOROUTINES */

    IEnumerator ExecuteImpulseOrder(OrderImpulse order)
    {
        // Get origine position
        Vector3 startPos = transform.position;
        float startTime = Time.fixedTime;

        // Create an order transmiter
        GameObject orderTransmiter = Instantiate(_orderTransmiterPrefab);
        // Place the order transmiter in the scenne
        orderTransmiter.transform.position = startPos;
        // Activate it
        orderTransmiter.SetActive(true);

        // Configure order system
        OrderSystem orderSystem = orderTransmiter.GetComponent<OrderSystem>();
        if (_backUnit != null) {
            orderSystem.Set(_storredOrder as OrderImpulse, this, _backUnit);
        } else {
            orderSystem.Set(_storredOrder as OrderImpulse, this, transform.position);
        }

        // Execute order
        _debug_useGravity = true;
        _physic.AddImpulse(order._impulse);

        float delayMin = 0.1f;
        while (delayMin > 0) {
            delayMin -= Time.deltaTime;
            yield return null;
        }
        // Wait for the unit to stop to compleate the process
        _orderCompleated = false;
        while (!_orderCompleated) {
            yield return null;
        }

        // End impulse order
        EndCurrentImpulseOrder();
        _currentOrder._travelTime = Time.fixedTime - startTime;

        /*
        // Reconfigure order system
        if (_backUnit != null && !orderSystem._integrateUnitFound) {
            orderSystem.Set(_storredOrder as OrderImpulse, this, _backUnit);
        }
        */

        orderSystem.Initialise();


        // Reset current order
        _storredOrder = _currentOrder;
        _currentOrder = new Order();
        yield return null;
    }

    /* PHYSIC */

    void FixedUpdate() {
		// Apply gravity to the conserved velocity
		// [Debug] Can be desactivated with the _debug_useGravity boolean
		if(_debug_useGravity) _physic.ApplyGravity();

		// Create new velocity based on the conserved velocity
		Vector3 newVelocity = _physic._conservedVelocity;

		foreach(Vector3 impulse in _physic._impulses){
			newVelocity += impulse;
		}

		// Apply new velocity
		_physic._rigidbody.velocity = newVelocity;
	}

	void OnCollisionEnter2D(Collision2D collision){
        // Debug.Log("Collision stay");
        _physic._timeAtLastColision = Time.fixedTime;
        _orderCompleated = true;
        _physic.ResetGravity();
	}

	void OnCollisionStay2D(Collision2D collision){
        // Debug.Log("Collision stay");
        _physic._timeAtLastColision = Time.fixedTime;
        _orderCompleated = true;
        _physic.ResetGravity();
	}

	void OnCollisionExit2D(Collision2D collision){
		// Set last collision (used to detect if on ground)
		_physic._lastCollision = collision;
	}

	public void EndCurrentImpulseOrder() {
		_physic.ResetImpulses();
		_debug_useGravity = false;

        if (_pendingOrder.Count > 0)
            _pendingOrder.RemoveAt(0);

		if(_currentOrder._orderType == Order.OrderType.IMPULSE){
            OrderImpulse orderInpulse = _currentOrder as OrderImpulse;
			if( orderInpulse._endWorldPos != Vector3.zero) {
				_correctPosition = orderInpulse._endWorldPos;
			}
			else{
				orderInpulse._endWorldPos = transform.position;
                _correctPosition = orderInpulse._endWorldPos;
                _currentOrder = orderInpulse;
			}
		}
	}

    /* PATH */

    // When recieving player's order
    void OpenNewPath() {
        Debug.Log("OpenNewPath : " + this);
        if (_backUnit != null) {
            // Transmit folowing unit to the front units
            foreach (CharacterSystem frontUnit in _frontUnits) {
                if (frontUnit._backUnit == this)
                    frontUnit._backUnit = null;
                if (frontUnit != _backUnit)
                    frontUnit._backUnit = _backUnit;
            }

            _backUnit._pendingOrder = _pendingOrder;
            foreach (OrderSystem orderSystem in _backUnit._pendingOrder) {
                orderSystem._targetUnit = _backUnit;
            }

            _backUnit._frontUnits = _frontUnits;
            if (!_backUnit._frontUnits.Contains(this))
                _backUnit._frontUnits.Add(this);

        }

        _pendingOrder = new List<OrderSystem>();

        // Reset front units
        _frontUnits = new List<CharacterSystem>();
    }

    // When recieving other unit's order
    void ContinuePath(CharacterSystem trueFrontUnit) {
        Debug.Log("ContinuePath : " + this);
        // Transmit the new back unit to the front units
        Debug.Log("trueFrontUnit : " + trueFrontUnit);
        // Rearange the front unit list for the new back unit
        if (_backUnit != null) {
            /*
            foreach (CharacterSystem frontUnit in _backUnit._frontUnits) {
                frontUnit._backUnit = _backUnit;
            }*/

            _backUnit._frontUnits = _frontUnits;
            if (!_backUnit._frontUnits.Contains(this))
                _backUnit._frontUnits.Add(this);
        }
    }

    // When integrating a chain
    public static void IntegratePathBehindUnit(CharacterSystem frontUnit, CharacterSystem newBackUnit) {

        // from
        // frontUnit._backUnit => frontUnit
        // to
        // frontUnit._backUnit => newBackUnit => frontUnit
        
        if (frontUnit._backUnit == null) {
            // If the front unit has no back unit, add the new back unit as the front unit's back unit
            frontUnit._backUnit = newBackUnit;

            /*
            newBackUnit._frontUnits = new List<CharacterSystem>();
            newBackUnit._frontUnits.Add(frontUnit);
            */

        } else {

            if (newBackUnit._backUnit != frontUnit._backUnit) {
                newBackUnit._backUnit = frontUnit._backUnit;
                frontUnit._backUnit = newBackUnit;
            } else {
                newBackUnit._backUnit = null;
                frontUnit._backUnit = newBackUnit;
                return;
            }


            if (newBackUnit._backUnit._frontUnits.Contains(frontUnit)) {
                newBackUnit._backUnit._frontUnits.Remove(frontUnit);
            }

            newBackUnit._backUnit._frontUnits.Add(newBackUnit);

            // Reset front units
            newBackUnit._frontUnits = new List<CharacterSystem>();

            // Add the most recent order giver as unique front unit
            newBackUnit._frontUnits.Add(frontUnit);

            // Set order giver to the new unit
            foreach (OrderSystem orderSystem in newBackUnit._backUnit._pendingOrder) {
                orderSystem._originUnit = newBackUnit;
            }
        }
    }
}
