using UnityEngine;

public class Sound {

	public AudioClip clip{ get; private set; }
	public float radius{ get; private set; }
	public bool isTrigger{ get; private set; }

	public Sound(AudioClip clip, float radius, bool isTrigger){
		this.clip = clip;
		this.radius = radius;
		this.isTrigger = isTrigger;
	}

}
