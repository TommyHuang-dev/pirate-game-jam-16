using UnityEngine;
using TMPro;
public class RoomText : MonoBehaviour
{
    public TextMeshProUGUI roomText1;
    public TextMeshProUGUI roomText2;


    void Start() {
        roomText1 = GameObject.Find("Room1").GetComponent<TextMeshProUGUI>();
        roomText2 = GameObject.Find("Room2").GetComponent<TextMeshProUGUI>();
        roomText1.enabled = false;
        roomText2.enabled = false;
    }
     void Update() {

     }

}
