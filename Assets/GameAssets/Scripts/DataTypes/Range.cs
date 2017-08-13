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

	public static Range surrounding(int x, int radius){
		return new Range(x - radius, x + radius);
	}

	public int size(){
		return max - min + 1;
	}

	public bool equals(Range range){
		return min == range.min && max == range.max;
	}

	public bool contains(int x){
		return x >= min && x <= max;
	}

	public bool overlapsWith(Range range){
		return contains(range.min) || contains(range.max) || range.contains(min) || range.contains(max);
	}
	
}
