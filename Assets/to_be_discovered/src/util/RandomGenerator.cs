using System;
using System.Collections.Generic;

public class RandomGenerator {

	private readonly Random random = new Random();

	public int nextInt(Range range){
		return random.Next(range.min, range.max + 1);
	}
	
	public int nextInt(Range range1, Range range2){
		int min = IntMath.max(range1.min, range2.min);
		int max = IntMath.min(range1.max, range2.max);
		return random.Next(min, max + 1);
	}
	
	//f(x)E[0,1] for all xE[0,1]
	public int nextInt(int maxExc, Func<float, float> f){
		return (int)(f((float)random.NextDouble()) * maxExc);
	}


	public int nextInt(RangeComposition rangeComposition){
		return rangeComposition.expandedValueOf(random.Next(rangeComposition.compressedSize()));
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
		for(; iterations > 1; iterations --){
			int newMin = nextInt(new Range(tendFrom.min, tendTo.min));
			int newMax = nextInt(new Range(tendFrom.max, tendTo.max));
			tendFrom = new Range(newMin, newMax);
		}
		return rangeBetween(tendFrom, tendTo);
	}

	public Range subRange(int size, Range universe){
		int min = nextInt(new Range(universe.min + 1, universe.max - size));
		return new Range(min, min + size - 1);
	}

	public T randomPreset<T>(T[] presets){
		return presets[nextInt(0, presets.Length)];
	}
	
	public T randomPreset<T>(List<T> presets){
		return presets[nextInt(0, presets.Count)];
	}

}
