using UnityEngine;
using UnityEngine.UI;

public class ItemMenu : MonoBehaviour
{
    private ItemData gameItem;
    public Button useButton;
    public Button dropButton;

    public void setItem(ItemData item)
    {
        gameItem = item;
    }
    
    private void Start()
    {
        useButton.onClick.AddListener(OnUseClicked);
        dropButton.onClick.AddListener(OnDropClicked);
    }

    private void OnUseClicked()
    {
        if (gameItem != null)
        {
            Debug.Log("Using item: " + gameItem.itemName);
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
}
