using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadGame() 
    {
        SceneManager.LoadScene(1); // loads game scene (scene 1)
    }
}
