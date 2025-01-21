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
        SceneManager.LoadScene("NewRunScreen");
        // AudioManager.Instance.ReturnToDefault();
    }
}
