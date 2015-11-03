using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPoolSystem : MonoBehaviour {

    /* Link to instanciated pool system */
    static ObjectPoolSystem _mainObjectPoolSystem = null;

    /* Prefabs */
    [Header("Prefabs")]

    // Unit prefab
    public GameObject _unitPrefab;
    // Node prefab
    public GameObject _nodePrefab;
    // Tree prefab
    public GameObject _treePrefab;


    /* Object pools */

    // Unit object pool
    List<GameObject> _unitPool;
    [Header("Pools start size")]
    public byte _unitPoolStartSize = 10;

    // Node object pool
    List<GameObject> _nodePool;
    public byte _nodePoolStartSize = 10;

    // Tree node object pool
    List<GameObject> _treePool;
    public byte _treePoolStartSize = 10;

    // Minimum size of a pool
    public byte _minimumPoolStartSize = 10;


    /* Unity */

    void Awake() {
        _mainObjectPoolSystem = this;
    }


    /* Generate pools content at start */
    void StartPool(List<GameObject> pool, byte startSize, GameObject prefab) {
        GameObject newInstance = null;
        for (int i = 0; i < startSize; i++) {
            newInstance = Instantiate(prefab);
            newInstance.transform.parent = transform;
            pool.Add(newInstance);
        }
    }


    /* Add to object pool */

    // Add unit to the unit pool
    void PushUnit(GameObject unit) {
        unit.SetActive(false);
        unit.transform.parent = transform;
        _unitPool.Add(unit);
    }

    void PushNode(GameObject node) {
        node.SetActive(false);
        node.transform.parent = transform;
        _unitPool.Add(node);
    }

    void PushTree(GameObject tree) {
        tree.SetActive(false);
        tree.transform.parent = transform;
        _unitPool.Add(tree);
    }


    /* Get from object pool */

    // Get and remove unit from the unit pool
    GameObject PullUnit() {
        GameObject outputUnit = null;
        if (_unitPool.Count <= 0) {
            Debug.Log("Error - " + this + " : PullUnit() called but the unit list is empty : Have to instanciate a new unit.");
            outputUnit = Instantiate(_unitPrefab);
        } else {
            outputUnit = _unitPool[0];
            _unitPool.RemoveAt(0);
        }
        return outputUnit;
    }

    // Get and remove node from the node pool
    GameObject PullNode() {
        GameObject outputNode = null;
        if (_unitPool.Count <= 0) {
            Debug.Log("Error - " + this + " : PullNode() called but the node list is empty : Have to instanciate a new node.");
            outputNode = Instantiate(_nodePrefab);
        } else {
            outputNode = _nodePool[0];
            _unitPool.RemoveAt(0);
        }
        return outputNode;
    }

    // Get and remove tree node from the tree node pool
    GameObject PullTree() {
        Debug.Log("Debug - " + this + " : PullTree() called but wasn't implemented yet.");
        return null;
    }

    /* Public methods */

    public GameObject GetNewUnit() {
        return PullUnit();
    }

    public GameObject GetNewNode() {
        return PullNode();
    }

    public bool TrashUnit(GameObject unit) {
        PushUnit(unit);
        return true;
    }

    public bool TrashNode(GameObject node) {
        PushNode(node);
        return true;
    }

}
