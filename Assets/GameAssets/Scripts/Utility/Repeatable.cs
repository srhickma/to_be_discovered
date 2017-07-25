using System;
using UnityEngine;

public class Repeatable : MonoBehaviour {

	public static void invoke(Action action, int times){
		for(int i = 0; i < times; i ++){
			action();
		}
	}
	
}
