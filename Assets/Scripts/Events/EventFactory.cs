using UnityEngine;
using System;
using UnityEngine.UI; // Make sure you include this for UI components

public enum BasicUpgrade {
    MoveSpeed,
    AttackSpeed,
    AttackDamage,
    DashDistance,
    DashDamage,
    Heal,
}

public enum AdvancedUpgrade
{
    Multishot, // adds additional projectiles at an angle
    RespiratoryBurst, // fires a spread of projectiles after a dash
    DashLifesteal, // lifesteal during dash
}

public enum EventUpgradeType
{
    Basic,
    Advanced
}

public class EventFactory : MonoBehaviour
{
    [SerializeField] public BasicUpgrade eventType;
    [SerializeField] public GameObject pickupEffect;
    [SerializeField] public Canvas upgradeUI;
    [SerializeField] public Button upButton0;
    [SerializeField] public Button upButton1;

    private Character player = null;

    public EventUpgradeType upgradeType = EventUpgradeType.Basic;

    private void Start() {
        // Pick an event at random. Currently only 1 event!
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

            var range = Enum.GetValues(typeof(BasicUpgrade)).Length;
            var rand = new System.Random();
            var upgrade0 = (BasicUpgrade)rand.Next(0, range);
            var upgrade1 = (BasicUpgrade)rand.Next(0, range);
            for (int i = 0; i < 5 && upgrade0 == upgrade1; i++)
            {
                upgrade1 = (BasicUpgrade)rand.Next(i, (range + i) % range);
            }

            upgradeUI.gameObject.SetActive(true);
        }
    }

    public void PerformUpgrade(string stat)
    {
        if (upgradeType == EventUpgradeType.Advanced)
        {
            PerformAdvancedUpgrade(stat);
        } else
        {
            PerformBasicUpgrade(stat);
        }

        AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.Upgrade, 1f, 1f);
        Instantiate(pickupEffect, transform.position, transform.rotation);
        SaveData.Instance.SaveToJson();
        player.SyncStats();
        upgradeUI.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void PerformAdvancedUpgrade(string upgrade)
    {

        var data = SaveData.Instance.data;
        switch (upgrade)
        {
            case nameof(AdvancedUpgrade.Multishot):
                int val;
                data.collectedUpgrades.TryGetValue(AdvancedUpgrade.Multishot, out val);
                data.collectedUpgrades[AdvancedUpgrade.Multishot] = val + 1;
                break;
            default:
                Debug.Log("did not find the upgrade: available upgrades: " + nameof(AdvancedUpgrade.Multishot));
                break;
        }
    }
    private void PerformBasicUpgrade(string stat)
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
                data.attackDamage = (int)(data.attackDamage * amount);
                break;
            default:
                Debug.LogWarning("Invalid stat chosen for upgrade: " + stat);
                break;
        }
    }

    //private void StartEvent(Character player) {
    //    switch (eventType) {
    //        case BasicUpgrade.MoveSpeed:
    //            // Create an effect on pickup
    //            Instantiate(pickupEffect, transform.position, transform.rotation);
    //            // Increase player move speed by 5% of 5 (so it doesn't compound... unless we want it to)
    //            Debug.Log("speed");
    //            SaveData.Instance.data.moveSpeed = SaveData.Instance.data.moveSpeed * (1 + (5 * 0.05f));
    //            SaveData.Instance.data.currentRoomCompleted = true;
    //            SaveData.Instance.SaveToJson();
    //            player.SyncStats();
    //            // Remove event pickup asset.
    //            Destroy(gameObject);
    //            break;
    //        default:
    //            Debug.LogWarning("Invalid event type");
    //            break;
    //    }
    //}
}
