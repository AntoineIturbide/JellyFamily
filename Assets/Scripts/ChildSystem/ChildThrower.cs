using UnityEngine;
using System.Collections;

public class ChildThrower : MonoBehaviour {

    public GameObject _childGiverPrefab;

    float delay = 0.1f;
    float time = 0f;

	// Update is called once per frame
	void Update () {
        time -= Time.deltaTime;
        if (time <= 0) {
            time = delay;
            GameObject newinstance = Instantiate(_childGiverPrefab);
            newinstance.GetComponent<Rigidbody2D>().velocity = new Vector2(
                Random.Range(-10f, 10f),
                Random.Range(20f, 100f)
                );
            newinstance.transform.position = new Vector3(
                Random.Range(-30f, 30f),
                transform.position.y,
                0
                );
        }
    }
}
