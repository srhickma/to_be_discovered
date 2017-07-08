using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range {

	public int min { get; }
	public int max { get; }

	public Range(int a, int b){
		if(a > b){
			max = a;
			min = b;
		}
		else{
			max = b;
			min = a;
		}
	}

	public int size(){
		return max - min + 1;
	}
	
}
