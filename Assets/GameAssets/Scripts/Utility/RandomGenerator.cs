using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RandomGenerator {

	private System.Random random = new System.Random();

	public int nextInt(Range range){
		return random.Next(range.min, range.max + 1);
	}

	public int nextInt(int minInc, int maxExc){
		return random.Next(minInc, maxExc);
	}

	public int nextSign(){
		return random.Next(0, 2) * 2 - 1;
	}

	public double nextDouble(){
		return random.NextDouble();
	}

	public bool nextBool(){
		return random.NextDouble() < 0.5;
	}

	public bool nextBool(double freq){
		return random.NextDouble() < freq;
	}

	public Range rangeBetween(Range a, Range b){
		int newMin = nextInt(new Range(a.min, b.min));
		int newMax = nextInt(new Range(a.max, b.max));
		return new Range(newMin, newMax);
	}

	public Range rangeBetween(Range tendFrom, Range tendTo, int iterations){
		if(iterations == 1){
			return rangeBetween(tendFrom, tendTo);
		}
		int newMin = nextInt(new Range(tendFrom.min, tendTo.min));
		int newMax = nextInt(new Range(tendFrom.max, tendTo.max));
		return rangeBetween(new Range(newMin, newMax), tendTo, iterations - 1);
	}

	public T randomPreset<T>(T[] presets){
		return presets[nextInt(0, presets.Count())];
	}

}
