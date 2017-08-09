using System.Collections.Generic;
using UnityEngine;

public class NavComputeObject : MonoBehaviour {
	
	private static readonly Queue<CompletableFuture<Navigation, NavPath>> navFutureQueue = new Queue<CompletableFuture<Navigation, NavPath>>();

	private const int FRAME_COMPUTE_ITERS = 100;

	private void Update(){
		for(int i = 0; navFutureQueue.Peek() != null && i < FRAME_COMPUTE_ITERS; i ++){
			CompletableFuture<Navigation, NavPath> navFuture = navFutureQueue.Dequeue();
			navFuture.complete();
		}
	}

	public static void enqueueNavigation(CompletableFuture<Navigation, NavPath> navFuture){
		navFutureQueue.Enqueue(navFuture);
	}
	
}
