using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Equipment", menuName = "Inventory/Equipment")]


public class Equipment : ItemData
{
    public EquipmentSlot equipSlot;
    public int damage;
    public int range;
    public int durability;
    public int cooldown;
    public int armor;

    public void Equip(GameObject user)
    {
        EquipmentManager.instance.Equip(this);
        GameObject menu = GameObject.FindWithTag("ItemMenu");
        if (menu != null)
        {
            ItemMenu menuScript = menu.GetComponent<ItemMenu>();
            if (menuScript != null)
            {
                menuScript.setItem(null);
                menu.SetActive(false);
            }
        }
    }
    public void Unequip(GameObject user)
    {
        EquipmentManager.instance.Unequip(this);
        GameObject menu = GameObject.FindWithTag("ItemMenu");
        if (menu != null)
        {
            ItemMenu menuScript = menu.GetComponent<ItemMenu>();
            if (menuScript != null)
            {
                menuScript.setItem(null);
                menu.SetActive(false);
            }
        }
    }
    
}
public enum EquipmentSlot { Head, Chest, Legs, Feet, Hand }
