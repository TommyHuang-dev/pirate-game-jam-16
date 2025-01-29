using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    public Data data;
    public static SaveData Instance;
    private string filePath;

    private void Awake() {
        // Can only have one save system (singleton)
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    // TODO: Can implement a menu to save, currently forces a save when the app quits.
    private void OnApplicationQuit() {
        if (data.currentRoomNumber > 0) {
            SaveToJson();
        }
    }

    private void Start() {
        filePath = Application.persistentDataPath + "/saveData.json";
        if (File.Exists(filePath)) {
            LoadFromJson();
            Debug.Log("Found a save on start. Loading into data");
        } else {
            Debug.Log("No save file detected.");
            data = new Data();
        }
    }

    public void SaveToJson() {
        string dataToWrite = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, dataToWrite);
        Debug.Log("Saved data to " + filePath);
    }

    public bool LoadFromJson() {
        // Returns if load was successful or not
        if (File.Exists(filePath)) {
            string loadedData = File.ReadAllText(filePath);

            data = JsonUtility.FromJson<Data>(loadedData);
            Debug.Log("Loaded data from " + filePath);
            return true;
        } else {
            Debug.LogWarning("No data found");
            return false;
        }
    }

    public void DeleteSaveData() {
        if ( !File.Exists(filePath) ) {
            Debug.Log("No save data found, nothing to delete");
        } else {
            Debug.Log("Deleting save data from " + filePath);
            File.Delete(filePath);
        }
        // Reset save.
        data = new Data();
    }
}

[System.Serializable]
// Initialize data with default values
public class Data {
    // Current room information
    public int currentRoomNumber = 0;
    public int currentRoomType = 0;
    public bool currentRoomCompleted = false;

    // Character attributes
    public float moveSpeed = 5.0f;
    public float attackRate = 4;
    public int attackDamage = 5;
    public int maxHealth = 10;
    public int currentHealth = 10;
}
