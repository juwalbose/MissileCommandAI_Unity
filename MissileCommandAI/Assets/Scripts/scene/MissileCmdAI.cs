using UnityEngine;

public class MissileCmdAI : MonoBehaviour {
	//Prefabs
	public GameObject asteroidPrefab;
	public GameObject missilePrefab;
	public AnimationCurve asteroidSpawnTimeCurve;//Animation curve to control the timing of new asteroids launching
	public GameObject turret;//the turret
	public float missileSpeed;//speed of the missile

	//values for tracking the animation curve
	float curveTimeValue=0;
	float curveTimeIncrement=0.02f;//how fast we need to proceed through the animation curve
	float timeMultiplier=2;
	float groundProximity;//below this we have a missile which is imminent threat
	float aiPollTime=0.6f;//how frequent we should call the ai code.

	// Use this for initialization
	void Start () {//create pools of objects for reuse
		SimplePool.Preload(asteroidPrefab,6);
		SimplePool.Preload(missilePrefab,4);
		StartAIGame();
	}
	
	public void StartAIGame(){
		groundProximity=Screen.height/2;//anything below is a imminent threat
		InvokeRepeating("FindTarget",1,aiPollTime);//set ai code polling
		SetNextSpawn();//set asteroid spawn
	}
	void SetNextSpawn(){//this sets time for the next asteroid launch
		float nextTimeValue=asteroidSpawnTimeCurve.Evaluate(curveTimeValue);
		Invoke("SpawnAsteroid",timeMultiplier*nextTimeValue);
		curveTimeValue+=curveTimeIncrement;
		curveTimeValue=Mathf.Clamp01(curveTimeValue);
	}
	void SpawnAsteroid(){
		GameObject asteroid=SimplePool.Spawn(asteroidPrefab,Vector2.one,Quaternion.identity);
		Asteroid asteroidScript=asteroid.GetComponent<Asteroid>();
		asteroidScript.Launch();
		SetNextSpawn();
	}
	void FindTarget(){//find fastest & closest asteroid
		GameObject[] aArr=GameObject.FindGameObjectsWithTag("asteroid");
		GameObject closestAsteroid=null;
		Asteroid fastestAsteroid=null;
		Asteroid asteroid;
		foreach(GameObject go in aArr){
			if(go.transform.position.y<groundProximity){//find closest
				if(closestAsteroid==null){
					closestAsteroid=go;
				}else if(go.transform.position.y<closestAsteroid.gameObject.transform.position.y){
					closestAsteroid=go;
				}
			}
			asteroid=go.GetComponent<Asteroid>();
			if(fastestAsteroid==null){//find fastest
				fastestAsteroid=asteroid;
			}else if(asteroid.asteroidSpeed>fastestAsteroid.asteroidSpeed){
				fastestAsteroid=asteroid;
			}
		}
		//if we have a closest one target that, else target the fastest
		if(closestAsteroid!=null){
			AcquireTargetLock(closestAsteroid);
		}else if(fastestAsteroid!=null){
			AcquireTargetLock(fastestAsteroid.gameObject);
		}
	}
	void AcquireTargetLock(GameObject targetAsteroid){
		/* Solve the quadratic equation
		a * sqr(x) + b * x + c == 0
		where
		a := sqr(target.velocityX) + sqr(target.velocityY) - sqr(projectile_speed)
		b := 2 * (target.velocityX * (target.startX - cannon.X)
          + target.velocityY * (target.startY - cannon.Y))
		c := sqr(target.startX - cannon.X) + sqr(target.startY - cannon.Y)
		Solution-
		disc := sqr(b) - 4 * a * c
		if disc<0 then you can't hit anymore else
		t1 := (-b + sqrt(disc)) / (2 * a)
		t2 := (-b - sqrt(disc)) / (2 * a)
		 */

		Asteroid asteroidScript=targetAsteroid.GetComponent<Asteroid>();
		Vector2 targetVelocity=asteroidScript.asteroidRb.velocity;
		
		float a=(targetVelocity.x*targetVelocity.x)+(targetVelocity.y*targetVelocity.y)-(missileSpeed*missileSpeed);
		float b=2*(targetVelocity.x*(targetAsteroid.gameObject.transform.position.x-turret.transform.position.x) 
		+targetVelocity.y*(targetAsteroid.gameObject.transform.position.y-turret.transform.position.y));
		float c= ((targetAsteroid.gameObject.transform.position.x-turret.transform.position.x)*(targetAsteroid.gameObject.transform.position.x-turret.transform.position.x))+
		((targetAsteroid.gameObject.transform.position.y-turret.transform.position.y)*(targetAsteroid.gameObject.transform.position.y-turret.transform.position.y));

		float disc= b*b -(4*a*c);
		if(disc<0){
			Debug.LogError("No possible hit!");
		}else{
			float t1=(-1*b+Mathf.Sqrt(disc))/(2*a);
			float t2=(-1*b-Mathf.Sqrt(disc))/(2*a);
			float t= Mathf.Max(t1,t2);// let us take the larger time value 
			float aimX=(targetVelocity.x*t)+targetAsteroid.gameObject.transform.position.x;
			float aimY=targetAsteroid.gameObject.transform.position.y+(targetVelocity.y*t);
			RotateAndFire(new Vector2(aimX,aimY));//now position the turret
		}
	}
	public void RotateAndFire(Vector2 deployPos){//AI based turn & fire
		float turretAngle=Mathf.Atan2(deployPos.y-turret.transform.position.y,deployPos.x-turret.transform.position.x)*Mathf.Rad2Deg;
		turretAngle-=90;//art correction
		turret.transform.localRotation=Quaternion.Euler(0,0,turretAngle);
		FireMissile(deployPos, turretAngle);//launch missile
	}
	void FireMissile(Vector3 deployPos, float turretAngle){
		float deployDist= Vector3.Distance(deployPos,turret.transform.position);//how far is our target
		
		GameObject firedMissile=SimplePool.Spawn(missilePrefab,turret.transform.position,Quaternion.Euler(0,0,turretAngle));
		Rigidbody2D missileRb=firedMissile.GetComponent<Rigidbody2D>();
		Missile missileScript=firedMissile.GetComponent<Missile>();
		missileScript.LockOn(deployDist);
		missileRb.velocity=missileSpeed*firedMissile.transform.up;//missile is rotated in necessary direction already
	}
}
