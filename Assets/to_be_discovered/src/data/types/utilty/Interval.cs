﻿using System;
using UnityEngine;

public class Interval {

	private readonly Action action;
	private readonly float interval;
	private float elapsedTime;

	public Interval(Action action, float interval){
		this.action = action;
		this.interval = interval;
		elapsedTime = interval;
	}

	public void update(){
		elapsedTime += Time.deltaTime;
		if(!(elapsedTime > interval))
			return;
		action();
		elapsedTime = 0f;
	}
	
	public void update(bool unlocked){
		elapsedTime += Time.deltaTime;
		if(!(elapsedTime > interval && unlocked))
			return;
		action();
		elapsedTime = 0f;
	}

	public void fire(){
		action();
		elapsedTime = 0f;
	}
	
}
