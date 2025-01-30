using UnityEngine;

public enum AvailableEvents {
    SpeedBoost
}
public class EventFactory : MonoBehaviour
{
    [SerializeField] public AvailableEvents eventType;
    [SerializeField] public GameObject pickupEffect;
    [SerializeField] public Canvas upgradeUI;
    private Character player = null;

    private void Start() {
        // Pick an event at random. Currently only 1 event!
        eventType = (AvailableEvents)Random.Range(0, 1);
        if (SaveData.Instance.data.currentRoomCompleted) {
            Destroy(gameObject);
        }

        if (upgradeUI != null)
        {
            upgradeUI.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            player = other.GetComponent<Character>();
            upgradeUI.gameObject.SetActive(true);
        }
    }

    public void PerformUpgrade(string stat)
    {
        var amount = 1.5f;

        if (player == null)
        {
            Debug.LogWarning("Player not found when attempting to upgrade!");
            upgradeUI.gameObject.SetActive(false);
            return;
        }

        var data = SaveData.Instance.data;
        switch (stat)
        {
            case "maxMoveSpeed":
                data.moveSpeed *= amount;
                break;
            case "attackRate":
                data.attackRate *= amount;
                break;
            case "attackDamage":
                data.attackDamage = (int) (data.attackDamage * amount);
                break;
            default:
                Debug.LogWarning("Invalid stat chosen for upgrade: " + stat);
                break;
        }
        AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.Upgrade, 1f, 1f);
        Instantiate(pickupEffect, transform.position, transform.rotation);
        SaveData.Instance.SaveToJson();
        player.SyncStats();
        upgradeUI.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void StartEvent(Character player) {
        switch (eventType) {
            case AvailableEvents.SpeedBoost:
                // Create an effect on pickup
                Instantiate(pickupEffect, transform.position, transform.rotation);
                // Increase player move speed by 5% of 5 (so it doesn't compound... unless we want it to)
                Debug.Log("speed");
                SaveData.Instance.data.moveSpeed = SaveData.Instance.data.moveSpeed * (1 + (5 * 0.05f));
                SaveData.Instance.data.currentRoomCompleted = true;
                SaveData.Instance.SaveToJson();
                player.SyncStats();
                // Remove event pickup asset.
                Destroy(gameObject);
                break;
            default:
                Debug.LogWarning("Invalid event type");
                break;
        }
    }
}
