using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour {

	public Transform zombieContainer;
	public Transform zombiePrefab;

	private const int MAX_SPAWN_DISTANCE = 250;
	private const int MIN_SPAWN_DISTANCE = 40;
	private const int SPAWN_HEIGHT_ABOVE_FLOOR = 3;
	private const float BASE_SPAWN_FREQ = 0.01f;
	private const float FULL_FREQ_DAY = 30.0f;
	private const int HARD_SPAWN_CAP = 99;
	
	private LinkedListNode<Building> closestBuilding;
	private static Interval spawnInterval;
	private static readonly List<Zombie> zombies = new List<Zombie>();
	private GameObject playerGO;

	private static TimeFunction softSpawnCap;
	private static TimeFunction minimumSpawned;
	
	private readonly RandomGenerator randomGenerator = new RandomGenerator();

	private void Awake(){
		spawnInterval = new Interval(trySpawningZombie, 0.1f);
		playerGO = GameObject.Find("Player");
		
		softSpawnCap = new TimeFunction(time => 50 + time.day * 8);
		minimumSpawned = new TimeFunction(time => 10 + time.day * 4);
	}

	private void Update(){
		spawnInterval.update();
	}

	private void trySpawningZombie(){
		int spawnedZombies = zombies.Count;
		int spawnCap = IntMath.min(softSpawnCap.eval(), HARD_SPAWN_CAP);
		if(spawnedZombies > spawnCap){
			return;
		}
		if(spawnedZombies < minimumSpawned.eval()){
			spawnZombie();
		}
		float spawnChance = BASE_SPAWN_FREQ + TimeController.day / FULL_FREQ_DAY;
		if(randomGenerator.nextBool(spawnChance)){
			spawnZombie();
		}
	}

	private void spawnZombie(){
		int playerX = (int)CoordinateSystem.fromReal(playerGO.transform.position.x);
		RangeComposition noSpawnRange =
			new RangeComposition(Range.surrounding(playerX, MIN_SPAWN_DISTANCE));
		Range spawnUniverse = Range.surrounding(playerX, MAX_SPAWN_DISTANCE);
		RangeComposition spawnRange = noSpawnRange.inverseRangeComposition(spawnUniverse);
		int x = randomGenerator.nextInt(spawnRange);

		Building building = getClosestBuilding(x);

		int y = SPAWN_HEIGHT_ABOVE_FLOOR;
		if(building.range.contains(x)){ //Zombie inside building
			Floor floor = randomGenerator.randomPreset(building.floors);
			y += floor.y;
			x = closestBuilding.Value.getTraversibleXOnFloor(floor, randomGenerator);
		}
		spawnZombie(x, y);
	}

	private void spawnZombie(int x, int y){
		Transform zombie = Instantiate(zombiePrefab, CoordinateSystem.toReal(x, y), Quaternion.identity, zombieContainer.transform);
		zombies.Add(zombie.GetComponent<Zombie>());
	}

	public static void removeZombie(Zombie zombie){
		zombies.Remove(zombie);
	}
	
	private Building getClosestBuilding(int x){
		if(closestBuilding == null){
			closestBuilding = BuildingGenerator.buildingData.First;
		}
		while(closestBuilding.Previous != null && x < closestBuilding.Value.x - 1){
			closestBuilding = closestBuilding.Previous;
		}
		while(closestBuilding.Next != null && x > closestBuilding.Next.Value.x - 1){
			closestBuilding = closestBuilding.Next;
		}
		return closestBuilding.Value;
	}
	
}
