using UnityEngine;
using System.Collections;

public class ChildGiver : MonoBehaviour {

    void OnTriggerEnter2D ( Collider2D collider) {
        UnitSystem unit = collider.gameObject.GetComponent<UnitSystem>();
        if (unit == null)
            unit = collider.transform.GetComponentInParent<UnitSystem>();
        if (unit == null)
            return;
        unit._childCount += 1;

        Debug.Log("ChildGiver : Implement event pooler");
        Destroy(gameObject);

    }

}
