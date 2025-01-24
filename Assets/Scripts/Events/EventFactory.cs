using UnityEngine;

public enum AvailableEvents {
    SpeedBoost
}
public class EventFactory : MonoBehaviour
{
    [SerializeField] public AvailableEvents eventType;
    [SerializeField] public GameObject pickupEffect;

    private void Start() {
        // Pick an event at random. Currently only 1 event!
        eventType = (AvailableEvents)Random.Range(0, 1);
        if (SaveData.Instance.data.currentRoomCompleted) {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            Character player = other.GetComponent<Character>();
            StartEvent(player);
        }
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
                player.maxMoveSpeed = SaveData.Instance.data.moveSpeed;
                // Remove event pickup asset.
                Destroy(gameObject);
                break;
            default:
                Debug.LogWarning("Invalid event type");
                break;
        }
    }
}
