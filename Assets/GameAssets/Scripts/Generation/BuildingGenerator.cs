using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour {

	public GameObject buildings;
	public Transform platformCore, platformEndL, platformEndR, wallCore, wallBack, platformBack, platformRamp, fallThroughPlatform, fallThroughRamp, fallThroughRampShoulder;
	public GameObject player;

	private static readonly RandomGenerator randomGenerator = new RandomGenerator();

	public static LinkedList<Building> buildingData = new LinkedList<Building>();

	private static Range BUILDING_WIDTH;
	private static Range FLOORS;
	private static Range FLOOR_HEIGHT;

	private static int nextXLeft = -25;
	private static int nextXRight = 25;
	private const float MIN_FRINGE_DISTACE = 200f;

	private readonly int[] buildingDistances = {2, 8, 16, 24, 50};

	private const float SQRT_2 = 1.414213562f;

	private static BuildingGenerator singleton;

	public static BuildingGenerator instance(){
		return singleton;
	}

	public static RandomGenerator rand(){
		return randomGenerator;
	}

	private void Start(){
		singleton = this;
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
			buildingData.AddFirst(new Building(nextXLeft - width,
				width,
				randomGenerator.nextInt(FLOOR_HEIGHT),
				randomGenerator.nextInt(FLOORS),
				4,
				randomGenerator.nextDouble(),
				buildings.transform
			));
			if(buildingData.First.Next != null){
				NavNode.join(buildingData.First.Value.baseNodeR, buildingData.First.Next.Value.baseNodeL);
			}
			nextXLeft -= width + randomGenerator.randomPreset(buildingDistances);
		}
		if(fringeDistance(nextXRight) < MIN_FRINGE_DISTACE){
			int width = randomGenerator.nextInt(BUILDING_WIDTH);
			buildingData.AddLast(new Building(nextXRight,
				width,
				randomGenerator.nextInt(FLOOR_HEIGHT),
				randomGenerator.nextInt(FLOORS),
				4,
				randomGenerator.nextDouble(),
				buildings.transform
			));
			if(buildingData.Last.Previous != null){
				NavNode.join(buildingData.Last.Value.baseNodeL, buildingData.Last.Previous.Value.baseNodeR);
			}
			nextXRight += width + randomGenerator.randomPreset(buildingDistances);
		}
	}

	public static void createPlatform(int x, int y, int width, GameObject parent){
		GameObject platform = new GameObject("_platform");
		platform.transform.SetParent(parent.transform);
		Transform core = Instantiate(singleton.platformCore, new Vector3(0, 0, 0), Quaternion.identity, platform.transform);
		core.localScale += new Vector3(width - 1, 0, 0);
		float offset = width * CoordinateSystem.BLOCK_WIDTH / 2 - 0.0365f;
		Instantiate(singleton.platformEndL, new Vector3(-offset, 0, 0), Quaternion.identity, platform.transform);
		Instantiate(singleton.platformEndR, new Vector3(offset, 0, 0), Quaternion.identity, platform.transform);
		platform.transform.SetPositionAndRotation(CoordinateSystem.toReal(stretch(x, width), y), Quaternion.identity);
	}

	public static int createFallThroughRamp(int x, int y,  int width, int height, GameObject parent){
		Transform ramp = Instantiate(singleton.fallThroughRamp, new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
		ramp.localScale += new Vector3(Mathf.Sqrt(width * width + height * height) - 1, 0, 0);
		float offset = CoordinateSystem.BLOCK_WIDTH / SQRT_2 / 2;
		Vector3 position = CoordinateSystem.toReal(stretch(x, width), stretch(y + 1, height));
		int rotSign = randomGenerator.nextSign();
		position += new Vector3(rotSign * offset, -offset, 0);
		ramp.transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, rotSign * 45));
		createGenericPlatform(x + (rotSign > 0 ? -2 : width), 0.01f + y, 2, singleton.fallThroughRampShoulder, parent);
		return rotSign;
	}

	public static void createGenericPlatform(float x, float y, int width, Transform prefab, GameObject parent){
		Transform platform = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
		platform.localScale += new Vector3(width - 1, 0, 0);
		platform.transform.SetPositionAndRotation(CoordinateSystem.toReal(stretch(x, width), y), Quaternion.identity);
	}

	public static void createGenericWall(int x, int y, int height, Transform prefab, GameObject parent){
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
