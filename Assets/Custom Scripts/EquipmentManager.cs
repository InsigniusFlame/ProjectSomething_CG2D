using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    
    #region Singleton

    public static EquipmentManager instance;

    void Awake() { instance = this; }

    #endregion

    Equipment[] currentEquipment;

    void Start()
    {
        int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        currentEquipment = new Equipment[numSlots];
    }

    public void Equip (Equipment newItem)
    {
        int slotIndex = (int)newItem.equipSlot;
        currentEquipment[slotIndex] = newItem;
        Debug.Log("Equipping item: " + newItem.itemName);
    }

    public void Unequip(Equipment equippedItem)
    {
        int slotIndex = (int)equippedItem.equipSlot;
        if (currentEquipment[slotIndex] != null && currentEquipment[slotIndex] == equippedItem)
        {
            currentEquipment[slotIndex] = null;
            Debug.Log("Unequipping item: " + equippedItem.itemName);
        }
    }
}
    
