public class GameTime {

	public int day{ get; private set; }
	public int hour{ get; private set; }
	public int min{ get; private set; }

	public GameTime(int day, int hour, int min){
		this.day = day;
		this.hour = hour;
		this.min = min;
	}
	
}
