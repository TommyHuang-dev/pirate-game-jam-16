using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public int enemiesToSpawn;
    public GameObject[] _enemyPrefabs;
    private LevelLoader.SceneType roomType;
    //private LevelLoader _levelLoader;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //_levelLoader = FindFirstObjectByType<LevelLoader>();
        roomType = (LevelLoader.SceneType)SaveData.Instance.data.currentRoomType;
        enemiesToSpawn = CalcEnemiesToSpawn();
        
        Debug.Log("Should spawn " +  enemiesToSpawn);
        StartCoroutine(SpawnEnemies());
    }

    private int CalcEnemiesToSpawn() {
        int roomNum = SaveData.Instance.data.currentRoomNumber;
        Debug.Log("scenetype " + roomType);

        // Idk
        switch(roomType) {
            case (LevelLoader.SceneType.BasicEnemy):
                return (roomNum > 8) ? roomNum : roomNum + 3; // Have some sense of scaling in enemy rooms
            case (LevelLoader.SceneType.EliteEnemy):
                return (roomNum > 8) ? 2 : 1;
            case (LevelLoader.SceneType.Boss):
                return 1;
            default:
                return 0;
        }
    }
    private IEnumerator SpawnEnemies() {
        
        while (enemiesToSpawn > 0) {
            Vector2 spawnPos = new Vector2(transform.position.x + Random.Range(-0.5f, 0.5f), transform.position.y + Random.Range(-0.5f, 0.5f));
            int toSpawn = Random.Range(1, enemiesToSpawn);
            for (int i = 0; i < toSpawn; i++) {
                GameObject enemy = Instantiate(_enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)], spawnPos, Quaternion.identity);
                enemiesToSpawn--;
                yield return new WaitForSeconds(0.2f);
            }
            
            yield return new WaitForSeconds(1f);
        }
    }

}
