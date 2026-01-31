using UnityEngine;

/// <summary>
/// Handles player health and damage effects like the red flash.
/// </summary>
public class DamageSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    
    [Header("Damage Flash Settings")]
    [Tooltip("Duration of the red damage flash")]
    public float flashDuration = 0.3f;
    
    [Tooltip("How thick the border flash is (0-0.5)")]
    [Range(0f, 0.5f)]
    public float borderThickness = 0.15f;
    
    [Tooltip("Flash color")]
    public Color flashColor = new Color(1f, 0f, 0f, 0.6f);
    
    [Tooltip("Minimum time between damage flashes")]
    public float damageCooldown = 0.5f;

    private float flashTimer;
    private float damageCooldownTimer;
    private Texture2D flashTexture;

    private void Start()
    {
        currentHealth = maxHealth;
        CreateFlashTexture();
    }

    private void CreateFlashTexture()
    {
        flashTexture = new Texture2D(1, 1);
        flashTexture.SetPixel(0, 0, Color.white);
        flashTexture.Apply();
    }

    private void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
        }
        
        if (damageCooldownTimer > 0)
        {
            damageCooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Called when the player takes damage.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (damageCooldownTimer > 0) return;
        
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Trigger the flash
        flashTimer = flashDuration;
        damageCooldownTimer = damageCooldown;
        
        Debug.Log($"Player took {amount} damage! Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            OnPlayerDeath();
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("Player died!");
        // Reset health for demo
        currentHealth = maxHealth;
    }

    private void OnGUI()
    {
        // Draw damage flash
        if (flashTimer > 0 && flashTexture != null)
        {
            float alpha = (flashTimer / flashDuration) * flashColor.a;
            Color currentColor = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            GUI.color = currentColor;

            float screenW = Screen.width;
            float screenH = Screen.height;
            float borderW = screenW * borderThickness;
            float borderH = screenH * borderThickness;

            // Top border
            GUI.DrawTexture(new Rect(0, 0, screenW, borderH), flashTexture);
            
            // Bottom border
            GUI.DrawTexture(new Rect(0, screenH - borderH, screenW, borderH), flashTexture);
            
            // Left border
            GUI.DrawTexture(new Rect(0, 0, borderW, screenH), flashTexture);
            
            // Right border
            GUI.DrawTexture(new Rect(screenW - borderW, 0, borderW, screenH), flashTexture);

            GUI.color = Color.white;
        }

        // Draw health bar
        DrawHealthBar();
    }

    private void DrawHealthBar()
    {
        float barWidth = 200f;
        float barHeight = 20f;
        float padding = 20f;
        
        Rect bgRect = new Rect(padding, Screen.height - padding - barHeight, barWidth, barHeight);
        Rect healthRect = new Rect(padding, Screen.height - padding - barHeight, 
            barWidth * ((float)currentHealth / maxHealth), barHeight);

        // Background
        GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        GUI.DrawTexture(bgRect, flashTexture);
        
        // Health
        float healthPercent = (float)currentHealth / maxHealth;
        if (healthPercent > 0.5f)
            GUI.color = Color.green;
        else if (healthPercent > 0.25f)
            GUI.color = Color.yellow;
        else
            GUI.color = Color.red;
            
        GUI.DrawTexture(healthRect, flashTexture);
        
        // Border
        GUI.color = Color.white;
        GUI.Box(bgRect, "");
        
        // Text
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;
        GUI.Label(bgRect, $"{currentHealth} / {maxHealth}", style);
    }
}
