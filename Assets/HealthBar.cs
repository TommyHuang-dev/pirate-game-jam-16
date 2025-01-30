using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Slider slider;

    public void MaxHealth(int health) {
        slider.maxValue = health;
        slider.value = health;
    }
    public void SetHealth(int health) {
        slider.value = health;
    }
}
