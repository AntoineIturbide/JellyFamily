using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeNode : MonoBehaviour {
    
    public GameObject _debug_prisePrefab;
    
    BranchNode _startNode = new BranchNode(Vector3.zero,10);

    void Start() {
        BranchNode._debug_prisePrefab = _debug_prisePrefab;
        _startNode = new BranchNode(transform.position, 10);
        _startNode.ResetRays();
        StartCoroutine(Grow(_startNode));
    }

    void Update() {
        _startNode.DisplayBranchs();
    }
    
    IEnumerator Grow(BranchNode node) {
        BranchNode._debug_currentIteration = 0;
        float delay = 0.5f;
        float time = delay;
        while (BranchNode._debug_currentIteration < BranchNode._debug_maxIteration) {
            time = Mathf.MoveTowards(time, 0, Time.fixedDeltaTime);
            if (time <= 0) {
                time = delay;
                BranchNode._debug_currentIteration ++;
                Debug.Log(BranchNode._debug_currentIteration + " : " + BranchNode._debug_maxIteration);
                node.Grow();
            }
            yield return null;
        }
    }

}
