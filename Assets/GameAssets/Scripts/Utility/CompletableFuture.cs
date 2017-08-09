using System;

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
