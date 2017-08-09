using NUnit.Framework;

public class SortedLinkedListTest {

	[Test]
	public void add_individual(){
		//Setup
		SortedLinkedList<int> list = new SortedLinkedList<int>((a, b) => a.CompareTo(b));
		int[] expectedArray = {-1, 0, 0, 3, 6, 7, 8, 11};
		
		//Exercise
		list.add(0);
		list.add(-1);
		list.add(0);
		list.add(7);
		list.add(8);
		list.add(6);
		list.add(3);
		list.add(11);
		
		//Verify
		assertElementsEqual(list, expectedArray);
	}
	
	[Test]
	public void add_multiple(){
		//Setup
		SortedLinkedList<int> list = new SortedLinkedList<int>((a, b) => a.CompareTo(b));
		int[] expectedArray = {-1, 0, 0, 3, 6, 7, 8, 11};
		
		//Exercise
		list.add(0, -1, 0, 7, 8, 6, 3, 11);
		
		//Verify
		assertElementsEqual(list, expectedArray);
	}

	[Test]
	public void first(){
		//Setup
		SortedLinkedList<int> list = givenAList();

		//Exercise
		int first = list.first();

		//Verify
		Assert.AreEqual(-1, first);
	}

	private SortedLinkedList<int> givenAList(){
		SortedLinkedList<int> list = new SortedLinkedList<int>((a, b) => a.CompareTo(b));
		list.add(0, -1, 8, 3, 11);
		return list;
	}

	private void assertElementsEqual(SortedLinkedList<int> list, int[] expectedArray){
		int i = 0;
		foreach(int element in list){
			Assert.AreEqual(element, expectedArray[i++]);
		}
	}

}
