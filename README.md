to_be_discovered
========

to_be_discovered (TBD) is a 2D side scroller game where the player must fight to survive against an endless onslaught of zombies, as well as nature itself. TBD is still in early development, and is largely an experiment in realistic (bounded) procedural generation, as well as the AI challenges associated with navigating in a random world.

![alt text](img/main.png "Logo Title Text 1")


Running
-----

TBD is built withing the Unity Game Engine on Linux, but can be build or run across many platforms through the Unity Editor. The primary editor used was Rider (by JetBrains) using the necessary Rider/Unity plugins.The core TBD code is written in C# with unit tests written using NUnit.


Assets
-----

The main TBD specific assets (and code-base) are located under Assets/to_be_discovered/ and are grouped by their respective types. All unit tests are located under Assets/Editor/Test/ and can only be run through the Unity Editor due to complications with Unity runtime dependencies.


Core Components
-----

The core code-base is split up into several distinct sections:
* **controllers** 

	Single instance MonoBehaviours used to control the flow of the game (spawning, random generation, time keeping, etc.)

* **entities** 

	Runtime instantiated MonoBehaviours representing physical GameObjects that are interacted with through user input and controllers (player, zombies, etc.).

* **navigation** 

	All navigation related objects including a computation delegate, ingame data caching components, data types, and the path solving algorithm. Only category containing both MonoBehaviour and raw computation objects.

* **data** 

	Custom containers and data types, mainly for use in navigation and random generation algorithms. Includes faster non-MonoBehaviour representations of ingame objects for storing data (Building, Floor, etc.).

* **util** 

	Utility code and abstractions for better random generation using Ranges and RangeCompositions.


Navigation
-----

### Structure
Navigation in TBD is based on a node structure of NavNode objects that are joined upon generation, creating a web-like mesh that can then be traversed from one NavNode to another. This linked node structure provides a very intuitive base for pathfinding through a dynamic and randomly generated world, since nodes can be added randomly at runtime without performance hits by simply linking new nodes to existing nodes. This would not be possible in most traditional pathfinding approaches where paths are "baked" beforehand and never change. Then NavAgent components are placed on important objects in the game (like players and zombies) which keep track of the whereabouts of these objects through the mesh, thus providing start and end points from which to compute navigation paths. Using dynamic linked lists that store buildings and their NavNodes, this "closest node" can be computed extremely fast and has no noticeable performance impact for 100+ concurrent NavAgents.

![alt text](img/generation.png "Logo Title Text 1")

### Pathfinding
Once a start and end node is acquired, then a traversal algorithm (located in the Navigation object) can be used to generate a NavPath representing a sequence of nodes leading from the start to finish. The path given is not always perfectly optimized, but some considerations were made to make the algorithm fast and space efficient. Once the NavPath is returned to the desired NavAgent, the NavAgent can provide the "next target" for the associated object and manipulate this path as the object moves. In theory, if the target does not move, a NavAgent seeking the target should only need to compute the path to the target once, and them simply move from node to node based on the instructions provided by the NavAgent. In reality, due to the unpredictability of the ingame physics engine the path can become out of sync with the object, thus the path should be recalculated on a reasonable interval to prevent objects following "bad" paths.

![alt text](img/navigation.png "Logo Title Text 1")

### Computation
While this whole process runs very fast, in order to put an upper limit on cycle time consumed by navigation, the NavComputeObject is used to handle all navigation path computation. This object stores navigation jobs in a FIFO queue as CompletableFutures (a custom data type used to "asynchronously" run a function and capture the return value in a callback). Every cycle, the NavComputeObject will only compute a set maximum number of navigation paths, thus providing an upper limit on the computation time per cycle. As a result, if the number of navigation jobs is very large, the navigations will simply take longer to "complete" rather than impacting the fps of the game. Based on all performance considerations, on any modern computer several hundred navigation agents can simultaneously navigate to different targets (recomputing their paths at a reasonable rate) with a performance hit of only ~2ms per cycle. Shown below are snippets of a NavAgent enqueuing a navigation computation and the NavComputeObject:
#### NavAgent
```cs
CompletableFuture<Navigation, NavPath> navFuture = CompletableFuture<Navigation, NavPath>
	.run(navigation => navigation.compute())
	.with(Navigation.track(closestNode, targetNode))
	.then(aquirePathCallback);
			
NavComputeObject.enqueueNavigation(navFuture);
```
#### NavComputeObject
```cs
private static readonly Queue<CompletableFuture<Navigation, NavPath>> navFutureQueue = new Queue<CompletableFuture<Navigation, NavPath>>();
private static int queueLength;

private const int FRAME_COMPUTE_ITERS = 10;

private void Update(){
	Repeatable.invoke(() => {
		CompletableFuture<Navigation, NavPath> navFuture = navFutureQueue.Dequeue();
		navFuture.complete();
	
		queueLength--;
	}, IntMath.min(queueLength, FRAME_COMPUTE_ITERS));
}
```

Random Generation
-----

### Range
Generation in TBD is mainly based on two data types, the Range and the RangeComposition. A range essentially represents an integer set S where S = {x∈ℤ | a≤x≤b, a,b∈ℤ } where a is the minimum of the range and b is the maximum of the range. Then using the RandomGenerator object, we can easily pick some random integer from withing this range. This is particularly useful in cases where a certain value should always fall into some specific range. For example, the number of floors in a building could be represented by the range from 1 to 10, then upon generation we only need to pass this range to a random generator. While ranges are only a simple (but convenient) abstraction above traditional random number generation, their real power is unlocked when combined into a RangeComposition. 
```cs
private static Range FLOORS = new Range(1, 10);
...
int floorsInBuilding = randomGenerator.nextInt(FLOORS);
```

### RangeComposition
A RangeComposition is a combination of ordered and non-overlapping Range objects able to represent any finite set of integers, but in a way that is extremely fast for random access and has a very small memory footprint (useful for seamless random generation at runtime). The RangeComposition object also allows for the dynamic injection of ranges into some composition which will smartly insert and combine overlapping or touching ranges and return the new range. Since the object is meant to represent a mathematical set, it is also possible to take the inverse of a RangeComposition over some universe (represented as a range). This new object serves as a very powerful tool for random generation when there are gaps in the possible values for some value. For example, in a simplified attempt to generate a middle floor of a building, we could compose the ranges of any stairs from the floor below, then take the inverse of that composition over the universe of the buildings exterior walls. From the result we could then chose a random element to be the position of a wall on that floor. This simplified example would ensure that a wall is never placed in a position where it blocks an entrance point from the floor below. Here is the actual algorithm used to decide wall locations for a given floor:
```cs
private LinkedList<Range> generateFloorInterior(int y, LinkedList<Range> prevRampRanges){
	List<int> floorWalls = new List<int>{range.min, range.max};
	Range possibleWallUniverse = new Range(range.min + roomWidth.min, range.max - roomWidth.min);
	RangeComposition excludedSet = new RangeComposition(prevRampRanges);
	int optimisticNumWalls = BuildingGenerator.rand().nextInt(maxRooms, x => Mathf.Pow(x, 2f));
	for(int i = 0; i < optimisticNumWalls; i ++){
		RangeComposition includedSet = excludedSet.inverseRangeComposition(possibleWallUniverse);
		if(includedSet.isEmpty()){
			break;
		}
		int wallX = BuildingGenerator.rand().nextInt(includedSet);
		excludedSet.injectRange(new Range(wallX - roomWidth.min - 1, wallX + roomWidth.min + 1));
		floorWalls.Add(wallX);
	}
...
}
```
In another case, if we know the ranges of the stairs on a floor, then we can easily generate an "optimized" ceiling for that floor. We would only need to compose these ranges, take the inverse over the universe of the building, iterate through the ranges of the new composition, and instantiate a ceiling "block" from the min to the max of each range. This would give us a complete ceiling with the guarantee that there are no overlapping/redundant objects, and thus would give the optimal game performance.


Author
-----

Shane Hickman <srhickman@edu.uwaterloo.ca> Software Engineering student at the University of Waterloo.

