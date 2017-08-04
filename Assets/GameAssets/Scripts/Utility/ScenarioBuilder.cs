using System;
using System.Collections.Generic;

public class ScenarioBuilder {
    
    private readonly List<Action> actions = new List<Action>();

    private ScenarioBuilder(Action baseAction){
        actions.Add(baseAction);
    }

    public static ScenarioBuilder withScenario(Action action){
        return new ScenarioBuilder(action);
    }
    
    public ScenarioBuilder or(Action action){
        actions.Add(action);
        return this;
    }

    public void invokeRandom(RandomGenerator randomGenerator){
        Action action = randomGenerator.randomPreset(actions);
        action.Invoke();
    }
    
}
