using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public enum spawnTypes{
        spawnSetNumberOfEnemies,
        spawnSetNumberOfWaves,
        spawnUntilTimeRunsOut,
        none,
    }

    public spawnTypes spawnMode;

    private float levelToSpawnAt;

    [SerializeField]
    private GameObject[] spawnPointsLevel1, spawnPointsLevel2, spawnPointsLevel3, spawnPointsBoss;
    public List<GameObject> allEnemies;

    public GameObject enemyObject;
    public float spawnDelay;
    private float spawnTime;
    public float startDelay;
    public bool spawnInGroups;
    [Tooltip("The minimum and maximum amount of enemies to spawn in a group.\nThe values must be between 1 and 5\n(Set the 2 values the same if you want the amount that spawns in each group to stay consistent)")]
    public Vector2 groupSizeRange;
    public  int numberOfEnemiesSpawned; 
    [SerializeField]
    private int numberOfWavesSpawned;

    [Header("SpawnMode Variables")]
    [Tooltip("Assign either the number of enmies to spawn, waves to spawn, or the length of the timer per level in the corresponding indexes -\0 = Lobby \n1 = Level 1 \n2 = Level 2 \n3 = Level 3 \n4 = Boss")]
    public int[] enemiesToSpawnPerPlayer;
    [SerializeField]
    private int numberOfEnemiesToSpawn;
    [SerializeField]
    private int numberOfWavesToSpawn;
    [SerializeField]
    private float timer;
    [SerializeField]
    private bool spawnInfiniteEnemies;

    [SerializeField]
    private PauseMenu pauseMenu;
    [SerializeField]
    private PlayerManager playerManager;
    [SerializeField]
    private BossController boss;

    void Start()
    {
        allEnemies = new List<GameObject>();
        if (spawnInfiniteEnemies) {
            levelToSpawnAt = 1;
            StartCoroutine(SpawnEndlessEnemies());
        }
    }

    private void Update() {
        timer -= Time.deltaTime;

        if(groupSizeRange.x < 1) {
            groupSizeRange.x = 1;
        }
        if(groupSizeRange.y > 5) {
            groupSizeRange.y = 5;
        }
    }

    public void PrepareToSpawn(int level, int numberOfPlayers) {
        numberOfEnemiesSpawned = 0;
        enemiesToSpawnPerPlayer[level] *= numberOfPlayers;
        numberOfEnemiesToSpawn = enemiesToSpawnPerPlayer[level];
        numberOfWavesToSpawn = enemiesToSpawnPerPlayer[level];
        timer = enemiesToSpawnPerPlayer[level];
        levelToSpawnAt = level;
        spawnTime = 0;
    }

    public int TotalEnemiesInLevel() {
        return numberOfEnemiesToSpawn;
    }

    public IEnumerator SpawnEnemies() {
/*        if (pauseMenu.IsTheGamePaused()) {
            yield return null;
        }*/
        // Wait for a short delay before the spawning begins
        while(spawnTime < startDelay) {
            if (!pauseMenu.IsTheGamePaused()) {
                spawnTime += Time.deltaTime;
            }
            yield return null;
        }
        // Check which spawn mode is selected, and spawn the enemies accordingly
        switch (spawnMode) {
            // Spawns a specific amount of enemies in the level
            case (spawnTypes.spawnSetNumberOfEnemies):
                for (int i = 0; i < numberOfEnemiesToSpawn; i++) {
                    if(numberOfEnemiesSpawned >= numberOfEnemiesToSpawn) {
                        yield break;
                    }
                    SpawnEnemy();
                    yield return new WaitForSeconds(spawnDelay);
                }
                break;
            // Spawns a specific amount of waves of enemies (useful when spawning in groups)
            case(spawnTypes.spawnSetNumberOfWaves):
                for (int i = 0; i < numberOfWavesToSpawn; i++) {
                    SpawnEnemy();
                    if (!pauseMenu.IsTheGamePaused()) {
                        numberOfWavesSpawned++;
                    }
                    yield return new WaitForSeconds(spawnDelay);
                }
                break;
            // Keep spawning enemies until a timer runs out
            case (spawnTypes.spawnUntilTimeRunsOut):
                // Used a large for loop because it needs to run until the timer runs out, and then break
                for (int i = 0; i < 100; i++) {
                    if (timer <= spawnDelay) {
                        yield return null;
                    } else {
                        SpawnEnemy();
                        yield return new WaitForSeconds(spawnDelay);
                    }
                }
            break;
        }
    }

    public IEnumerator SpawnEndlessEnemies() {
        if (pauseMenu.IsTheGamePaused()) {
            yield return null;
        }
        // Wait for a short delay before the spawning begins
        while (spawnTime < startDelay) {
            if (!pauseMenu.IsTheGamePaused()) {
                spawnTime += Time.deltaTime;
            }
            yield return null;
        }
        // Check which spawn mode is selected, and spawn the enemies accordingly
        while (spawnInfiniteEnemies) {
            if(playerManager.numberOfPlayers > 0) {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnEnemy() {
        if (pauseMenu.IsTheGamePaused() || boss.isDead) {
            return;
        }
        if (!spawnInGroups) {
            // Select one of the spawn points in the level at random to place the enemies
            GameObject newEnemy = Instantiate(enemyObject, GetRandomSpawnPoint().position, Quaternion.identity);
            numberOfEnemiesSpawned++;
            allEnemies.Add(newEnemy);
        } else {
            // Generate a random amount of enemies to spawn in one group
            int amountToSpawn = Random.Range((int)groupSizeRange.x, (int)groupSizeRange.y);
            Transform newSpawnPoint = GetRandomSpawnPoint();
            for (int i = 0; i < amountToSpawn; i++) {
                // If the game has already spawned the desired amount of enemies, break out of the loop
                if(numberOfEnemiesSpawned >= numberOfEnemiesToSpawn) {
                    return;
                }

                GameObject newEnemy = Instantiate(enemyObject, newSpawnPoint.position, Quaternion.identity);
                numberOfEnemiesSpawned++;
                allEnemies.Add(newEnemy);
            }

        }

    }

    Transform GetRandomSpawnPoint() {
        int spawnPoint;
        if(levelToSpawnAt == 1) {
            spawnPoint = Random.Range(0, spawnPointsLevel1.Length);
            return spawnPointsLevel1[spawnPoint].transform;
        } else if(levelToSpawnAt == 2) {
            spawnPoint = Random.Range(0, spawnPointsLevel2.Length);
            return spawnPointsLevel2[spawnPoint].transform;
        } else if(levelToSpawnAt == 3) {
            spawnPoint = Random.Range(0, spawnPointsLevel3.Length);
            return spawnPointsLevel3[spawnPoint].transform;
        } else if (levelToSpawnAt == 4) {
            spawnPoint = Random.Range(0, spawnPointsBoss.Length);
            return spawnPointsBoss[spawnPoint].transform;
        }
        return transform;
    }
}
