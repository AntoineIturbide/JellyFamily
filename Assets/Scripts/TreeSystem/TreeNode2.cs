using UnityEngine;
using System.Collections;

public class TreeNode2 : MonoBehaviour {

    public TreeNode2 _origine;
    public float _growingSpeed = 1;

    public enum State {
        GROWING,
        FINALIZED
    }
    public State _state = State.FINALIZED;

    public TreeNode2(Vector3 position, TreeNode2 origine) {
        transform.position = position;
        _state = State.GROWING;
        _origine = origine;

        // Set feedback
        SetBranch();
    }

    /* Unity */

    void Awake() {
        // Retrieve ref
        _feedback_branchLineRenderer = gameObject.GetComponent<LineRenderer>();
        // Set feedback
        SetBranch();
    }

    void Update() {
        // Update feedback
        UpdateBranch();
    }


    /* FEEDBACK */
    LineRenderer _feedback_branchLineRenderer;
    Vector3 _feedback_growingPosition;

    void SetBranch() {
        if (_origine != null) {
            _feedback_growingPosition = _origine.transform.position;
            _feedback_branchLineRenderer.SetPosition(0, _feedback_growingPosition);
            _feedback_branchLineRenderer.SetPosition(1, _feedback_growingPosition);
        }
    }

    void UpdateBranch() {
        if (_state == State.GROWING && _origine != null) {
            // Update Size 
            float startWidth = _origine.transform.localScale.x;
            float endWidth = transform.localScale.x;
            _feedback_branchLineRenderer.SetWidth(startWidth, endWidth);

            // Update position
            _feedback_growingPosition = Vector3.MoveTowards(_feedback_growingPosition, transform.position, _growingSpeed * Time.deltaTime);
            _feedback_branchLineRenderer.SetPosition(1, _feedback_growingPosition);
        }
    }

}
