using UnityEngine;

/// <summary>
/// Simple game manager that displays instructions and handles game state.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI Settings")]
    public bool showInstructions = true;
    public float instructionsFadeTime = 10f;

    private float instructionsAlpha = 1f;
    private GUIStyle instructionStyle;
    private GUIStyle titleStyle;

    private void Start()
    {
        // Ensure cursor is locked at start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Fade out instructions over time
        if (showInstructions && instructionsAlpha > 0)
        {
            instructionsAlpha -= Time.deltaTime / instructionsFadeTime;
        }
    }

    private void OnGUI()
    {
        if (!showInstructions || instructionsAlpha <= 0) return;

        // Initialize styles
        if (instructionStyle == null)
        {
            instructionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            };
            
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };
        }

        // Apply alpha
        Color textColor = Color.white;
        textColor.a = instructionsAlpha;
        instructionStyle.normal.textColor = textColor;
        titleStyle.normal.textColor = textColor;

        // Draw background
        Color bgColor = new Color(0, 0, 0, 0.5f * instructionsAlpha);
        Texture2D bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, bgColor);
        bgTex.Apply();
        
        Rect bgRect = new Rect(20, 20, 350, 180);
        GUI.DrawTexture(bgRect, bgTex);

        // Draw instructions
        GUILayout.BeginArea(new Rect(30, 30, 330, 160));
        GUILayout.Label("Animal AI Demo", titleStyle);
        GUILayout.Space(10);
        GUILayout.Label("Controls:", instructionStyle);
        GUILayout.Label("  WASD - Move", instructionStyle);
        GUILayout.Label("  Mouse - Look around", instructionStyle);
        GUILayout.Label("  Shift - Sprint", instructionStyle);
        GUILayout.Label("  ESC - Toggle cursor", instructionStyle);
        GUILayout.Space(10);
        GUILayout.Label("Walk close to animals to see them flee!", instructionStyle);
        GUILayout.EndArea();
    }
}
