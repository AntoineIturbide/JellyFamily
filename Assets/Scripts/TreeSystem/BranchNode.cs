using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BranchNode { 
    public static GameObject _debug_prisePrefab;
    public static byte _debug_maxIteration = 5;
    public static byte _debug_currentIteration = 0;

    static List<Vector3[]> _rays = new List<Vector3[]>();
    static byte _treeRank = 0;

    Vector3 _position = Vector3.zero;
    byte _rank = 0;
    List<BranchNode> _branchNodes = new List<BranchNode>();
    
    public BranchNode(Vector3 position, byte rank) {
        _position = position;
        _rank = rank;
    }

    public void Grow() {
        if(_branchNodes.Count <= 0) {
            MakeBranch();
        } else {
            foreach (BranchNode node in _branchNodes) {
                node.Grow();
            }
        }
    }

    public void MakeBranch() {
        if (_rank <= 0)
            return;

        /*
        for (int i = 0; _rank / 3 > 1 && i < _rank / 3; i++) {
            AddBranch((byte)(_rank - 1));
        }*/
        for (int i = 0; i < 3; i++) {
            AddBranch(1);
        }
    }

    public void AddBranch(byte rank) {
        Vector3 position = _position + new Vector3(
            Random.Range(-5f -_debug_currentIteration, 5f + _debug_currentIteration),
            Random.Range(2f, 5f + _debug_currentIteration),
            0
            );
        _branchNodes.Add(new BranchNode(position, rank));
        _rays.Add(new Vector3[2] { _position, position - _position });

        // Debug
        GameObject debugGO = GameObject.Instantiate(_debug_prisePrefab);
        debugGO.transform.position = position;

        //Debug.Log(_rays.Count);
    }

    public void DisplayBranchs() {
        foreach (Vector3[] ray in _rays) {
            Debug.DrawRay(ray[0], ray[1], Color.gray);
        }
    }

    public void ResetRays() {
        _rays = new List<Vector3[]>();
    }

}
