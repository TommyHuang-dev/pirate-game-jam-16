using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public int enemiesToSpawn = 0;
    public int eliteEnemiesToSpawn = 0;
    public bool isBossRoom = false;
    public GameObject[] _enemyPrefabs;
    public GameObject[] _bossPrefabs;
    private LevelLoader.SceneType roomType;

    public int enemyCount;

    // Constants
    private int eliteEnemyHP = 150;
    private float eliteEnemyScale = 1.4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //_levelLoader = FindFirstObjectByType<LevelLoader>();
        roomType = (LevelLoader.SceneType)SaveData.Instance.data.currentRoomType;
        CalcEnemiesToSpawn();
        
        StartCoroutine(SpawnEnemies());
    }

    private void CalcEnemiesToSpawn() {
        int roomNum = SaveData.Instance.data.currentRoomNumber;
        Debug.Log("scenetype " + roomType);

        // Idk
        switch(roomType) {
            case (LevelLoader.SceneType.BasicEnemy):
                enemiesToSpawn = (roomNum > 8) ? roomNum : roomNum + 3; // Have some sense of scaling in enemy rooms
                return;
            case (LevelLoader.SceneType.EliteEnemy):
                enemiesToSpawn = 3;
                eliteEnemiesToSpawn = (int)(roomNum / 2) + 1;
                return;
            case (LevelLoader.SceneType.Boss):
                isBossRoom = true;
                return;
            default:
                return;
        }
    }
    private IEnumerator SpawnEnemies() {
        if (isBossRoom) {
            SpawnBoss();
        }

        while (eliteEnemiesToSpawn > 0) {
            if (Random.Range(0f, 1f) > 0.5f) {
                StartCoroutine(SpawnFromTop(spawnElite: true));
            } else {
                StartCoroutine(SpawnFromBottom(spawnElite: true));
            }
        }

        while (enemiesToSpawn > 0) {
            if (Random.Range(0f, 1f) > 0.5f) {
                StartCoroutine(SpawnFromTop());
            } else {
                StartCoroutine(SpawnFromBottom());
            }
        }
        yield return new WaitForSeconds(1f);
    }

    private void SpawnBoss() {
        Vector3 spawnPos = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f), transform.position.y + Random.Range(-0.5f, 0.5f), -2);
        // Get the boss progression by (currentRoomNum / 4) - 1, where the prefabs are stored in order in an array
        GameObject boss = Instantiate(_bossPrefabs[(SaveData.Instance.data.currentRoomNumber / 4) - 1], spawnPos, Quaternion.identity);
    }

    private IEnumerator SpawnFromTop(bool spawnElite = false) {
        Vector3 spawnPos = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f), transform.position.y + Random.Range(-0.5f, 0.5f), -2);
        int toSpawn = spawnElite ? Random.Range(1, eliteEnemiesToSpawn) : Random.Range(1, enemiesToSpawn);
        for (int i = 0; i < toSpawn; i++) {
            GameObject enemy = Instantiate(_enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)], spawnPos, Quaternion.identity);
            
            if (spawnElite) {
                Debug.Log("spawn elite");
                eliteEnemiesToSpawn--;
                enemy.transform.localScale = Vector2.one * eliteEnemyScale;
                enemy.GetComponent<Enemy>().health = eliteEnemyHP;
            } else {
                enemiesToSpawn--;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator SpawnFromBottom(bool spawnElite = false) {
        // Offset y by 14 to spawn from bottom (yay hardcoding)
        Vector3 spawnPos = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f), (transform.position.y - 14) + Random.Range(-0.5f, 0.5f), -2);
        int toSpawn = spawnElite ? Random.Range(1, eliteEnemiesToSpawn) : Random.Range(1, enemiesToSpawn);
        // Spawn a random number of enemies
        for (int i = 0; i < toSpawn; i++) {
            GameObject enemy = Instantiate(_enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)], spawnPos, Quaternion.identity);
            
            if (spawnElite) {
                eliteEnemiesToSpawn--;
                enemy.transform.localScale = Vector2.one * eliteEnemyScale;
                enemy.GetComponent<Enemy>().health = eliteEnemyHP;
            } else {
                enemiesToSpawn--;
            }
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(1f);
    }
}
