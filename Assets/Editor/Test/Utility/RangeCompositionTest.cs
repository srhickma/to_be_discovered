using System.Collections.Generic;
using NUnit.Framework;

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
