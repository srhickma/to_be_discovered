using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RangeCompositionTest {

	[Test]
	public void addRange_goodRanges(){
		//Setup
		RangeComposition rc = givenARangeComposition();
		int[] expectedArray = {-1, 0, 1, 4, 6, 7, 8, 9, 10, 11, 13, 14, 15};
		
		//Exercise
		rc.addRange(new Range(-1, -1));
		rc.addRange(new Range(6, 7));
		rc.addRange(new Range(13, 15));
		
		//Verify
		assertEqualCompression(rc, expectedArray);
	}
	
	[Test]
	public void addRange_badRanges(){
		//Setup
		RangeComposition rc = givenARangeComposition();
		
		//Exercise
		TestDelegate td1 = () => rc.addRange(new Range(0, 0));
		TestDelegate td2 = () => rc.addRange(new Range(-2, 13));
		TestDelegate td3 = () => rc.addRange(new Range(5, 8));
		
		//Verify
		Assert.Throws<RangeComposition.RangeCompositionException>(td1);
		Assert.Throws<RangeComposition.RangeCompositionException>(td2);
		Assert.Throws<RangeComposition.RangeCompositionException>(td3);
	}
	
	[Test]
	public void injectRange_surrounding(){
		//Setup
		RangeComposition rc = givenARangeComposition();
		int[] expectedArray = {-3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13};
		
		//Exercise
		rc.injectRange(new Range(-3, 13));
		
		//Verify
		assertEqualCompression(rc, expectedArray);
	}
	
	[Test]
	public void injectRange_before(){
		//Setup
		RangeComposition rc1 = givenARangeComposition();
		RangeComposition rc2 = givenARangeComposition();
		RangeComposition rc3 = givenARangeComposition();
		int[] expectedArray = {-2, -1, 0, 1, 4, 8, 9, 10, 11};
		
		//Exercise
		rc1.injectRange(new Range(-2, -1));
		rc2.injectRange(new Range(-2, 0));
		rc3.injectRange(new Range(-2, 1));
		
		//Verify
		assertEqualCompression(rc1, expectedArray);
		assertEqualCompression(rc2, expectedArray);
		assertEqualCompression(rc3, expectedArray);
	}
	
	[Test]
	public void injectRange_middle1(){
		//Setup
		RangeComposition rc1 = givenARangeComposition();
		RangeComposition rc2 = givenARangeComposition();
		RangeComposition rc3 = givenARangeComposition();
		int[] expectedArray = {0, 1, 4, 5, 6, 7, 8, 9, 10, 11};
		
		//Exercise
		rc1.injectRange(new Range(5, 7));
		rc2.injectRange(new Range(4, 7));
		rc3.injectRange(new Range(5, 9));
		
		//Verify
		assertEqualCompression(rc1, expectedArray);
		assertEqualCompression(rc2, expectedArray);
		assertEqualCompression(rc3, expectedArray);
	}
	
	[Test]
	public void injectRange_middle2(){
		//Setup
		RangeComposition rc1 = givenARangeComposition();
		RangeComposition rc2 = givenARangeComposition();
		int[] expectedArray = {0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11};
		
		//Exercise
		rc1.injectRange(new Range(3, 7));
		rc2.injectRange(new Range(3, 8));
		
		//Verify
		assertEqualCompression(rc1, expectedArray);
		assertEqualCompression(rc2, expectedArray);
	}
	
	[Test]
	public void injectRange_middle3(){
		//Setup
		RangeComposition rc1 = givenARangeComposition();
		RangeComposition rc2 = givenARangeComposition();
		RangeComposition rc3 = givenARangeComposition();
		int[] expectedArray = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};
		
		//Exercise
		rc1.injectRange(new Range(1, 7));
		rc2.injectRange(new Range(1, 10));
		rc3.injectRange(new Range(1, 11));
		
		//Verify
		assertEqualCompression(rc1, expectedArray);
		assertEqualCompression(rc2, expectedArray);
		assertEqualCompression(rc3, expectedArray);
	}
	
	[Test]
	public void injectRange_middle4(){
		//Setup
		RangeComposition rc1 = givenARangeComposition();
		RangeComposition rc2 = givenARangeComposition();
		int[] expectedArray = {0, 1, 4, 7, 8, 9, 10, 11};
		
		//Exercise
		rc1.injectRange(new Range(7, 10));
		rc2.injectRange(new Range(7, 11));
		
		//Verify
		assertEqualCompression(rc1, expectedArray);
		assertEqualCompression(rc2, expectedArray);
	}
	
	[Test]
	public void injectRange_after(){
		//Setup
		RangeComposition rc1 = givenARangeComposition();
		RangeComposition rc2 = givenARangeComposition();
		RangeComposition rc3 = givenARangeComposition();
		RangeComposition rc4 = givenARangeComposition();
		RangeComposition rc5 = givenARangeComposition();
		int[] expectedArray = {0, 1, 4, 8, 9, 10, 11, 12, 13, 14};
		
		//Exercise
		rc1.injectRange(new Range(12, 14));
		rc2.injectRange(new Range(11, 14));
		rc3.injectRange(new Range(10, 14));
		rc4.injectRange(new Range(9, 14));
		rc5.injectRange(new Range(8, 14));
		
		//Verify
		assertEqualCompression(rc1, expectedArray);
		assertEqualCompression(rc2, expectedArray);
		assertEqualCompression(rc3, expectedArray);
		assertEqualCompression(rc4, expectedArray);
		assertEqualCompression(rc5, expectedArray);
	}
	
	[Test]
	public void injectRange_noChange(){
		//Setup
		RangeComposition rc = givenARangeComposition();
		int[] expectedArray = {0, 1, 4, 8, 9, 10, 11};
		
		//Exercise
		rc.injectRange(new Range(1, 1));
		rc.injectRange(new Range(0, 1));
		rc.injectRange(new Range(4, 4));
		rc.injectRange(new Range(8, 10));
		rc.injectRange(new Range(9, 10));
		rc.injectRange(new Range(8, 11));
		
		//Verify
		assertEqualCompression(rc, expectedArray);
	}
	
	[Test]
	public void injectRange_lowSpread(){
		//Setup
		RangeComposition rc = new RangeComposition();
		int[] expectedArray = {2, 4, 5, 7};
		
		//Exercise
		rc.injectRange(new Range(2, 2));
		rc.injectRange(new Range(5, 5));
		rc.injectRange(new Range(7, 7));
		rc.injectRange(new Range(4, 4));

		//Verify
		assertEqualCompression(rc, expectedArray);
	}
	
	[Test]
	public void injectRange_swap(){
		//Setup
		RandomGenerator randomGenerator = new RandomGenerator();
		RangeComposition excludedSet = new RangeComposition();
		Range universe = new Range(0, 20);
		int removed = 0;
		
		//Exercise
		while(true){
			RangeComposition includedSet = excludedSet.inverseRangeComposition(universe);
			if(includedSet.compressedSize() == 0){
				break;
			}
			int x = randomGenerator.nextInt(includedSet);
			excludedSet.injectRange(new Range(x, x));
			removed++;
		}

		//Verify
		Assert.AreEqual(19, removed);
	}

	[Test]
	public void inverseRangeComposition_outerBound(){
		//Setup
		RangeComposition rc = givenARangeComposition();
		Range universe = new Range(-2, 14);
		int[] expectedArray = {-1, 2, 3, 5, 6, 7, 12, 13};

		//Exercise
		RangeComposition irc = rc.inverseRangeComposition(universe);

		//Verify
		assertEqualCompression(irc, expectedArray);
	}
	
	[Test]
	public void inverseRangeComposition_midBound(){
		//Setup
		RangeComposition rc = givenARangeComposition();
		Range universe = new Range(0, 9);
		int[] expectedArray = {2, 3, 5, 6, 7};

		//Exercise
		RangeComposition irc = rc.inverseRangeComposition(universe);

		//Verify
		assertEqualCompression(irc, expectedArray);
	}
	
	[Test]
	public void inverseRangeComposition_innerBound(){
		//Setup
		RangeComposition rc = givenARangeComposition();
		Range universe = new Range(4, 9);
		int[] expectedArray = {5, 6, 7};

		//Exercise
		RangeComposition irc = rc.inverseRangeComposition(universe);

		//Verify
		assertEqualCompression(irc, expectedArray);
	}
	
	[Test]
	public void inverseRangeComposition_touchingOuter(){
		//Setup
		RangeComposition rc = new RangeComposition();
		rc.injectRange(new Range(1, 1));
		Range universe = new Range(0, 7);
		int[] expectedArray = {2, 3, 4, 5, 6};

		//Exercise
		RangeComposition irc = rc.inverseRangeComposition(universe);

		//Verify
		assertEqualCompression(irc, expectedArray);
	}
	
	[Test]
	public void isEmptyNotEmpty(){
		//Setup
		RangeComposition rc = givenARangeComposition();
		
		//Exercise
		bool empty = rc.isEmpty();

		//Verify
		Assert.IsFalse(empty);
	}
	
	[Test]
	public void isEmptyEmpty(){
		//Setup
		RangeComposition rc = new RangeComposition(new LinkedList<Range>());
		
		//Exercise
		bool empty = rc.isEmpty();

		//Verify
		Assert.IsTrue(empty);
	}
	
	[Test]
	public void compressedSizeTest(){
		//Setup
		RangeComposition rc = givenARangeComposition();
		
		//Exercise
		int size = rc.compressedSize();

		//Verify
		Assert.AreEqual(7, size);
	}
	
	[Test]
	public void expandedValueOf(){
		//Setup
		RangeComposition rc = givenARangeComposition();
		int[] expectedArray = {0, 1, 4, 8, 9, 10, 11};
		
		//Exercise/Verify
		assertEqualCompression(rc, expectedArray);
	}

	private static void assertEqualCompression(RangeComposition rc, int[] expectedArray){
		for(int i = 0; i < rc.compressedSize(); i ++){
			Assert.AreEqual(expectedArray[i], rc.expandedValueOf(i));
		}
	}

	private static RangeComposition givenARangeComposition(){
		Range r1 = new Range(0, 1);
		Range r2 = new Range(4, 4);
		Range r3 = new Range(8, 11);
		List<Range> ranges = new List<Range>{r1, r2, r3};
		return new RangeComposition(new LinkedList<Range>(ranges));
	}
	
}
