using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Assertions;

public class BuildingGerator : MonoBehaviour {

	public GameObject buildings;
	public Transform platformCore, platformEndL, platformEndR, wallCore, wallBack, platformBack, platformRamp, fallThroughPlatform, fallThroughRamp, fallThroughRampShoulder;
	public GameObject player;

	private readonly RandomGenerator randomGenerator = new RandomGenerator();

	public static LinkedList<Building> buildingData = new LinkedList<Building>();

	private static Range BUILDING_WIDTH;
	private static Range FLOORS;
	private static Range FLOOR_HEIGHT;

	private static int nextXLeft = -25;
	private static int nextXRight = 25;
	private const float MIN_FRINGE_DISTACE = 200f;

	private readonly int[] buildingDistances = {2, 8, 16, 24, 50};

	private const float SQRT_2 = 1.414213562f;

	private void Start(){
		BUILDING_WIDTH = new Range(35, 80);
		FLOORS = new Range(2, 10);
		FLOOR_HEIGHT = new Range(Player.JUMP_HEIGHT, Player.JUMP_HEIGHT + 4);
		
		autoFillBuildings();
	}

	private void Update(){
		autoFillBuildings();
	}

	private void autoFillBuildings(){
		if(fringeDistance(nextXLeft) < MIN_FRINGE_DISTACE){
			int width = randomGenerator.nextInt(BUILDING_WIDTH);
			buildingData.AddFirst(generateBuilding(new Building(nextXLeft - width,
				width,
				randomGenerator.nextInt(FLOOR_HEIGHT),
				randomGenerator.nextInt(FLOORS),
				randomGenerator.nextDouble()
			)));
			if(buildingData.First.Next != null){
				NavNode.join(buildingData.First.Value.baseNodeR, buildingData.First.Next.Value.baseNodeL);
			}
			nextXLeft -= width + randomGenerator.randomPreset(buildingDistances);
		}
		if(fringeDistance(nextXRight) < MIN_FRINGE_DISTACE){
			int width = randomGenerator.nextInt(BUILDING_WIDTH);
			buildingData.AddLast(generateBuilding(new Building(nextXRight,
				width,
				randomGenerator.nextInt(FLOOR_HEIGHT),
				randomGenerator.nextInt(FLOORS),
				randomGenerator.nextDouble()
			)));
			if(buildingData.Last.Previous != null){
				NavNode.join(buildingData.Last.Value.baseNodeL, buildingData.Last.Previous.Value.baseNodeR);
			}
			nextXRight += width + randomGenerator.randomPreset(buildingDistances);
		}
	}

	private Building generateBuilding(Building building){
		building.parent = new GameObject("_building");
		building.parent.transform.SetParent(buildings.transform);
		createGenericWall(building.x, 1, building.floorHeight * building.floors, wallBack, building.parent);
		createGenericWall(building.x + building.width - 1, 1, building.floorHeight * building.floors, wallBack, building.parent);
		generateBase(building);
		for(int i = 1; i < building.floors - 1; i ++){
			//generateFloor(building, i * building.floorHeight);
		}
		generateTopFloor(building, (building.floors - 1) * building.floorHeight);
		building.connectNavNodes();

		return building;
	}

	private void generateBase(Building building){
		generateCeilingOld(building, 0);
		building.baseNodeL = generateWallWithDoor(building, building.range.min, 0, false);
		building.baseNodeR = generateWallWithDoor(building, building.range.max, 0, false);
	}

	private List<Range> generateFloor(Building building, int y, LinkedList<Range> prevRampRanges){
		generateCeilingOld(building, y);
		generateFloorWall(building, building.range.min, y);
		generateFloorWall(building, building.range.max, y);
		
		return null;
	}

//	LinkedList<Range> generateNextRampRanges(Building building, LinkedList<Range> prevRampRanges){
//		int minRoomWidth = building.roomWidth.min;
//		Range possibleWallUniverse = new Range(building.range.min + minRoomWidth, building.roomWidth.max - minRoomWidth);
//		RangeComposition excludedSet = new RangeComposition(prevRampRanges);
//		int optimisticNumWalls = randomGenerator.nextInt(0, 4);
//		for(int i = 0; i < optimisticNumWalls; i ++){
//			RangeComposition includedSet = excludedSet.inverseRangeComposition(possibleWallUniverse);
//			if(includedSet.isEmpty()){
//				break;
//			}
//			int wallX = randomGenerator.nextInt(includedSet);
//			excludedSet.addRange(new Range(wallX - minRoomWidth, wallX + minRoomWidth));
//		}
//	}

//	private Range createRampWithinRange(Building building, Range range, ){
//		
//	}

	private void generateTopFloor(Building building, int y){
		Range rampRange = generateCeilingOld(building, y);
		Range minRange = new Range(rampRange.min - 1, rampRange.max + 1);
		Range topRange = randomGenerator.rangeBetween(building.range, minRange, 2);
		generateFloorWall(building, building.range.min, y);
		generateFloorWall(building, building.range.max, y);
		int topY = y + building.floorHeight;
		generateWallWithDoor(building, topRange.min, topY, true);
		generateWallWithDoor(building, topRange.max, topY, true);
		createPlatform(topRange.min, y + building.floorHeight * 2, topRange.size(), building.parent);
	}

	private void generateFloorWall(Building building, int x, int y){
		int wallY = y + 1;
		int wallHeight = building.floorHeight - 1;
		if(randomGenerator.nextBool(building.windowFreq)){
			int windowHeight = Player.HEIGHT;
			int bottomHeight = (building.floorHeight - windowHeight) / 2;
			createGenericWall(x, wallY, bottomHeight, wallCore, building.parent);
			createGenericWall(x, wallY + bottomHeight + windowHeight, wallHeight - bottomHeight - windowHeight, wallCore, building.parent);
		}
		else{
			createGenericWall(x, wallY, wallHeight, wallCore, building.parent);
		}
	}

	private NavNode generateWallWithDoor(Building building, int x, int y, bool generateBacking){
		createGenericWall(x, Player.HEIGHT + 1 + y, building.floorHeight - Player.HEIGHT - 1, wallCore, building.parent);
		if(generateBacking){
			createGenericWall(x, y, building.floorHeight, wallBack, building.parent);
		}
		return building.createNavNode(x, y, NavNode.Type.DOOR);
	}
	
	private Range generateCeilingOld(Building building, int y){
		int gapXStart = randomGenerator.nextInt(building.x + 2, building.x + building.width - building.rampWidth - 1);
		int gapXEnd = gapXStart + building.rampWidth - 1;
		int floorXEnd = building.x + building.width - 1;
		int ceilingY = y + building.floorHeight;
		createPlatform(building.x, ceilingY, gapXStart - building.x, building.parent);
		createPlatform(gapXEnd + 1, ceilingY, floorXEnd - gapXEnd, building.parent);
		createGenericPlatform(building.x, ceilingY, building.width, platformBack, building.parent);
		
		generateRamp(building, new Range(gapXStart, gapXEnd), y);

		return new Range(0, 0);
	}

	private void generateCeiling(Building building, List<Range> rampRanges, int y){
		foreach(Range range in rampRanges){
			generateRamp(building, range, y);
		}
	}

	private void generateRamp(Building building, Range range, int y){
		createGenericPlatform(range.min, y + building.floorHeight, building.rampWidth, fallThroughPlatform, building.parent);
		int rotSign = createFallThroughRamp(range.min, y, building.rampWidth, building.floorHeight, building.parent);
		building.createRampNavNodes(range.min, range.max + building.rampWidth - 1, y, rotSign > 0);
	}

	private void createPlatform(int x, int y, int width, GameObject parent){
		GameObject platform = new GameObject("_platform");
		platform.transform.SetParent(parent.transform);
		Transform core = Instantiate(platformCore, new Vector3(0, 0, 0), Quaternion.identity, platform.transform);
		core.localScale += new Vector3(width - 1, 0, 0);
		float offset = width * CoordinateSystem.BLOCK_WIDTH / 2 - 0.0365f;
		Instantiate(platformEndL, new Vector3(-offset, 0, 0), Quaternion.identity, platform.transform);
		Instantiate(platformEndR, new Vector3(offset, 0, 0), Quaternion.identity, platform.transform);
		platform.transform.SetPositionAndRotation(CoordinateSystem.toReal(stretch(x, width), y), Quaternion.identity);
	}

	private int createFallThroughRamp(int x, int y,  int width, int height, GameObject parent){
		Transform ramp = Instantiate(fallThroughRamp, new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
		ramp.localScale += new Vector3(Mathf.Sqrt(width * width + height * height) - 1, 0, 0);
		float offset = CoordinateSystem.BLOCK_WIDTH / SQRT_2 / 2;
		Vector3 position = CoordinateSystem.toReal(stretch(x, width), stretch(y + 1, height));
		int rotSign = randomGenerator.nextSign();
		position += new Vector3(rotSign * offset, -offset, 0);
		ramp.transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, rotSign * 45));
		createGenericPlatform(x + (rotSign > 0 ? -2 : width), 0.01f + y, 2, fallThroughRampShoulder, parent);
		return rotSign;
	}

	private static void createGenericPlatform(float x, float y, int width, Transform prefab, GameObject parent){
		Transform platform = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
		platform.localScale += new Vector3(width - 1, 0, 0);
		platform.transform.SetPositionAndRotation(CoordinateSystem.toReal(stretch(x, width), y), Quaternion.identity);
	}

	private static void createGenericWall(int x, int y, int height, Transform prefab, GameObject parent){
		Transform wall = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
		wall.localScale += new Vector3(0, height - 1, 0);
		wall.transform.SetPositionAndRotation(CoordinateSystem.toReal(x, stretch(y, height)), Quaternion.identity);
	}

	private static float stretch(float x, int size){
		return x + size / 2f - 0.5f;
	}

	private float fringeDistance(int fringeX){
		return Mathf.Abs(player.transform.position.x - CoordinateSystem.toReal(fringeX));
	}

}
