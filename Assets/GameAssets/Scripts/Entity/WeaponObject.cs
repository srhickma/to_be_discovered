using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponObject : MonoBehaviour {

	public Transform raySource;

	public LayerMask raysHit;

	private void shoot(Weapon weapon){
		print("HERE");
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 delta = mousePos - (Vector2)raySource.position;
		RaycastHit2D hit = Physics2D.Raycast(raySource.position, delta, Mathf.Infinity, raysHit);
		if(hit.collider != null){
			DrawLine(raySource.position, hit.point, Color.grey, 0.05f);
		}
		else{
			DrawLine(raySource.position, hit.point, Color.grey, 0.05f);
		}
	}

	private static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f){
		GameObject line = new GameObject();
		line.transform.position = start;
		line.AddComponent<LineRenderer>();
		LineRenderer lr = line.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.SetColors(color, color);
		lr.SetWidth(0.06f, 0.06f);
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		Destroy(line, duration);
	}

}
