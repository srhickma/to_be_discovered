using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateSystem{

	public static float BLOCK_WIDTH = 0.625f;

	public static Vector3 toReal(float x, float y){
		return new Vector3(toReal(x), toReal(y), 0);
	}

	public static Vector3 toReal(Vector2 pos){
		return new Vector3(toReal(pos.x), toReal(pos.y), 0);
	}

	public static float toReal(float x){
		return x * BLOCK_WIDTH + 0.5f;
	}

	public static float fromReal(float x){
		return (x - 0.5f) / BLOCK_WIDTH;
	}

}
