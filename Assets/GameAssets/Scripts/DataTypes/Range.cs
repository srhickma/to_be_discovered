using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range {

	public int min { get; set; }
	public int max { get; set; }

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

	public bool equals(Range range){
		return min == range.min && max == range.max;
	}
	
}
