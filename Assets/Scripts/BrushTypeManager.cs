using UnityEngine;
using UnityEngine.UI;

public class BrushTypeManager : MonoBehaviour
{
    [Header("Brush Type Buttons")]
    public Button roundBrushButton; // Assign in Inspector
    public Button squareBrushButton; // Assign in Inspector (or other brush types)

    [Header("Brush Visuals (for coloring)")]
    public Image roundBrushColorPanel;
    public Image squareBrushColorPanel;

    [Header("Brush Textures")]
    public Texture2D roundBrushTexture; // Assign your actual round brush Texture2D here
    public Texture2D squareBrushTexture; // Assign your actual square brush Texture2D here
    
    // --- NEW: Decal Prefabs for each brush type ---
    [Header("Brush Decal Prefabs")]
    public GameObject roundBrushDecalPrefab; // Assign the decal prefab for the round brush
    public GameObject squareBrushDecalPrefab; // Assign the decal prefab for the square brush
    // ----------------------------------------------

    private Button currentSelectedBrushButton;
    private Transform currentActiveBorder;

    void Awake()
    {
        if (roundBrushButton != null && roundBrushDecalPrefab != null)
        {
            roundBrushButton.onClick.AddListener(() => OnBrushTypeSelected(roundBrushButton, roundBrushTexture, roundBrushDecalPrefab));
        }
        else
        {
            Debug.LogError("Round Brush Button or Decal Prefab not assigned in BrushTypeManager.", this);
        }

        if (squareBrushButton != null && squareBrushDecalPrefab != null)
        {
            squareBrushButton.onClick.AddListener(() => OnBrushTypeSelected(squareBrushButton, squareBrushTexture, squareBrushDecalPrefab));
        }
        else
        {
            Debug.LogError("Square Brush Button or Decal Prefab not assigned in BrushTypeManager.", this);
        }

        ColorManager.OnColorChanged += UpdateBrushButtonColors;

        // Set initial selected brush and color
        if (roundBrushButton != null && roundBrushTexture != null && roundBrushDecalPrefab != null)
        {
            OnBrushTypeSelected(roundBrushButton, roundBrushTexture, roundBrushDecalPrefab);
            UpdateBrushButtonColors(ColorManager.SelectedColor);
        }
    }

    void OnDestroy()
    {
        ColorManager.OnColorChanged -= UpdateBrushButtonColors;
    }

    // --- MODIFIED: Now accepts a decalPrefab parameter ---
    void OnBrushTypeSelected(Button selectedButton, Texture2D brushTex, GameObject decalPrefab)
    {
        // Deactivate the previously active border, if any
        if (currentActiveBorder != null)
        {
            currentActiveBorder.gameObject.SetActive(false);
        }

        // Find and activate the border of the newly selected button
        if (selectedButton != null)
        {
            Transform newBorder = selectedButton.transform.Find("border");
            if (newBorder != null)
            {
                newBorder.gameObject.SetActive(true);
                currentActiveBorder = newBorder;
            }
            else
            {
                Debug.LogWarning($"No 'border' child named 'border' found for button: {selectedButton.name}. Please check its hierarchy.", selectedButton);
                currentActiveBorder = null;
            }
            currentSelectedBrushButton = selectedButton;
        }
        else
        {
            currentSelectedBrushButton = null;
            currentActiveBorder = null;
        }

        // Set the static brush texture in the DrawableWall script
        if (brushTex != null)
        {
            DrawableWall.BrushTexture = brushTex;
            Debug.Log($"Brush texture set to: {brushTex.name}");
        }

        // --- NEW: Set the static decal prefab in the SprayPainter script ---
        if (decalPrefab != null)
        {
            SprayPainter.CurrentDecalPrefab = decalPrefab;
            Debug.Log($"SprayPainter Decal Prefab set to: {decalPrefab.name}");
        }
        else
        {
            Debug.LogWarning($"Decal Prefab for selected brush is null. SprayPainter might not work correctly.");
        }
        // -----------------------------------------------------------------
    }

    void UpdateBrushButtonColors(Color newColor)
    {
        if (roundBrushColorPanel != null)
        {
            roundBrushColorPanel.color = newColor;
        }
        if (squareBrushColorPanel != null)
        {
            squareBrushColorPanel.color = newColor;
        }
    }
}