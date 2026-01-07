using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;

    public bool edible;
    public int calories;

    public void Use(GameObject user)
    {
        if (edible)
        {
            PlayerInventory inventory = user.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                Debug.Log("Eating at: " + inventory.hunger);
                inventory.hunger += calories;
                inventory.hunger = Mathf.Clamp(inventory.hunger, 0f, 100f);
                Debug.Log("Used " + itemName + ". Hunger increased by " + calories + ". Current Hunger: " + inventory.hunger);
                inventory.RemoveItem(this);
            }
        }
        else
        {
            Debug.Log(itemName + " cannot be used.");
        }

        GameObject menu = GameObject.FindWithTag("ItemMenu");
        if (menu != null)
        {
            ItemMenu menuScript = menu.GetComponent<ItemMenu>();
            if (menuScript != null)
            {
                Debug.Log("Clearing item from menu.");
                menuScript.setItem(null);
                menu.SetActive(false);
            }
        }
    }
}