using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple singleton to track collected animals and display them on screen.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find it in the scene
                _instance = Object.FindAnyObjectByType<InventoryManager>();
                
                // If still null, create a new one automatically
                if (_instance == null)
                {
                    GameObject go = new GameObject("InventoryManager (Auto-Created)");
                    _instance = go.AddComponent<InventoryManager>();
                    Debug.Log("InventoryManager missing! Auto-created it for you.");
                }
            }
            return _instance;
        }
    }
    private static InventoryManager _instance;

    private Dictionary<string, int> items = new Dictionary<string, int>();
    private Texture2D bgTexture;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Create a simple background texture for the UI
        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.6f));
        bgTexture.Apply();
    }

    public void AddItem(string itemName, int amount)
    {
        if (items.ContainsKey(itemName))
        {
            items[itemName] += amount;
        }
        else
        {
            items.Add(itemName, amount);
        }
        Debug.Log($"Inventory: Added {amount} x {itemName}");
    }

    private void OnGUI()
    {
        // Simple Inventory UI on the top right
        float width = 200f;
        float height = items.Count * 25f + 40f;
        float padding = 10f;
        
        Rect rect = new Rect(Screen.width - width - padding, padding, width, height);

        // Draw background
        GUI.DrawTexture(rect, bgTexture);

        // Header
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(rect.x, rect.y + 5, width, 25), "INVENTORY", headerStyle);

        // List items
        int i = 0;
        foreach (var item in items)
        {
            GUI.Label(new Rect(rect.x + 10, rect.y + 30 + (i * 25), width - 20, 25), $"{item.Key}: {item.Value}");
            i++;
        }
        
        if (items.Count == 0)
        {
            GUIStyle emptyStyle = new GUIStyle(GUI.skin.label);
            emptyStyle.fontStyle = FontStyle.Italic;
            emptyStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x, rect.y + 30, width, 25), "(Empty)", emptyStyle);
        }
    }
}
