using UnityEngine;

public class EquipmentToggle : MonoBehaviour
{
    public GameObject holdPivot; // Drag your Hold_Pivot here
    public KeyCode equipKey = KeyCode.K; // Set to K as requested
    
    private bool isVisible = false;

    void Start()
    {
        // Hide the arms and knife at the very start
        if(holdPivot != null) holdPivot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(equipKey))
        {
            isVisible = !isVisible;
            holdPivot.SetActive(isVisible);
        }
    }
}