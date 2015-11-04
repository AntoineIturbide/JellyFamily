using UnityEngine;
using System.Collections;

public class TreeSystem2 : MonoBehaviour {

    public GameObject _debug_treeNodePrefab;
    public static GameObject _debug_staticTreeNodePrefab;

    TreeNode2 _startNode;

    Vector3 _debug_testRotation = Vector3.one;

    void Awake() {
        TreeNode2._stopGrowing = false;
        _debug_staticTreeNodePrefab = _debug_treeNodePrefab;
    }

    void Start() {
        SpawnFirstNode();

        StartCoroutine("Grow", _startNode);
    }

    void Update () {
        _debug_testRotation = Quaternion.Euler(Vector3.forward * Time.deltaTime * 360) * _debug_testRotation;
        Debug.DrawRay(Vector3.zero, _debug_testRotation);
    }

    void SpawnFirstNode() {
        GameObject startNode = Instantiate(_debug_treeNodePrefab);
        startNode.transform.position = transform.position;
        startNode.transform.localScale = Vector3.one * 1;
        _startNode = startNode.GetComponent<TreeNode2>();
        _startNode._state = TreeNode2.State.GROWING;
        _startNode._askGrow = true;
        _startNode._rank = 8;
        _startNode._size = 2;
        _startNode._direction = Vector3.up;
    }

    IEnumerator Grow(TreeNode2 node) {
        float delay = 5f;
        float time = delay;
        for (;;) {
            time = Mathf.MoveTowards(time, 0, Time.fixedDeltaTime);
            if (time <= 0) {
                time = delay;
                if (!TreeNode2._stopGrowing) {
                    node._askGrow = true;
                }
            }
            yield return null;
        }
    }

}
