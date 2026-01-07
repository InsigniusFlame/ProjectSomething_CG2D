using UnityEngine;

public class GiveItem : MonoBehaviour
{
    public ItemData item;
    
    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory != null && item != null)
        {
            inventory.AddItem(item);
            Destroy(gameObject);
        }
    }
}
