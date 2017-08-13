using UnityEngine;

public class TimeController : MonoBehaviour {

	private const float SECONDS_PER_GAME_MIN = 0.1f;

	public int day{ get; private set; }
	public int hour{ get; private set; }
	public int min{ get; private set; }

	private float elapsedSeconds;

	private void Awake(){
		day = 1;
		hour = 0;
		elapsedSeconds = 0f;
	}

	private void Update(){
		elapsedSeconds += Time.deltaTime;
		if(elapsedSeconds > SECONDS_PER_GAME_MIN){
			min++;
			if(min > 59){
				min = 0;
				hour++;
				if(hour > 23){
					hour = 0;
					day++;
				}
			}
			elapsedSeconds = 0f;
		}
	}

	private string getTimeString(){
		return string.Format("{0:D2}:{1:D2} Day {2}", hour, min, day);
	}
	
}
