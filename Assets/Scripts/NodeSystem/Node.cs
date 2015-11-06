using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class Node : MonoBehaviour {

    /* SYSTEM */

    // Tree system
    public TreeNode2 _treeNode;

    // Connexion sytem
    public struct Connexion {
        // Connected node
        public Node _connectedNode;
        // Path to this node
        public Order _path;
        // Fixed time at the creation of this connexion
        public float _creationTime;
        // Maximum age to take this path
        public float _age;

        public Connexion(Node connectedNode, Order path, float age) {
            _connectedNode = connectedNode;
            _path = path;
            _creationTime = Time.fixedTime;
            _age = age;
        }
    }

    // Lists of nodes that this node is connected to,
    // With a connection going from this node toward the front node
    public List<Connexion> _frontNodes = new List<Connexion>();
    // With a connection going from the back node toward this node
    public List<Connexion> _backNodes = new List<Connexion>();
    [Header("Path age")]
    public float pathAgeTolerance = 0.1f;

    [Header("Overlaping correction")]
    // Range within nodes will be considerated as overlapping
    public float _overlapingNodeRange = 0.5f;

    [Header("Node life")]
    // Start and maximum lifetime of this node
    public float _lifetime = 5f;
    // Current remaining lifetime of this node
    public float _currentLifetime = 5f;
    // Current remaining lifetime of this node
    public bool _doAge = true;

    // Unit on node system
    [Header("Unit on node system")]
    // Unit on this node
    public UnitSystem _unitOnNode;
    // Relevant connexion that this unit is waiting to travel
    public Connexion _relevantConnexion;
    bool _relevantConnexionFound = false;

    // Add a front node
    public bool AddFrontNode(UnitSystem unit, Order pathToThisNode) {

        if (unit == null) {
            // Return that it couldn't succesfully link the nodes
            Debug.Log("Error - " + this + " : LinkToFrontNode() called with no unit to start new node generation.");
            return false;
        }

        // Check if the path to the new node realy start from this node
        if(unit._lastNode != this) {
            // Return that it couldn't succesfully link the nodes
            Debug.Log("Error - " + this + " : LinkToFrontNode() called but the last node of the base unit isn't this node.");
            return false;
        }
        
        // Check if the path to the new is valide
        if (pathToThisNode._orderType == Order.OrderType.NULL) {
            // Return that it couldn't succesfully link the nodes
            Debug.Log("Error - " + this + " : LinkToFrontNode() called with an invalide order.");
            return false;
        }

        // Generate the new node game object
        GameObject newNodeGameObject = Instantiate(_nodePrefab);

        // Parameter the new node game object
        newNodeGameObject.name = "Node";
        newNodeGameObject.transform.parent = gameObject.transform.parent;
        Vector3 spawnPosition = unit.transform.position;
        spawnPosition.z = 0;
        newNodeGameObject.transform.position = spawnPosition;

        // Get the node system from the generated game object
        Node newNode = newNodeGameObject.GetComponent<Node>();

        // Put the unit on the new node
        newNode._unitOnNode = unit;

        // Set the current node on unit
        unit._currentNode = newNode;

        // Add a connection to this node
        _frontNodes.Add(new Connexion(newNode, pathToThisNode, unit._age));

        // Realculate relevant destination of the unit on this node
        if (IsOccupied()) {
            _relevantConnexionFound = CalculateRelevantConnexion(ref _relevantConnexion);
            if (!_relevantConnexionFound) {
                // No relevant destination has been found
            } else {
                // A relevant destination has been found
            }
        }

        // Sign & Feedback
        newNode.SetLinkFeedback(this, unit._age);

        return false;
    }

    // Add a back node
    bool LinkToBacktNode(Node newFrontNode) {

        // _debug_ Not implemented
        Debug.Log("Debug - " + this + " : LinkToBacktNode() called but wasn't implemented yet.");

        return false;
    }

    // Recieve a unit as the unit on this node
    bool RecieveUnit(UnitSystem unit) {
        // Check if there is a unit to recieve
        if (unit == null) {
            // Return that it couldn't succesfully retrieve the unit
            Debug.Log("Error - " + this + " : RecieveUnit() called with no unit to recieve.");
            return false;
        }

        // Check if there isn't already any unit on this node
        if (_unitOnNode != null) {
            // Return that it couldn't succesfully recieve the unit
            Debug.Log("Error - " + this + " : RecieveUnit() called but a unit is already asociated with this node.");
            return false;
        }

        // Store the unit into this node
        _unitOnNode = unit;

        // Calculate relevant destination
        _relevantConnexionFound = CalculateRelevantConnexion(ref _relevantConnexion);
        if (!_relevantConnexionFound) {
            // No relevant destination has been found
        } else {
            // A relevant destination has been found
        }

        if(_treeNode != null) {
            _treeNode._askGrow = true;
        }

        // Return that it succesfully recieved the unit
        return true;
    }

    // Transmit the unit on this node toward the relevant connexion's node
    bool SendUnit(Connexion relevantConnexion) {
        // Check if there is a unit to send
        if (_unitOnNode == null) {
            // Otherwise,
            // Return that it couldn't succesfully send the inexisting unit
            Debug.Log("Error - " +this +" : SendUnit() called with no unit to send.");
            return false;
        }

        // Check if the unit can start traveling to the relevant node
        if (!_unitOnNode.IsReadyToExecuteOrder()) {
            // Otherwise,
            // Return that it couldn't succesfully send the inexisting unit
            Debug.Log("Error - " + this + " : SendUnit() called but the unit isnt't ready to travel to it's new destination.");
            return false;
        }

        // Transmit the unit on this node to the target node system
        if (!relevantConnexion._connectedNode.RecieveUnit(_unitOnNode)) {
            // If the target node couldn't recive the unit,
            // Return that it couldn't succesfully send the unit
            Debug.Log("Error - " + this + " : SendUnit() called but couldn't successfully transmit the unit to it's destination node.");
            return false;
        }

        // Make the unit travel to it's relevant node
        _unitOnNode.RecieveOrder(relevantConnexion._path);


        // Set the target node on unit
        _unitOnNode._currentNode = relevantConnexion._connectedNode;

        // Set the last node on unit
        _unitOnNode._lastNode = this;

        // Check if the unit has a child
        if (_unitOnNode._childCount > 0) {
            // If it does, spawn it on this node
            _unitOnNode._childCount--;
            _unitOnNode = SpawnUnitOnNode();
            // Calculate relevant destination
            _relevantConnexionFound = CalculateRelevantConnexion(ref _relevantConnexion);
        } else {
            // Otherwise, remove the stored unit from this node if it was sucessfully sent
            _unitOnNode = null;
            // Reset relevant connexion
            _relevantConnexionFound = false;
        }

        // Return that it succesfully send the unit
        return true;
    }

    // Try to retrieve a unit that triggered this node's collider
    bool RetrieveUnit(UnitSystem unit) {

        // Check if there is a unit to retrieve
        if (unit == null) {
            // Return that it couldn't succesfully retrieve the unit
            Debug.Log("Error - " + this + " : RetrieveUnit() called with no unit to retrieve.");
            return false;
        }

        // Check if the unit isn't already trying to reach another node
        if (unit._currentNode != null) {
            // Return that it couldn't succesfully retrieve the unit
            return false;
        }

        // Check if not trying to retrieve a unit just expulsed
        if (unit._lastNode == this) {
            // Return that it couldn't succesfully retrieve the unit
            return false;
        }

        // Check if not trying to retrieve a unit just expulsed by a node overlapping this one
        if (unit._lastNode != null && Vector3.Distance(transform.position,unit._lastNode.transform.position) < _overlapingNodeRange) {
            // Return that it couldn't succesfully retrieve the unit
            return false;
        }

        // Check if there isn't already any unit on this node
        if (_unitOnNode != null) {
            // Return that it couldn't succesfully retrieve the unit
            return false;
        }

        // Recive unit
        if(!RecieveUnit(unit)) {
            // Return that it couldn't succesfully retrieve the unit
            Debug.Log("Error - " + this + " : RetrieveUnit() called but couldnt recieve the retrieved unit.");
            return false;
        }

        unit._currentNode = this;
        unit._orderState = UnitSystem.OrderState.FINALISING_BY_NODE;

        // Return that the unit was succesfully retrieved
        return true;
    }

    // The unit on this node ask for exiting this node
    public bool AskExit(UnitSystem unit) {
        // Check if there is the unit asking for exit correspond to the unit on this node
        if (unit != _unitOnNode) {
            // Return that it couldn't succesfully exit the unit on this node
            Debug.Log("Error - " + this + " : AskExit() called but unit asking for exit isn't on this node.");
            return false;
        }

        // Set the current node on unit
        unit._currentNode = null;

        // Set the last node on unit
        unit._lastNode = this;

        // Check if the unit has a child
        if (_unitOnNode._childCount > 0) {
            // If it does, spawn it on this node
            _unitOnNode._childCount--;
            _unitOnNode = SpawnUnitOnNode();
            // Calculate relevant destination
            _relevantConnexionFound = CalculateRelevantConnexion(ref _relevantConnexion);
        } else {
            // Otherwise, remove the stored unit from this node if it was sucessfully sent
            _unitOnNode = null;
            // Reset relevant connexion
            _relevantConnexionFound = false;
        }

        return true;
    }

    // 
    UnitSystem SpawnUnitOnNode() {
        GameObject childUnit = Instantiate(_unitPrefab);
        childUnit.transform.parent = _unitOnNode.transform.parent;
        childUnit.transform.position = _unitOnNode.transform.position;
        UnitSystem childUnitSystem = childUnit.GetComponent<UnitSystem>();
        childUnitSystem._currentNode = this;
        childUnitSystem._age = 0;
        return childUnitSystem;
    }


    /* UNITY */

    [Header("References")]
    // Node prefab
    public GameObject _nodePrefab;
    // Unit prefab
    public GameObject _unitPrefab;

    void Awake() {
        _feedback_LifetimeSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _feedback_linkFeedbackLineRenderer = gameObject.GetComponent<LineRenderer>();
        _feedback_lineParticle = transform.GetComponentInChildren<ParticleSystem>();
        _unitPrefab  = (GameObject)Resources.Load("Unit", typeof(GameObject));
    }

    void Start() {
        _currentLifetime = _lifetime;
    }

    void Update() {
        // Upate lifetime
        if ( _doAge && !IsOccupied()) {
            _currentLifetime = Mathf.MoveTowards(_currentLifetime, 0, Time.deltaTime);
            if (_currentLifetime <= 0) {
                Destroy(gameObject);
                return;
            }
        } else {
            _currentLifetime = _lifetime;
        }

        // Sign & Feedback
        UpdateLifetimeSign();
        UpdateLinkFeedback();
    }

    void FixedUpdate() {
        // Try to make the unit on this node progress
        if (
            // In the case where this node is occupied by a unit,
            IsOccupied()
            // That a relevant connexion to a node has been found for this unit,
            && _relevantConnexionFound
            // That that relevant node isn't already occupied
            && !_relevantConnexion._connectedNode.IsOccupied()
            // And that the unit is ready to travel to this node
            && _unitOnNode.IsReadyToExecuteOrder()
            ) {
            // Send this unit to this relevant node
            SendUnit(_relevantConnexion);
        }
    }


    /* UNITY TRIGGER */
    void OnTriggerEnter2D(Collider2D collider) {
        // If this is a unit, try to retrieve it
        if(collider.gameObject.tag == "unit") {
            UnitSystem unitToRetrieve = collider.gameObject.GetComponent<UnitSystem>();
            if (unitToRetrieve != null) {
                RetrieveUnit(collider.gameObject.GetComponent<UnitSystem>());
            }
        }

    }

    void OnTriggerStay2D(Collider2D collider) {
        /*

        // If this is a unit, try to retrieve it
        if (collider.gameObject.tag == "unit") {
            RetrieveUnit(collider.gameObject.GetComponent<UnitSystem>());
        }

        */
    }


    /* GET & SET */

    // Get if this node is occupied
    public bool IsOccupied() {
        return _unitOnNode != null;
    }

    bool CalculateRelevantConnexion(ref Connexion relevantConnexion) {

        // Check if a unit is on this node
        if (_unitOnNode == null) {
            // Otherwise, return that it couldn't succesfully calculate the most relevant connexion
            Debug.Log("Error - " + this + " : SortFrontNodesByRelevancy() called but a unit on this node is needed to sort nodes by relevancy.");
            return false;
        }

        // Check if the front node list isn't empty
        if (_frontNodes.Count <= 0) {
            // Otherwise, return that it couldn't succesfully calculate the most relevant connexion
            Debug.Log("Debug - " + this + " : SortFrontNodesByRelevancy() : _frontNodes.Count <= 0");
            return false;
        }

        // Select the only connexion if there is only one contained in the front node list
        if(_frontNodes.Count == 1) {
            // _debug_ Check if the the connexion is viable
            if (_frontNodes[0]._connectedNode != null && _unitOnNode._age <= _frontNodes[0]._age + pathAgeTolerance) {
                relevantConnexion = _frontNodes[0];
                return true;
            }
        } else {
            // Select the most recent and accesible one if there are more
            bool firstRelevantConnexionFound = false;
            foreach (Connexion connexion in _frontNodes) {
                // If no relevant connexion has already been found
                if (!firstRelevantConnexionFound){
                    // _debug_ Check if the the connexion is viable
                    if(connexion._connectedNode != null && _unitOnNode._age <= _frontNodes[0]._age + pathAgeTolerance) {
                        relevantConnexion = connexion;
                        firstRelevantConnexionFound = true;
                    }
                } else if (connexion._creationTime >= relevantConnexion._creationTime) {
                    // _debug_ Check if the the connexion is viable
                    if (connexion._connectedNode != null && _unitOnNode._age <= _frontNodes[0]._age + pathAgeTolerance)
                    {
                        relevantConnexion = connexion;
                    }
                }
            }
            if (firstRelevantConnexionFound)
                return true;
        }

        Debug.Log("Debug - " + this + " : SortFrontNodesByRelevancy() : firstRelevantConnexionFound = false");

        // Return that no relevant connexions have been found
        return false;
    }


    /* Sign & Feedback */
    [Header("Sign & Feedback")]

    // Node creator age feedback
    public Gradient _feedback_creatorUnitAgeColor;
    float _feedback_creatorUnitAge;

    // Node lifetime signe

    // Node point signe
    SpriteRenderer _feedback_LifetimeSpriteRenderer;

    // Link feedback
    LineRenderer _feedback_linkFeedbackLineRenderer;
    public ParticleSystem _feedback_lineParticle;
    public float _feedback_linkColorCorrectionSpeed = 1;
    Color _feedback_currentLineStartColor;
    Color _feedback_currentLineEndColor;
    Node _origineNode;

    void UpdateLifetimeSign() {
        _feedback_LifetimeSpriteRenderer.color = new Color (
                _feedback_LifetimeSpriteRenderer.color.r,
                _feedback_LifetimeSpriteRenderer.color.g,
                _feedback_LifetimeSpriteRenderer.color.b,
                Mathf.MoveTowards(_feedback_LifetimeSpriteRenderer.color.a, _currentLifetime / _lifetime, 5 * Time.deltaTime)
                );
    }

    void SetLinkFeedback(Node origineNode, float unitCreatorAge) {
        // Check if there is the line has a start
        if (origineNode == null) {
            return;
        }

        // Check if there is a line renderer
        if (_feedback_linkFeedbackLineRenderer == null) {
            return;
        }
        _origineNode = origineNode;

        _feedback_creatorUnitAge = unitCreatorAge;

        _feedback_linkFeedbackLineRenderer.SetPosition(0, _origineNode.transform.position);

        _feedback_linkFeedbackLineRenderer.SetPosition(1, this.transform.position);

        // Set line color
        Color _feedback_currentLineStartColor = _feedback_creatorUnitAgeColor.Evaluate(_feedback_creatorUnitAge);
        _feedback_currentLineStartColor.a = 0;
        Color _feedback_currentLineEndColor = Color.white;
        _feedback_currentLineEndColor.a = 0;

        // Apply color
        _feedback_linkFeedbackLineRenderer.SetColors(
            _feedback_currentLineStartColor,
            _feedback_currentLineEndColor
            );

        // Display colored line
        _feedback_linkFeedbackLineRenderer.enabled = true;

        // Configure particle system if there is one
        if (_feedback_lineParticle != null) {
            // Configure particle path
            Vector3 spawnPosition = _origineNode.transform.position;
            spawnPosition.z = 1f;
            _feedback_lineParticle.transform.position = spawnPosition;
            Vector3 spawnLookPosition = transform.position;
            spawnLookPosition.z = 1f;
            _feedback_lineParticle.transform.LookAt(spawnLookPosition);

            // Calculate angle
            Vector3 delta = origineNode.transform.position - transform.position;
            delta.z = 0;
            float angle;
            if (delta.y > 0)
                angle = Vector3.AngleBetween(-Vector3.right, delta);
            else
                angle = -Vector3.AngleBetween(-Vector3.right, delta);
            _feedback_lineParticle.startRotation = angle;

            // Calculate lifetime
            _feedback_lineParticle.startLifetime = Vector3.Distance(origineNode.transform.position, transform.position) / _feedback_lineParticle.startSpeed;

            // Calculate size
            _feedback_lineParticle.startSize = 0.25f + _feedback_creatorUnitAge * 0.25f;

            // Start playing particle
            _feedback_lineParticle.Play();
        }
    }

    void UpdateLinkFeedback() {
        if (_feedback_linkFeedbackLineRenderer == null) {
            return;
        }

        // Calculate target colors
        Color startColor = _feedback_creatorUnitAgeColor.Evaluate(_feedback_creatorUnitAge);
        Color endColor = Color.white;
        if (_origineNode != null) {
            float lifetime = _origineNode._currentLifetime / _origineNode._lifetime;
            float distance = Mathf.Clamp(Vector3.Distance(transform.position, _origineNode.transform.position) - 0.2f, 0f, 1f);
            startColor.a = lifetime * distance;
            endColor.a = lifetime * distance;
        } else {
            startColor.a = endColor.a = 0;
        }

        // Move toward those colors
        _feedback_currentLineStartColor = new Color(
            Mathf.MoveTowards(_feedback_currentLineStartColor.r, startColor.r, _feedback_linkColorCorrectionSpeed * Time.deltaTime),
            Mathf.MoveTowards(_feedback_currentLineStartColor.g, startColor.g, _feedback_linkColorCorrectionSpeed * Time.deltaTime),
            Mathf.MoveTowards(_feedback_currentLineStartColor.b, startColor.b, _feedback_linkColorCorrectionSpeed * Time.deltaTime),
            Mathf.MoveTowards(_feedback_currentLineStartColor.a, startColor.a, _feedback_linkColorCorrectionSpeed * Time.deltaTime)
            );

        _feedback_currentLineEndColor = new Color(
            Mathf.MoveTowards(_feedback_currentLineEndColor.r, endColor.r, _feedback_linkColorCorrectionSpeed * Time.deltaTime),
            Mathf.MoveTowards(_feedback_currentLineEndColor.g, endColor.g, _feedback_linkColorCorrectionSpeed * Time.deltaTime),
            Mathf.MoveTowards(_feedback_currentLineEndColor.b, endColor.b, _feedback_linkColorCorrectionSpeed * Time.deltaTime),
            Mathf.MoveTowards(_feedback_currentLineEndColor.a, endColor.a, _feedback_linkColorCorrectionSpeed * Time.deltaTime)
            );

        // Apply color
        _feedback_linkFeedbackLineRenderer.SetColors(
            _feedback_currentLineStartColor,
            _feedback_currentLineEndColor
            );

        // Calculate size
        _feedback_linkFeedbackLineRenderer.SetWidth(0.5f + _feedback_creatorUnitAge, 0.5f + _feedback_creatorUnitAge);

        // Particles

        // Stop particle emition if the origine node doesn't exist anymore
        if (_feedback_lineParticle != null) {
            if (_origineNode == null) {
                _feedback_lineParticle.Stop();
            } else {
                Color newParticleSpawnColor = Color.white;
                float distance = Mathf.Clamp(Vector3.Distance(transform.position, _origineNode.transform.position) - 0.5f, 0f, 1f);
                newParticleSpawnColor.a = (_origineNode._currentLifetime / _origineNode._lifetime) * distance;
                _feedback_lineParticle.startColor = newParticleSpawnColor;
            }
        }
    }

}
