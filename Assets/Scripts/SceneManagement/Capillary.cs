using System.Linq;
using UnityEngine;

public class Capillary : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int currRoomNum;
    
    [SerializeField] private Capillary otherCapillary; // Communication b/w the two exit capillaries so they don't give players duplicate options
    public LevelLoader.SceneType? queuedScene = null; // The next scene if the player chooses this capillary
    [SerializeField] public string queuedSceneDebug;

    private int invalidRoomType; // If the other capillary already has a queued scene type, that one is invalid
    void Start()
    {
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
        queuedSceneDebug = queuedScene.ToString();
    }
}
