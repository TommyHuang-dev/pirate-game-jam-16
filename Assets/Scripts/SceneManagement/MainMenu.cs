using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start() {
        AudioManager.Instance.SwapTrack(0);
    }
    public void NewGame() 
    {
        PlayerPrefs.DeleteKey("currentRoomNumber");
        SaveData.Instance.DeleteSaveData();
        SceneManager.LoadScene("NewRunScreen");
        // AudioManager.Instance.ReturnToDefault();
    }

    public void LoadGame() {
        if (SaveData.Instance.LoadFromJson() && SaveData.Instance.data.currentRoomNumber > 1) {
            Debug.Log("Loading room " + System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(SaveData.Instance.data.currentRoomType)));
            SceneManager.LoadScene(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(SaveData.Instance.data.currentRoomType)));

            AudioManager.Instance.SwapTrack(SaveData.Instance.data.currentRoomType);
        }       
    }
}
