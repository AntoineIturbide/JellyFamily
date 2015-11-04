using UnityEngine;
using System.Collections;

public class ParticleEffectScript : MonoBehaviour {

    ParticleSystem _particleSystem;
    public Transform _target;

    public float _minDistance;
    public float _maxSpeed;

    // Use this for initialization
    void Awake () {
        _particleSystem = gameObject.GetComponent<ParticleSystem>();
        _particleSystem.startSpeed = 0;
    }
	
	// Update is called once per frame
	void Update () {
        float speed = Vector3.Distance(_target.position, transform.position);
        if(speed < _minDistance) {
            speed = 0;
        } else {
            speed = Mathf.Clamp(speed, _minDistance, _minDistance * 2);
            speed -= _minDistance;
            speed = Mathf.Lerp(0, _maxSpeed, speed / _minDistance);
        }

        _particleSystem.startSpeed = speed;
    }
}
