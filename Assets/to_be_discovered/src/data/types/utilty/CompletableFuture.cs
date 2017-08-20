using System;
using System.Collections;
using UnityEngine;

public class CompletableFuture<P, R> {

    private readonly Func<P, R> func;
    private P param;
    private Action<R> callback;

    private CompletableFuture(Func<P, R> func){
        this.func = func;
        param = default(P);
        callback = r => { };
    }

    public static CompletableFuture<P, R> run(Func<P, R> func){
        return new CompletableFuture<P, R>(func);
    }
    
    public CompletableFuture<P, R> with(P param){
        this.param = param;
        return this;
    }

    public CompletableFuture<P, R> then(Action<R> callback){
        this.callback = callback;
        return this;
    }

    public void complete(){
        callback.Invoke(func.Invoke(param));
    }

}

public class CompletableFuture {

    private readonly Action action;
    
    private CompletableFuture(Action action){
        this.action = action;
    }
    
    public static CompletableFuture run(Action action){
        return new CompletableFuture(action);
    }

    public void invokeIn(float waitTime, MonoBehaviour behaviour){
        behaviour.StartCoroutine(waitFunction(action, waitTime));
    }

    private static IEnumerator waitFunction(Action action, float waitTime){
        yield return new WaitForSeconds(waitTime);
        action();
    }

}
