using System;

public class Repeatable {

	public static void invoke(Action action, int times){
		for(int i = 0; i < times; i ++){
			action();
		}
	}
	
}
