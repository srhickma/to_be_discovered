using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RandomGeneratorTest {
	
	private readonly RandomGenerator randomGenerator = new RandomGenerator();
	private const float rangeDelta = 0.08f;

	[Test]
	public void subRange(){
		//Setup
		Dictionary<int, int> distribution = new Dictionary<int, int>();
		Range universe = new Range(10, 20);
		const int iterations = 1000;
		const int size = 4;
		float expectedProbability = 1 / (float)(universe.size() - size + 1);
		
		//Exercise/Verify
		Repeatable.invoke(() => {
			Range range = randomGenerator.subRange(size, universe);
			Assert.AreEqual(size, range.size());
			Assert.True(range.min > universe.min && range.max < universe.max);
			if(distribution.ContainsKey(range.min)){
				distribution[range.min]++;
			}
			else{
				distribution.Add(range.min, 1);
			}
		}, iterations);
		
		//Verify
		foreach(int occurrences in distribution.Values){
			Assert.True(Mathf.Abs(expectedProbability - occurrences / (float)iterations) < rangeDelta);
		}
	}
	
}