using System.Linq;
using UnityEngine;
using TMPro;
using System;

public class Capillary : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int currRoomNum;
    private LevelLoader levelLoader;
    private SpawnManager spawnManager;
    private EventFactory eventFactory;
    private int enemyCount;
    
    [SerializeField] private Capillary otherCapillary; // Communication b/w the two exit capillaries so they don't give players duplicate options
    public LevelLoader.SceneType? queuedScene = null; // The next scene if the player chooses this capillary
    [SerializeField] public string queuedSceneDebug;
    [SerializeField] private TextMeshProUGUI roomText1;
    [SerializeField] private TextMeshProUGUI roomText2;

    [SerializeField] private Capillary exitCap1;
    [SerializeField] private Capillary exitCap2;
    private int invalidRoomType; // If the other capillary already has a queued scene type, that one is invalid
    void Start()
    {   
        levelLoader = FindFirstObjectByType<LevelLoader>();
        spawnManager = FindFirstObjectByType<SpawnManager>();
        enemyCount = (spawnManager == null) ? 0 : spawnManager.enemyCount;


        exitCap1 = GameObject.Find("ExitCapil1").GetComponent<Capillary>();
        exitCap2 = GameObject.Find("ExitCapil2").GetComponent<Capillary>();
        // Avoid duplicate rooms in both capillaries
        currRoomNum = SaveData.Instance.data.currentRoomNumber;
        if (otherCapillary.queuedScene == null) {
            invalidRoomType = -1;
        } else {
            invalidRoomType = (int) otherCapillary.queuedScene;
        }

        // Generate a random event based on the current room number.

        if ((currRoomNum + 1) % 4 != 0 || currRoomNum == 0) {
            var range = Enumerable.Range(2, 3).Where(i => i != invalidRoomType);
            var rand = new System.Random();

            int index = rand.Next(0, range.Count());
            var roomRNG = range.ElementAt(index);
            queuedScene = (LevelLoader.SceneType) roomRNG; // Convert room int to enum
        } else {
            queuedScene = LevelLoader.SceneType.Boss;
        }
        //queuedSceneDebug = queuedScene.ToString();
        GetComponent<Collider2D>().isTrigger = false;

        roomText1 = GameObject.Find("Room1").GetComponent<TextMeshProUGUI>();
        roomText2 = GameObject.Find("Room2").GetComponent<TextMeshProUGUI>();
        roomText1.text = (currRoomNum != 12) ? CustomToString(exitCap1.queuedScene.ToString()) : "";
        roomText2.text = (currRoomNum != 12) ? CustomToString(exitCap2.queuedScene.ToString()) : "";
    }

    void Update(){
        enemyCount = (spawnManager == null) ? 0 : spawnManager.enemyCount;
        // FindFirst... is slow 
        if (enemyCount < 1 && FindFirstObjectByType<EventFactory>() == null) {
            roomText1.enabled = true;
            roomText2.enabled = true;
            GetComponent<Collider2D>().isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            if (enemyCount == 0) {
                SaveData.Instance.data.currentRoomCompleted = true;
                levelLoader.LoadNextLevel(queuedScene ?? LevelLoader.SceneType.MainMenu);
            } else {
                Debug.Log("Went to the exit, but there are still " + enemyCount + " enemies alive!");
            }
        }
    }

    public String CustomToString(String input) {
        switch (input) {
            case "MainMenu":
                return "Main Menu";
            case "BasicEnemy":
                return "Basic Enemy";
            case "EliteEnemy":
                return "Elite Enemy";
            case "Event":
                return "Reward";
            case "Boss":
                return "Boss";
        }  
        return "error";
    }
}
