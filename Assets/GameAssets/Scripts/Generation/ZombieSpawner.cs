using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour {

	public Transform zombieContainer;
	public Transform zombiePrefab;

	private const int MAX_SPAWN_DISTANCE = 250;
	private const int MIN_SPAWN_DISTANCE = 40;
	private const int SPAWN_HEIGHT_ABOVE_FLOOR = 3;

	private static readonly Range zombieRange = new Range(40, 100);
	
	private LinkedListNode<Building> closestBuilding;
	private static Interval spawnInterval;
	private static readonly List<Zombie> zombies = new List<Zombie>();
	private GameObject playerGO;
	
	private readonly RandomGenerator randomGenerator = new RandomGenerator();

	private void Awake(){
		spawnInterval = new Interval(trySpawningZombie, 0.1f);
		playerGO = GameObject.Find("Player");
	}

	private void Update(){
		spawnInterval.update();
	}

	private void trySpawningZombie(){
		int spawnedZombies = zombies.Count;
		if(spawnedZombies > zombieRange.max){
			return;
		}
		if(spawnedZombies < zombieRange.min){
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
		Debug.Log(x);

		Building building = getClosestBuilding(x);

		int y = SPAWN_HEIGHT_ABOVE_FLOOR;
		if(building.range.contains(x)){ //Zombie inside building
			Floor floor = randomGenerator.randomPreset(building.floors);
			y += floor.y;

			foreach(int wallX in floor.getWalls()){
				if(wallX == x){
					int prevX = wallX - 1;
					int postX = wallX + 1;
					x = building.range.contains(prevX) ? prevX : postX;
					break;
				}
			}
		}
		Debug.Log(x);
		spawnZombie(x, y);
	}

	private void spawnZombie(int x, int y){
		Transform zombie = Instantiate(zombiePrefab, CoordinateSystem.toReal(x, y), Quaternion.identity, zombieContainer.transform);
		zombies.Add(zombie.GetComponent<Zombie>());
		Debug.Log(zombie.transform.position.x);
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
