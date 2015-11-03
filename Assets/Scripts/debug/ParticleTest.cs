using UnityEngine;
using System.Collections;

public class ParticleTest : MonoBehaviour {

    ParticleSystem _particleSystem;

    Transform _end;

    void Awake() {
        _particleSystem = gameObject.GetComponent<ParticleSystem>();
        _end = GameObject.Find("End").transform;
    }

    void Update() {
        transform.LookAt(_end);
        _particleSystem.startLifetime = Vector3.Distance(transform.position, _end.position)/_particleSystem.startSpeed;

        Vector3 delta = transform.position - _end.position;
        delta.z = 0;
        float angle;
        if (delta.y > 0)
            angle = Vector3.AngleBetween(-Vector3.right, delta);
        else
            angle = -Vector3.AngleBetween(-Vector3.right, delta);

        _particleSystem.startRotation = angle;

        Debug.Log(angle);
    }

}
