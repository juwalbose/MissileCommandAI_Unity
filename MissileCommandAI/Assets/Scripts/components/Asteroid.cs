using UnityEngine;

public class Asteroid : MonoBehaviour {
	public float asteroidSpeed;//the speed of our asteroid, need to be accessible from outside for checks
	public Rigidbody2D asteroidRb;//need external access to the rigidbody
	public float asteroidMaxSpeed=6;//maximum speed range
	public float asteroidMinSpeed=3;//minimum speed range

	Vector2 tl;
	Vector2 tr;
	Vector2 bl;
	Vector2 br;
	Vector2 asteroidPos;
	Vector2 destination;
	float deployDistance=3000;

	void Awake(){
		asteroidRb=GetComponent<Rigidbody2D>();
		asteroidPos=new Vector2(0,0);
		destination=new Vector2(0,0);
	}
	public void Launch(){//place the asteroid in top with random x & launch it to bottom with random x
		bl=Camera.main.ScreenToWorldPoint(new Vector2(10,0));
		br=Camera.main.ScreenToWorldPoint(new Vector2(Screen.width-20,0));
		tl=Camera.main.ScreenToWorldPoint(new Vector2(0,Screen.height));
		tr=Camera.main.ScreenToWorldPoint(new Vector2(Screen.width,Screen.height));
		
		transform.localScale=Vector2.one*(0.2f+Random.Range(0.2f,0.8f));
		asteroidSpeed=Random.Range(asteroidMinSpeed,asteroidMaxSpeed);
		asteroidPos.x=Random.Range(tl.x,tr.x);
		asteroidPos.y=tr.y+1;
		destination.y=bl.y;
		destination.x=Random.Range(bl.x,br.x);
		Vector2 velocity= asteroidSpeed* ((destination-asteroidPos).normalized);
		transform.position=asteroidPos;
		asteroidRb.velocity=velocity;//set a velocity to rigidbody to set it in motion
		
		deployDistance=Vector3.Distance(asteroidPos,destination);//after traveling this much distance, return to pool
	}
	void Update () {
		if(Vector2.Distance(transform.position,asteroidPos)>deployDistance){//once we have traveled the set distance, return to pool
			ReturnToPool();
		}
	}
	void ReturnToPool(){
		asteroidRb.velocity=Vector2.zero;
		SimplePool.Despawn(gameObject);
	}
	void OnTriggerEnter2D(Collider2D projectile) {
		if(projectile.gameObject.CompareTag("missile")){//check collision with missile, return to pool
			ReturnToPool();
		}
    }
}
