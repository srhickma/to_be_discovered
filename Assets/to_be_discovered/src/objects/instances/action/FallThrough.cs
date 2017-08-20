using UnityEngine;

public class FallThrough : MonoBehaviour {

	void Start(){
		CompletableFuture
			.run(() => Physics2D.IgnoreCollision(Player.collider, gameObject.GetComponent<EdgeCollider2D>(), true))
			.invokeIn(0.01f, this);
	}

}