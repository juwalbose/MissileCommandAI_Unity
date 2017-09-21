using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour {
	float deployDistance=3000;//after traveling this much distance, Kaboom!

	Rigidbody2D missileRb;
	Vector2 missileLaunchPos;//Turret position

	void Awake(){
		missileRb=GetComponent<Rigidbody2D>();
	}
	void Update () {
		if(Vector2.Distance(transform.position,missileLaunchPos)>deployDistance){//once we have traveled the set distance, return to pool
			ReturnToPool();
		}
	}
	void ReturnToPool(){
		missileRb.velocity=Vector2.zero;
		SimplePool.Despawn(gameObject);
	}
	public void LockOn(float distance){//set the distance to travel to hit the incoming target
		missileLaunchPos=transform.position;
		deployDistance=distance;
	}
}
