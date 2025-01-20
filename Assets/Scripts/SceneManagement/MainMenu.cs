using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start() {
        AudioManager.Instance.SwapTrack(0);
    }
    public void LoadGame() 
    {
        SceneManager.LoadScene("NewRunScreen");
        AudioManager.Instance.SwapTrack(1);
    }
}
