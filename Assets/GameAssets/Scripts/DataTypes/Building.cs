using System.Collections.Generic;
using UnityEngine;

public class Building {

	public int x{ get; set; }
	public int width{ get; set; }
	public int floorHeight{ get; set; }
	public int rampWidth{ get; set; }
	public int numfloors{ get; set; }
	public int maxRooms{ get; set; }
	public double windowFreq{ get; set; }
	public Range roomWidth{ get; set; }
	public Range range{ get; set; }
	public GameObject parent{ get; set; }
	public NavNode baseNodeL{ get; set; }
	public NavNode baseNodeR{ get; set; }
	private readonly Floor[] floors;

public Building(int x, int width, int floorHeight, int numfloors, int maxRooms, double windowFreq, Transform buildings){
		this.x = x;
		this.width = width;
		this.floorHeight = floorHeight;
		this.numfloors = numfloors;
		this.maxRooms = maxRooms;
		this.windowFreq = windowFreq;
		rampWidth = floorHeight;
		roomWidth = new Range(rampWidth + 2, width);
		range = new Range(x, x + width - 1);
		floors = new Floor[numfloors + 1];
		for(int i = 0; i < floors.Length; i ++){
			floors[i] = new Floor(i * floorHeight);
		}
		
		parent = new GameObject("_building");
		parent.transform.SetParent(buildings.transform);
		BuildingGenerator.createGenericWall(x, 1, floorHeight * numfloors, BuildingGenerator.instance().wallBack, parent);
		BuildingGenerator.createGenericWall(x + width - 1, 1, floorHeight * numfloors, BuildingGenerator.instance().wallBack, parent);
		LinkedList<Range> prevRampRanges = new LinkedList<Range>();
		prevRampRanges.AddFirst(generateBase());
		for(int i = 1; i < numfloors - 1; i ++){
			prevRampRanges = generateFloor(i * floorHeight, prevRampRanges);
		}
		generateTopFloor((numfloors - 1) * floorHeight);
		connectNavNodes();
	}

	public NavNode createNavNode(int x, int y, NavNode.Type type){
		return createNavNode(x, y, type, getFloor(y));
	}

	private NavNode createNavNode(int x, int y, NavNode.Type type, int floor){
		NavNode newNode = new NavNode(x, y, type);
		floors[floor].addNode(newNode);
		return newNode;
	}

	public void createRampNavNodes(Range rampRange, int y, bool topRight){
		int topY = y + floorHeight;
		NavNode nodeL, nodeR;
		if(topRight){
			nodeL = createNavNode(rampRange.min - 1, y, NavNode.Type.RAMP_BOTTOM);
			nodeR = createNavNode(rampRange.max + 1, topY, NavNode.Type.RAMP_TOP);
		}
		else{
			nodeL = createNavNode(rampRange.min - 1, topY, NavNode.Type.RAMP_TOP);
			nodeR = createNavNode(rampRange.max + 1, y, NavNode.Type.RAMP_BOTTOM);
		}
		NavNode.join(nodeL, nodeR);
	}

	public void connectNavNodes(){
		foreach(Floor floor in floors){
			floor.connectNodes();
		}
	}

	public Floor getClosestFloor(float y){
		int floor = getFloor(y);
		return floors[floor < 0 ? 0 : (floor > numfloors ? numfloors : floor)];
	}

	private int getFloor(int floorY){
		return floorY / floorHeight;
	}

	public int getFloor(float y){
		return (int)(y / floorHeight);
	}

	public int getFloorFromReal(float realY){
		return getFloor(CoordinateSystem.fromReal(realY));
	}

	private Range generateBase(){
		baseNodeL = generateWallWithDoor(range.min, 0, false);
		baseNodeR = generateWallWithDoor(range.max, 0, false);
		return generateLimitCeiling(0);
	}

	private LinkedList<Range> generateFloor(int y, LinkedList<Range> prevRampRanges){
		generateFloorWall(range.min, y);
		generateFloorWall(range.max, y);
		
		BuildingGenerator.createGenericPlatform(x, y + floorHeight, width, BuildingGenerator.instance().platformBack, parent);
		
		return generateFloorInterior(y, prevRampRanges);
	}

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
		
		floorWalls.Sort((a, b) => a.CompareTo(b));
		LinkedList<Range> rampRanges = new LinkedList<Range>();
		LinkedList<Range> rampGapRanges = new LinkedList<Range>();
		for(int i = 0; i < floorWalls.Count - 1; i ++){
			if(i > 0){
				generateInteriorWall(floorWalls[i], y);
			}
			Range roomRange = new Range(floorWalls[i] + 1, floorWalls[i + 1] - 1);
			Range rampRange = generateRoom(roomRange, y);
			rampRanges.AddLast(new Range(rampRange.min - 1, rampRange.max + 1));
			rampGapRanges.AddLast(rampRange);
		}
		floors[getFloor(y)].setWalls(floorWalls);
		
		generateCeiling(y + floorHeight, rampGapRanges);
		
		return rampRanges;
	}

	private Range generateRoom(Range roomRange, int y){
		//Fill room
		return generateRampWithinRange(roomRange, y);
	}

	private Range generateRampWithinRange(Range roomRange, int y){
		Range rampRange = BuildingGenerator.rand().subRange(rampWidth, roomRange);
		generateRamp(rampRange, y);
		return new Range(rampRange.min, rampRange.max);
	}

	private void generateCeiling(int y, LinkedList<Range> rampGapRanges){
		Range universe = new Range(range.min - 1, range.max + 1);
		RangeComposition rampGapSet = new RangeComposition(rampGapRanges);
		RangeComposition platformSet = rampGapSet.inverseRangeComposition(universe);
		foreach(Range platRange in platformSet.getRanges()){
			BuildingGenerator.createPlatform(platRange.min, y, platRange.size(), parent);
		}
	}
	
	private Range generateLimitCeiling(int y){
		int gapXStart = BuildingGenerator.rand().nextInt(x + 2, x + width - rampWidth - 1);
		int gapXEnd = gapXStart + rampWidth - 1;
		int floorXEnd = x + width - 1;
		int ceilingY = y + floorHeight;
		BuildingGenerator.createPlatform(x, ceilingY, gapXStart - x, parent);
		BuildingGenerator.createPlatform(gapXEnd + 1, ceilingY, floorXEnd - gapXEnd, parent);
		BuildingGenerator.createGenericPlatform(x, ceilingY, width, BuildingGenerator.instance().platformBack, parent);
		
		generateRamp(new Range(gapXStart, gapXEnd), y);

		return new Range(gapXStart, gapXEnd);
	}

	private void generateTopFloor(int y){
		Range rampRange = generateLimitCeiling(y);
		Range minRange = new Range(rampRange.min - 1, rampRange.max + 1);
		Range topRange = BuildingGenerator.rand().rangeBetween(range, minRange, 2);
		generateFloorWall(range.min, y);
		generateFloorWall(range.max, y);
		int topY = y + floorHeight;
		generateWallWithDoor(topRange.min, topY, true);
		generateWallWithDoor(topRange.max, topY, true);
		BuildingGenerator.createPlatform(topRange.min, y + floorHeight * 2, topRange.size(), parent);
	}

	private void generateFloorWall(int wallX, int y){
		int wallY = y + 1;
		int wallHeight = floorHeight - 1;
		if(BuildingGenerator.rand().nextBool(windowFreq)){
			int windowHeight = Player.HEIGHT;
			int bottomHeight = (floorHeight - windowHeight) / 2;
			BuildingGenerator.createGenericWall(wallX, wallY, bottomHeight, BuildingGenerator.instance().wallCore, parent);
			BuildingGenerator.createGenericWall(wallX, wallY + bottomHeight + windowHeight, wallHeight - bottomHeight - windowHeight, BuildingGenerator.instance().wallCore, parent);
		}
		else{
			BuildingGenerator.createGenericWall(wallX, wallY, wallHeight, BuildingGenerator.instance().wallCore, parent);
		}
	}

	private void generateInteriorWall(int wallX, int y){
		BuildingGenerator.createGenericWall(wallX, y + 1, floorHeight - 1, BuildingGenerator.instance().wallCore, parent);
	}

	private NavNode generateWallWithDoor(int wallX, int y, bool generateBacking){
		BuildingGenerator.createGenericWall(wallX, Player.HEIGHT + 1 + y, floorHeight - Player.HEIGHT - 1, BuildingGenerator.instance().wallCore, parent);
		if(generateBacking){
			BuildingGenerator.createGenericWall(wallX, y, floorHeight, BuildingGenerator.instance().wallBack, parent);
		}
		return createNavNode(wallX, y, NavNode.Type.DOOR);
	}

	private void generateRamp(Range rampRange, int y){
		BuildingGenerator.createGenericPlatform(rampRange.min, y + floorHeight, rampWidth, BuildingGenerator.instance().fallThroughPlatform, parent);
		int rotSign = BuildingGenerator.createFallThroughRamp(rampRange.min, y, rampWidth, floorHeight, parent);
		createRampNavNodes(rampRange, y, rotSign > 0);
	}

}
