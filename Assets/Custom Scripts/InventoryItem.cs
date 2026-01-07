using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour,IPointerClickHandler
{
    private ItemData curItem;
    private GameObject useMenu;
    public void Setup(ItemData item,GameObject _useMenu)
    {
        curItem = item;
        UnityEngine.UI.Image iconImage = transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();
        iconImage.sprite = item.icon;
        useMenu = _useMenu;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (curItem != null)
        {
            useMenu.SetActive(true);
            ItemMenu menuScript = useMenu.GetComponent<ItemMenu>();
            menuScript.setItem(curItem);
        }
    }

    public void Use(){
        
    }

    public void Drop(){

    }
}
