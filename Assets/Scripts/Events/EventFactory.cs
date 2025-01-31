using UnityEngine;
using System;
using UnityEngine.UI; // Make sure you include this for UI components
using TMPro;
using System.Collections;

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
    [SerializeField] public Button button0;
    [SerializeField] public Button button1;

    private bool eventTriggerPopped = false;
    private bool enemiesDefeated = false;
    private SpriteRenderer sprite;

    private Character player = null;

    public EventUpgradeType upgradeType = EventUpgradeType.Basic;

    private BasicUpgrade upgrade0;
    private BasicUpgrade upgrade1;
    private AdvancedUpgrade advUpgrade0;
    private AdvancedUpgrade advUpgrade1;

    private void Start() {
        // Pick an event at random. Currently only 1 event!
        if (SaveData.Instance.data.currentRoomCompleted) {
            Destroy(gameObject);
        }

        if (upgradeUI != null)
        {
            upgradeUI.gameObject.SetActive(false);
        }
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(0, 0, 0, 0);

    }

    private void Update() {
        StartCoroutine(EnemiesAlive());
    }

    private IEnumerator EnemiesAlive() {
        yield return new WaitForSeconds(1f);
        if (FindFirstObjectByType<Enemy>() == null) {
            yield return new WaitForSeconds(0.5f);
            enemiesDefeated = true;
            sprite.color = new Color(213, 255, 255, 255);
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            if (eventTriggerPopped || !enemiesDefeated) {
                return;
            }
            player = other.GetComponent<Character>();

            upgradeUI.gameObject.SetActive(true);
            eventTriggerPopped = true;
            if (upgradeType == EventUpgradeType.Advanced)
            {
                // rn we only have two advanced upgrades
                advUpgrade0 = AdvancedUpgrade.Multishot;
                advUpgrade1 = AdvancedUpgrade.RespiratoryBurst;

                button0.GetComponentInChildren<TextMeshProUGUI>().text = "specialization: B cell";  // Set text for button 1
                button1.GetComponentInChildren<TextMeshProUGUI>().text = "specialization: Macrophage"; // Set text for button 2
            } else
            {
                // select two random basic upgrades
                var range = Enum.GetValues(typeof(BasicUpgrade)).Length;
                var rand = new System.Random();
                upgrade0 = (BasicUpgrade)rand.Next(0, range);
                upgrade1 = (BasicUpgrade)rand.Next(0, range);
                for (int i = 0; i < 5 && upgrade0 == upgrade1; i++)
                {
                    upgrade1 = (BasicUpgrade)rand.Next(i, (range + i) % range);
                }

                button0.GetComponentInChildren<TextMeshProUGUI>().text = "adaptation: " + upgrade0.ToString();  // Set text for button 1
                button1.GetComponentInChildren<TextMeshProUGUI>().text = "adaptation: " + upgrade1.ToString(); // Set text for button 2
            }
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
        Destroy(upgradeUI.gameObject);
        Destroy(gameObject);
    }

    private void PerformAdvancedUpgrade(string upgradeName)
    {
        var data = SaveData.Instance.data;
        // Heal the player after boss fights
        data.currentHealth = data.maxHealth;

        AdvancedUpgrade stat;
        if (upgradeName == "upgrade0")
        {
            stat = advUpgrade0;
        }
        else
        {
            stat = advUpgrade1;
        }

        switch (stat)
        {
            case AdvancedUpgrade.Multishot:
                int val;
                data.collectedUpgrades.TryGetValue(AdvancedUpgrade.Multishot, out val);
                data.collectedUpgrades[AdvancedUpgrade.Multishot] = val + 1;
                break;
            case AdvancedUpgrade.RespiratoryBurst:
                data.collectedUpgrades[AdvancedUpgrade.RespiratoryBurst] = 1;
                break;
            default:
                Debug.Log("did not find the upgrade: available upgrades: " + nameof(AdvancedUpgrade.Multishot));
                break;
        }
    }
    private void PerformBasicUpgrade(string upgradeName)
    {
        if (player == null)
        {
            Debug.LogWarning("Player not found when attempting to upgrade!");
            upgradeUI.gameObject.SetActive(false);
            return;
        }

        var data = SaveData.Instance.data;

        BasicUpgrade stat;
        if (upgradeName == "upgrade0")
        {
            stat = upgrade0;
        }
        else if (upgradeName == "upgrade1")
        {
            stat = upgrade1;
        }
        else
        {
            Debug.LogWarning("upgrade " + upgradeName + " is not valid");
            upgradeUI.gameObject.SetActive(false);
            return;
        }
        switch (stat)
        {
            case BasicUpgrade.MoveSpeed:
                data.moveSpeed += 1.5f; // +30% from base
                break;
            case BasicUpgrade.AttackSpeed:
                data.attackRate += 2.0f; // +50% from base
                break;
            case BasicUpgrade.AttackDamage:
                data.attackDamage += 3;  // +60% from base
                break;
            case BasicUpgrade.DashDistance:
                data.dashDistance += 4.0f; // +50% from base
                break;
            case BasicUpgrade.DashDamage:
                data.dashDamage += 6; // +60% from base
                break;
            case BasicUpgrade.Heal:
                data.currentHealth = data.maxHealth;
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
