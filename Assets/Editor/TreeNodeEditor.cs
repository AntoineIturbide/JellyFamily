using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TreeNode))]

public class TreeNodeEditor : Editor {

    public override void OnInspectorGUI() {
        TreeNode myTarget = (TreeNode)target;

        Handles.DrawSolidArc(myTarget.transform.position, Vector3.up, Vector3.right, 90f, 5f);
    }

}