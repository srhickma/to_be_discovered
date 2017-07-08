using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGerator : MonoBehaviour {

	public GameObject buildings;
	public Transform platformCore, platformEndL, platformEndR, wallCore, wallBack, platformBack, platformRamp, fallThroughPlatform, fallThroughRamp;
	public GameObject player;

	RandomGenerator randomGenerator = new RandomGenerator();

	public static LinkedList<Building> buildingData = new LinkedList<Building>();

	private static Range BUILDING_WIDTH;
	private static Range FLOORS;
	private static Range FLOOR_HEIGHT;

	private static int nextXLeft = -25;
	private static int nextXRight = 25;
	private static float MIN_FRINGE_DISTACE = 200f;

	private int[] buildingDistances = {2, 8, 16, 24, 50};

	private float SQRT_2 = 1.414213562f;

	void Start(){
		BUILDING_WIDTH = new Range(35, 80);
		FLOORS = new Range(2, 10);
		FLOOR_HEIGHT = new Range(Player.JUMP_HEIGHT, Player.JUMP_HEIGHT + 4);

		autoFillBuildings();
	}

	void Update(){
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

	Building generateBuilding(Building building){
		building.parent = new GameObject("_building");
		building.parent.transform.SetParent(buildings.transform);
		createGenericWall(building.x, 1, building.floorHeight * building.floors, wallBack, building.parent);
		createGenericWall(building.x + building.width - 1, 1, building.floorHeight * building.floors, wallBack, building.parent);
		generateBase(building);
		for(int i = 1; i < building.floors - 1; i ++){
			generateFloor(building, i * building.floorHeight);
		}
		generateTopFloor(building, (building.floors - 1) * building.floorHeight);
		building.connectNavNodes();

		return building;
	}

	void generateBase(Building building){
		generateCeiling(building, 0);
		building.baseNodeL = generateWallWithDoor(building, building.range.min, 0, false);
		building.baseNodeR = generateWallWithDoor(building, building.range.max, 0, false);
	}

	void generateFloor(Building building, int y){
		generateCeiling(building, y);
		generateFloorWall(building, building.range.min, y);
		generateFloorWall(building, building.range.max, y);
	}

	void generateTopFloor(Building building, int y){
		Range rampRange = generateCeiling(building, y);
		Range minRange = new Range(rampRange.min - 1, rampRange.max + 1);
		Range topRange = randomGenerator.rangeBetween(building.range, minRange, 2);
		generateFloorWall(building, building.range.min, y);
		generateFloorWall(building, building.range.max, y);
		int topY = y + building.floorHeight;
		generateWallWithDoor(building, topRange.min, topY, true);
		generateWallWithDoor(building, topRange.max, topY, true);
		createPlatform(topRange.min, y + building.floorHeight * 2, topRange.size(), building.parent);
		building.createNavNode(building.range.min, topY, NavNode.Type.WALL);
		building.createNavNode(building.range.max, topY, NavNode.Type.WALL);
	}

	void generateFloorWall(Building building, int x, int y){
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
		building.createNavNode(x, y, NavNode.Type.WALL);
	}

	NavNode generateWallWithDoor(Building building, int x, int y, bool generateBacking){
		createGenericWall(x, Player.HEIGHT + 1 + y, building.floorHeight - Player.HEIGHT - 1, wallCore, building.parent);
		if(generateBacking){
			createGenericWall(x, y, building.floorHeight, wallBack, building.parent);
		}
		return building.createNavNode(x, y, NavNode.Type.DOOR);
	}

	Range generateCeiling(Building building, int y){
		int gapWidth = building.floorHeight;
		int gapXStart = randomGenerator.nextInt(building.x + 1, building.x + building.width - gapWidth);
		int gapXEnd = gapXStart + gapWidth - 1;
		int floorXEnd = building.x + building.width - 1;
		int ceilingY = y + building.floorHeight;
		createPlatform(building.x, ceilingY, gapXStart - building.x, building.parent);
		createPlatform(gapXEnd + 1, ceilingY, floorXEnd - gapXEnd, building.parent);

		createGenericPlatform(gapXStart, ceilingY, gapWidth, fallThroughPlatform, building.parent);
		int rotSign = createFallThroughRamp(gapXStart, y, gapWidth, building.floorHeight, building.parent);
		building.createRampNavNodes(gapXStart, gapXEnd, y, rotSign > 0);

		createGenericPlatform(building.x, ceilingY, building.width, platformBack, building.parent);

		return new Range(gapXStart, gapXEnd);
	}

	void createPlatform(int x, int y, int width, GameObject parent){
		GameObject platform = new GameObject("_platform");
		platform.transform.SetParent(parent.transform);
		Transform core = Instantiate(platformCore, new Vector3(0, 0, 0), Quaternion.identity, platform.transform) as Transform;
		core.localScale += new Vector3(width - 1, 0, 0);
		float offset = width * CoordinateSystem.BLOCK_WIDTH / 2 - 0.0365f;
		Instantiate(platformEndL, new Vector3(-offset, 0, 0), Quaternion.identity, platform.transform);
		Instantiate(platformEndR, new Vector3(offset, 0, 0), Quaternion.identity, platform.transform);
		platform.transform.SetPositionAndRotation(CoordinateSystem.toReal(stretch(x, width), y), Quaternion.identity);
	}

	int createFallThroughRamp(int x, int y,  int width, int height, GameObject parent){
		Transform ramp = Instantiate(fallThroughRamp, new Vector3(0, 0, 0), Quaternion.identity, parent.transform) as Transform;
		ramp.localScale += new Vector3(Mathf.Sqrt(width * width + height * height) - 1, 0, 0);
		float offset = CoordinateSystem.BLOCK_WIDTH / SQRT_2 / 2;
		Vector3 position = CoordinateSystem.toReal(stretch(x, width), stretch(y + 1, height));
		int rotSign = randomGenerator.nextSign();
		position += new Vector3(rotSign * offset, -offset, 0);
		ramp.transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, rotSign * 45));
		return rotSign;
	}

	void createGenericPlatform(int x, int y, int width, Transform prefab, GameObject parent){
		Transform platform = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, parent.transform) as Transform;
		platform.localScale += new Vector3(width - 1, 0, 0);
		platform.transform.SetPositionAndRotation(CoordinateSystem.toReal(stretch(x, width), y), Quaternion.identity);
	}

	void createGenericWall(int x, int y, int height, Transform prefab, GameObject parent){
		Transform wall = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, parent.transform) as Transform;
		wall.localScale += new Vector3(0, height - 1, 0);
		wall.transform.SetPositionAndRotation(CoordinateSystem.toReal(x, stretch(y, height)), Quaternion.identity);
	}

	float stretch(int x, int size){
		return x + size / 2f - 0.5f;
	}

	private float fringeDistance(int fringeX){
		return Mathf.Abs(player.transform.position.x - CoordinateSystem.toReal(fringeX));
	}

}
