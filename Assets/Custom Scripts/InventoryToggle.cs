using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryToggle : MonoBehaviour
{
    [Header("UI References (Still need to drag these)")]
    public GameObject inventoryPanel; 
    public GameObject slotPrefab;    
    public Transform contentParent;  

    // These are now private because the script finds them itself
    private FirstPersonController fpsController;
    private PlayerInventory playerInventory; 

    private bool isInventoryOpen = false;
    private List<GameObject> activeSlots = new List<GameObject>();

    public GameObject useMenu;

    private void Awake()
    {
        // Automatically find the scripts attached to THIS player object
        fpsController = GetComponent<FirstPersonController>();
        playerInventory = GetComponent<PlayerInventory>();

        // Safety Check: If you forgot to add one of the scripts, Unity will tell you
        if (fpsController == null) Debug.LogError("FirstPersonController missing from Player!");
        if (playerInventory == null) Debug.LogError("PlayerInventory missing from Player!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        if (playerInventory.updateInvPending)
        {
            if (isInventoryOpen)
            {
                DisplayItems();
            }
            playerInventory.updateInvPending = false;
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            fpsController.cameraCanMove = false;
            fpsController.playerCanMove = false;
            
            DisplayItems();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            fpsController.cameraCanMove = true;
            fpsController.playerCanMove = true;
        }
    }

    void DisplayItems()
    {
        foreach (GameObject slot in activeSlots) Destroy(slot);
        activeSlots.Clear();

        foreach (ItemData item in playerInventory.myItems)
        {
            GameObject newSlot = Instantiate(slotPrefab, contentParent);
            activeSlots.Add(newSlot);

            InventoryItem slotScript = newSlot.GetComponent<InventoryItem>();
            if (slotScript != null)
            {
                slotScript.Setup(item,useMenu);
            }
        }
    }
}