using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallThrough : MonoBehaviour {

	void Start(){
		StartCoroutine(LateStart(0.01f));
	}

	IEnumerator LateStart(float waitTime){
		yield return new WaitForSeconds(waitTime);
		Physics2D.IgnoreCollision(Player.collider, gameObject.GetComponent<EdgeCollider2D>(), true);
	}

}
