using UnityEngine;
using UnityEngine.UI;

public class ItemMenu : MonoBehaviour
{
    private ItemData gameItem;
    public Button useButton;
    public Button dropButton;
    public Button equipButton;
    public Button UnequipButton;

    public void setItem(ItemData item)
    {
        gameItem = item;
    }
    
    private void Start()
    {
        useButton.onClick.AddListener(OnUseClicked);
        dropButton.onClick.AddListener(OnDropClicked);
        equipButton.onClick.AddListener(OnEquipClicked);
        UnequipButton.onClick.AddListener(OnUnequipClicked);
    }

    private void OnUseClicked()
    {
        if (gameItem != null)
        {
            gameItem.Use(GameObject.FindWithTag("Player"));
        }
    }

    private void OnDropClicked()
    {
        if (gameItem != null)
        {
            Debug.Log("Dropping item: " + gameItem.itemName);
            // Implement item drop logic here
        }
    }

    private void OnEquipClicked()
    {
        if (gameItem != null)
        {
            if (gameItem is Equipment equipmentItem)
            {
                equipmentItem.Equip(GameObject.FindWithTag("Player"));
            }
            else
            {
                Debug.Log("Item is not an Equipment: " + gameItem.itemName);
            }
        }
    }

    private void OnUnequipClicked()
    {
        if (gameItem != null)
        {
            if (gameItem is Equipment equipmentItem)
            {
                equipmentItem.Unequip(GameObject.FindWithTag("Player"));
            }
            else
            {
                Debug.Log("Item is not an Equipment: " +  gameItem.itemName);
            }
        }
    }
}
