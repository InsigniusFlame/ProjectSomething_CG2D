using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{

    public List<ItemData> myItems = new List<ItemData>();
    public float hunger = 100f;

    public bool updateInvPending = false;

    public void AddItem(ItemData item)
    {
        myItems.Add(item);
        Debug.Log("Picked up: " + item.itemName);
    }

    public void RemoveItem(ItemData item)
    {
        if (myItems.Contains(item))
        {
            myItems.Remove(item);
            Debug.Log("Removed: " + item.itemName);
            updateInvPending = true;
        }
        else
        {
            Debug.LogWarning("Item not found in inventory: " + item.itemName);
        }
    }
    void Update()
    {
        hunger -= Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0f, 100f);
    }
}