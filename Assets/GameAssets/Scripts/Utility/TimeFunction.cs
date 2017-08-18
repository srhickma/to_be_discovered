using System;

public class TimeFunction {

    private readonly Func<GameTime, int> function;

    public TimeFunction(Func<GameTime, int> function){
        this.function = function;
    }

    public int eval(){
        return function.Invoke(new GameTime(TimeController.day, TimeController.hour, TimeController.min));
    }
    
}