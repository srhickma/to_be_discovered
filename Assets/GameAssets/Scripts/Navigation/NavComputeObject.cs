using System.Collections.Generic;
using UnityEngine;

public class NavComputeObject : MonoBehaviour {
	
	private static readonly Queue<CompletableFuture<Navigation, NavPath>> navFutureQueue = new Queue<CompletableFuture<Navigation, NavPath>>();
	private static int queueLength;

	private const int FRAME_COMPUTE_ITERS = 10;

	private void Update(){
		Repeatable.invoke(() => {
			CompletableFuture<Navigation, NavPath> navFuture = navFutureQueue.Dequeue();
			try{
				navFuture.complete();
			}
			catch(MissingReferenceException){}
			
			queueLength--;
		}, IntMath.min(queueLength, FRAME_COMPUTE_ITERS));
	}

	public static void enqueueNavigation(CompletableFuture<Navigation, NavPath> navFuture){
		navFutureQueue.Enqueue(navFuture);
		queueLength++;
	}
	
}
