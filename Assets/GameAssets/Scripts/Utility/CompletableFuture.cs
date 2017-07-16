using System.Collections;
using System;
using UnityEngine;

public class CompletableFuture {

    public static void runAsync(Action action, float delay, MonoBehaviour monoBehaviour){
        monoBehaviour.StartCoroutine(func(action, delay));
    }

    private static IEnumerator func(Action action, float delay){
        yield return new WaitForSeconds(delay);
        action();
    }
    
}
