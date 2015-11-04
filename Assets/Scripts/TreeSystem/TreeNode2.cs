using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeNode2 : MonoBehaviour {

    public static bool _stopGrowing = false;

    public TreeNode2 _origine;
    public List<TreeNode2> _branchingNodes = new List<TreeNode2>();

    [Header("Duration")]
    public float _prewarmDuration = 1f;
    public float _growinDuration = 1f;

    public float _size = 0f;
    public byte _rank = 10;
    float _sizeByRank = 0.1f;

    [HideInInspector]
    public Vector3 _direction;

    public enum State {
        PREWARMING,
        PREWARM_FINALIZED,
        GROWING,
        GROWING_FINALIZED
    }
    public State _state = State.GROWING_FINALIZED;
    public float _stateProgres = 0f;

    public bool _askGrow = false;


    public TreeNode2(Vector3 position, TreeNode2 origine, byte rank) {
        transform.position = position;
        _state = State.PREWARMING;
        _stateProgres = 0f;
        _origine = origine;
        _rank = rank;
        _size = 0;

        // Set feedback
        SetRank();
        SetBranch();
    }

    public void Set(Vector3 position, TreeNode2 origine, byte rank) {
        transform.position = position;
        _state = State.PREWARMING;
        _stateProgres = 0f;
        _origine = origine;
        _rank = rank;
        _size = 0;
        _sizeByRank = 0.1f;
        _direction = position - origine.transform.position;
        _direction.z = 0;
        _direction.Normalize();

        // Set feedback
        SetRank();
        SetBranch();
    }

    public void Grow() {
        Debug.Log("Grow" + this);
        if (_state == State.GROWING_FINALIZED) {
            _size = Mathf.MoveTowards(_size, ((float)_rank) * _sizeByRank, _sizeByRank);
            Debug.Log(((float)_rank) * _sizeByRank);
            if (_branchingNodes.Count <= 0) {
                MakeBranchs();
            } else if (_rank > 2) {
                foreach (TreeNode2 branch in _branchingNodes) {
                    branch._askGrow = true;
                }
            } else {
                _stopGrowing = true;
            }
        }
    }

    public void MakeBranchs() {
        int newBrachRank = _rank - 1;
        if (newBrachRank < 1)
            return;

        for (int i = 0; i < 3; i++) {
            AddBranch((byte)newBrachRank);
        }
    }

    public void AddBranch(byte rank) {
        Vector3 position = transform.position + (Quaternion.Euler(Vector3.forward * Random.Range(-45, 45)) * _direction * Random.Range(5f, 10f));
        /*
        Vector3 position = transform.position + new Vector3(
            Random.Range(-5f, 5f),
            Random.Range(2f, 5f),
            0
            );
            */
        GameObject newBranch = Instantiate(TreeSystem2._debug_staticTreeNodePrefab);
        newBranch.transform.position = position;
        TreeNode2 newBranchNode = newBranch.GetComponent<TreeNode2>();
        newBranchNode.Set(position, this, rank);
        _branchingNodes.Add(newBranchNode);
    }

    /* Unity */

    void Awake() {
        // Retrieve ref
        _feedback_branchLineRenderer = gameObject.GetComponent<LineRenderer>();
        // Set feedback
        SetRank();
        SetBranch();
    }

    void Update() {
        // Update state
        switch (_state) {
            case State.PREWARMING:
                _stateProgres = Mathf.MoveTowards(_stateProgres,_prewarmDuration,Time.deltaTime);
                if (_stateProgres == _prewarmDuration) {
                    _state = State.PREWARM_FINALIZED;
                    _stateProgres = 0f;
                }
                break;
            case State.PREWARM_FINALIZED:
                if (_askGrow) {
                    _askGrow = false;
                    _state = State.GROWING;
                }
                break;
            case State.GROWING:
                _stateProgres = Mathf.MoveTowards(_stateProgres, _growinDuration, Time.deltaTime);
                if (_stateProgres == _growinDuration) {
                    _state = State.GROWING_FINALIZED;
                    _stateProgres = 0f;
                    Grow();
                }
                break;;
            case State.GROWING_FINALIZED:
                //
                if (_askGrow) {
                    _askGrow = false;
                    Grow();
                }
                break;
        }

        // Update feedback
        UpdateBranch();


        // Debug
        Debug.DrawRay(transform.position, _direction, Color.red);
        // Debug.DrawRay(transform.position, (Quaternion.Euler(Vector3.forward * Random.Range(-45, 45)) * _direction * Random.Range(2f, 5f)));
    }


    /* FEEDBACK */
    LineRenderer _feedback_branchLineRenderer;
    Vector3 _feedback_rankSize;
    Vector3 _feedback_growingPosition;
    float _feedback_growingWidth;
    float _feedback_positionSpeed;
    float _feedback_scaleSpeed;

    void SetRank() {
        _size = 0.1f;
        _feedback_rankSize = Vector3.one * _size;
    }

    void SetBranch() {
        if (_origine != null) {
            // Calculate speeds
            _feedback_positionSpeed = Vector3.Distance(_origine.transform.position, transform.position) / _prewarmDuration;
            _feedback_scaleSpeed = (1-0) / _prewarmDuration;
            // Set size
            transform.localScale = Vector3.zero;
            _feedback_growingWidth = 0;
            _feedback_branchLineRenderer.SetWidth(_origine.transform.localScale.x, _feedback_growingWidth);
            // Set position
            _feedback_growingPosition = _origine.transform.position;
            _feedback_branchLineRenderer.SetPosition(0, _feedback_growingPosition);
            _feedback_branchLineRenderer.SetPosition(1, _feedback_growingPosition);
        }
    }

    void UpdateBranch() {
        if(_origine != null) {
            switch (_state) {
                case State.PREWARMING:
                    // Update position
                    _feedback_growingPosition = Vector3.MoveTowards(_feedback_growingPosition, transform.position, _feedback_positionSpeed * Time.deltaTime);
                    _feedback_branchLineRenderer.SetPosition(1, _feedback_growingPosition);
                    break;
                case State.GROWING:
                    goto case State.GROWING_FINALIZED;
                case State.GROWING_FINALIZED:
                    // Update Size 
                    transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * _size, _feedback_scaleSpeed * Time.deltaTime);
                    break;
            }

            float startWidth = _origine.transform.localScale.x;
            // float endWidth = _feedback_growingWidth = Mathf.MoveTowards(_feedback_growingWidth, transform.localScale.x, _feedback_scaleSpeed * Time.deltaTime);
            float endWidth = transform.localScale.x;
            _feedback_branchLineRenderer.SetWidth(startWidth, endWidth);
        }
    }
}
