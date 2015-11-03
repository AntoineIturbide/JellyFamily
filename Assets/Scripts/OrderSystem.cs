using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrderSystem : MonoBehaviour {

    Transform _durationTransform;

    public CharacterSystem _originUnit;
    public CharacterSystem _targetUnit;
    public Vector3 _targetPosition;
    public bool _integrateUnitFound = false;


    // Order
    OrderImpulse _order;
    public float _duration = 1f;
    public float _progress = 1f;

    // Destroy
    public float _destructionDelay = 1;

    // Sates
    public enum States {
        WAITING_ORIGIN,
        WAITING_TARGET,
        TRAVELING,
        DESTROYING
    }

    public States _state = States.WAITING_ORIGIN;

    /* FEEDBACKS */

    // Range
    SpriteRenderer _rangeSprite;

    // Particle
    public struct OrderParticle {
        Transform paticle;
        // Traveling system
        List<Vector3> wayPoints;
        int currentWaypointId;
        float travelSpeed;

        public OrderParticle(Transform paticleTransform) {
            paticle = paticleTransform;
            wayPoints = new List<Vector3>();
            currentWaypointId = 0;
            travelSpeed = 0;
        }

        // Call on update to make the paticle travel
        public void Travel() {
            if (wayPoints.Count > 0) {
                // Retrieve current position
                Vector3 newPos = paticle.position;
                // Edit the position by making it traveling toward the current waypoint
                newPos = Vector3.MoveTowards(
                    // From current position
                    newPos,
                    // Toward current waypoint
                    wayPoints[Mathf.Clamp(currentWaypointId,0, wayPoints.Count-1)],
                    // With the travel speed
                    travelSpeed * Time.deltaTime
                    );
                // Apply the new position
                paticle.position = newPos;

                if (newPos == wayPoints[Mathf.Clamp(currentWaypointId, 0, wayPoints.Count-1)]) {
                    currentWaypointId = Mathf.Clamp(currentWaypointId + 1, 0, wayPoints.Count-1);
                }
            }
        }

        // Recalculate 
        public void RecalculateTravelSpeed(float duration) {
            float travelTotalLengh = 0f;
            if (wayPoints.Count >= 2) {
                travelTotalLengh += Vector3.Distance(paticle.position, wayPoints[0]);
                for (int i= 0; i < wayPoints.Count-1; i++) {
                    travelTotalLengh += Vector3.Distance(wayPoints[i], wayPoints[i + 1]);
                }
            }
            travelSpeed = travelTotalLengh / duration;
        }

        public void AddWayPoint(Vector3 newWaypoint) {
            // If this is the first waypoint, set is as the origine
            if(wayPoints.Count == 0) {
                paticle.position = newWaypoint;
            }
            wayPoints.Add(newWaypoint);
        }

        public void Activate() {
            paticle.gameObject.SetActive(true);
        }

        public void SetNewTarget(CharacterSystem newTarget) {
            wayPoints = new List<Vector3>();
            wayPoints.Add(newTarget.transform.position);
            currentWaypointId = 0;
        }
    }

    OrderParticle _particle;


    public void Set(OrderImpulse order, CharacterSystem originUnit, CharacterSystem targetUnit) {
        _order = order;
        _originUnit = originUnit;
        _targetUnit = targetUnit;
        _integrateUnitFound = false;

        if (originUnit == _targetUnit) {
            StartCoroutine(InitiateDestruction());
            return;
        }
    }

    public void Set(OrderImpulse order, CharacterSystem originUnit, Vector3 targetPosition) {
        _order = order;
        _originUnit = originUnit;
        _targetPosition = targetPosition;
        _integrateUnitFound = false;
    }

    void Awake() {
        // Retrieve range feedback sprite renderer
        _rangeSprite = gameObject.GetComponent<SpriteRenderer>();

        // Retrieve duration feedback's transform
        _durationTransform = transform.FindChild("Delay");

        // Retrieve paticle and configure it
        _particle = new OrderParticle(transform.FindChild("Particle"));
        _particle.RecalculateTravelSpeed(_duration);

        _state = States.WAITING_ORIGIN;

    }

    void Start() {
        //
    }

    void Update() {
        if (_state != States.DESTROYING) {
            _rangeSprite.color = new Color(
                _rangeSprite.color.r,
                _rangeSprite.color.g,
                _rangeSprite.color.b,
                Mathf.MoveTowards(_rangeSprite.color.a, 1, 2 * Time.deltaTime)
                );
        }

        // _debug_
        if (_originUnit == _targetUnit) {
            StartCoroutine(InitiateDestruction());
            return;
        }

        switch (_state) {
            case States.WAITING_TARGET:
                if (_targetUnit == null) {
                    StartTransmition();
                    break;
                } else {
                    if (_targetUnit._pendingOrder[0] == this) {
                        StartTransmition();
                    }
                }
                break;
            case States.TRAVELING:
                // Update progress
                _progress = Mathf.MoveTowards(_progress, 0, (1f / _duration) * Time.deltaTime);

                // Update duration feedback scale
                Vector3 newScale = _durationTransform.localScale;
                newScale = new Vector3(
                    Mathf.MoveTowards(newScale.x, 0, (1f / _duration) * Time.deltaTime),
                    Mathf.MoveTowards(newScale.y, 0, (1f / _duration) * Time.deltaTime),
                    Mathf.MoveTowards(newScale.z, 0, (1f / _duration) * Time.deltaTime)
                    );
                _durationTransform.localScale = newScale;

                // Update particle feedback
                _particle.Travel();

                if(_progress <= 0) {
                    if(_integrateUnitFound) {
                        _targetUnit.StoreOrder(_order);
                    } else {
                        SendOrder(_order, _targetUnit);
                    }
                    // Destroy transmiter
                    _state = States.DESTROYING;
                    StartCoroutine(InitiateDestruction());
                }
                break;
        }

    }

    //
    public void Initialise() {
        // Set state
        _state = States.WAITING_TARGET;
        
        // Add this OrderSystem to the unit's waiting list
        if (_targetUnit != null) _targetUnit._pendingOrder.Add(this);
    }

    public void StartTransmition() {
        _state = States.TRAVELING;

        // Activate the duration feedback
        _durationTransform.gameObject.SetActive(true);

        // Configure the particle feedback
        _particle.AddWayPoint(_originUnit.transform.position);

        if (_order != null && _order._endWorldPos != null && _order._endWorldPos != Vector3.zero)
            _particle.AddWayPoint(_order._endWorldPos);

        _particle.AddWayPoint(transform.position);

        if (_targetUnit != null)
            _particle.AddWayPoint(_targetUnit.transform.position);
        else
            _particle.AddWayPoint(_targetPosition);
        _particle.RecalculateTravelSpeed(_duration);

        // Activate the particle feedback
        _particle.Activate();
    }

    public IEnumerator InitiateDestruction() {
        float progress = 1;
        while (progress > 0) {
            // Update destruction progress
            progress = Mathf.MoveTowards(progress, 0, (1f/ _destructionDelay)* Time.deltaTime);

            // Apply feedback of the destruction over time
            _rangeSprite.color = new Color(
                _rangeSprite.color.r,
                _rangeSprite.color.g,
                _rangeSprite.color.b,
                Mathf.MoveTowards(_rangeSprite.color.a, 0, (1f/ _destructionDelay) * Time.deltaTime)
                );
            yield return null;
        }

        // Remove this order if it's still in the target pending list
        if (_targetUnit!=null && _targetUnit._pendingOrder.Contains(this)) {
            _targetUnit._pendingOrder.Remove(this);
        }

        // Destroy game object after the animation is finished
        Destroy(gameObject);
        yield return null;
    }

    void SendOrder(Order order, CharacterSystem targetUnit) {
        if (targetUnit != null) targetUnit.RecieveOrder(order,_originUnit);
    }

    void OnTriggerEnter2D (Collider2D collider) {
        if (collider.transform.tag == "unit") {
            if (
               _state == States.WAITING_ORIGIN
               || _state == States.WAITING_TARGET
               || _state == States.TRAVELING
                ) {
                
                // Retrieve unit to integrate
                CharacterSystem unitToIntegrate = collider.gameObject.GetComponent<CharacterSystem>();

                if (
                    unitToIntegrate != null
                    && (unitToIntegrate == _originUnit || unitToIntegrate == _targetUnit)
                    )
                    return;

                if (unitToIntegrate._frontUnits.Contains(_originUnit))
                    return;
                
                CharacterSystem.IntegratePathBehindUnit(
                    _originUnit,
                    unitToIntegrate
                    );

                // Remove this order from old target's waiting list
                if (_targetUnit != null)
                    _targetUnit._pendingOrder.Remove(this);

                // Set unit to integrate as new target
                _targetUnit = unitToIntegrate;

                _targetUnit.transform.position = transform.position;
                if (_targetUnit._currentOrder != null)
                    _targetUnit._orderCompleated = true;

                _integrateUnitFound = true;

                // _targetUnit._frontUnits.Add(_originUnit);

                // Add this order to the new target waiting list

                /*
                if (_targetUnit != null)
                    _targetUnit._pendingOrder.Add(this);
                */

                if (_state == States.TRAVELING) {
                    // Desactivate de delay feedback
                    _durationTransform.gameObject.SetActive(false);

                    // Recalculate the particle feedback
                    _particle.SetNewTarget(_targetUnit);

                    return;
                }
            }
        }
    }
}
