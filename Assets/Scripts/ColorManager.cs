// using UnityEngine;
// using UnityEngine.UI;
//
// public class ColorManager : MonoBehaviour
// {
//     public Transform buttonContainer; // Assign "Color Buttons" in the Inspector
//     public int defaultSelectedIndex = 0;
//
//     private GameObject currentSelectedButton;
//
//     // --- NEW ---
//     // Public static property to hold the currently selected color
//     public static Color SelectedColor { get; private set; } = Color.white; // Default to white
//     // -----------
//
//     void Start()
//     {
//         // ... (your existing Start method code) ...
//         for (int i = 0; i < buttonContainer.childCount; i++)
//         {
//             GameObject btnObj = buttonContainer.GetChild(i).gameObject;
//             Button btn = btnObj.GetComponent<Button>();
//             Image btnImage = btnObj.GetComponent<Image>();
//             Color color = btnImage.color;
//
//             // Copy for closure
//             GameObject capturedObj = btnObj;
//             Color capturedColor = color;
//
//             btn.onClick.AddListener(() =>
//             {
//                 OnColorSelected(capturedObj, capturedColor);
//             });
//         }
//
//         // Select default
//         if (buttonContainer.childCount > defaultSelectedIndex)
//         {
//             GameObject defaultBtn = buttonContainer.GetChild(defaultSelectedIndex).gameObject;
//             OnColorSelected(defaultBtn, defaultBtn.GetComponent<Image>().color);
//         }
//     }
//
//     void OnColorSelected(GameObject buttonObj, Color color)
//     {
//         // Unhighlight previous
//         if (currentSelectedButton != null)
//         {
//             Transform oldBorder = currentSelectedButton.transform.Find("border");
//             if (oldBorder) oldBorder.gameObject.SetActive(false);
//         }
//
//         // Highlight new
//         Transform newBorder = buttonObj.transform.Find("border");
//         if (newBorder) newBorder.gameObject.SetActive(true);
//
//         currentSelectedButton = buttonObj;
//
//         // Apply color to preview box (optional)
//         // if (previewBox != null) previewBox.color = color;
//
//         // --- NEW ---
//         // Set the static selected color
//         SelectedColor = color;
//         // -----------
//
//         // Debug
//         Debug.Log("Selected color: " + color + " from " + buttonObj.name);
//     }
//
//     void AddBorderToButton(GameObject buttonObj)
//     {
//         // ... (your existing AddBorderToButton method code) ...
//         GameObject border = new GameObject("border");
//         border.transform.SetParent(buttonObj.transform);
//         border.transform.SetAsFirstSibling(); // Behind button image
//
//         RectTransform rect = border.AddComponent<RectTransform>();
//         rect.anchorMin = Vector2.zero;
//         rect.anchorMax = Vector2.one;
//         rect.offsetMin = new Vector2(-5, -5);
//         rect.offsetMax = new Vector2(5, 5);
//
//         Image img = border.AddComponent<Image>();
//         img.color = new Color(1, 1, 1, 0.4f); // White with alpha
//         img.raycastTarget = false;
//
//         border.SetActive(false);
//     }
// }


using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    public Transform buttonContainer; // Assign "Color Buttons" in the Inspector
    public int defaultSelectedIndex = 0;

    private GameObject currentSelectedButton;

    public static Color SelectedColor { get; private set; } = Color.white; // Default to white

    // --- NEW ---
    // Event to notify other scripts when the color changes
    public static event System.Action<Color> OnColorChanged;
    // -----------

    void Start()
    {
        for (int i = 0; i < buttonContainer.childCount; i++)
        {
            GameObject btnObj = buttonContainer.GetChild(i).gameObject;
            Button btn = btnObj.GetComponent<Button>();
            Image btnImage = btnObj.GetComponent<Image>();
            Color color = btnImage.color;

            // Copy for closure
            GameObject capturedObj = btnObj;
            Color capturedColor = color;

            btn.onClick.AddListener(() =>
            {
                OnColorSelected(capturedObj, capturedColor);
            });
        }

        // Select default
        if (buttonContainer.childCount > defaultSelectedIndex)
        {
            GameObject defaultBtn = buttonContainer.GetChild(defaultSelectedIndex).gameObject;
            OnColorSelected(defaultBtn, defaultBtn.GetComponent<Image>().color);
        }
    }

    void OnColorSelected(GameObject buttonObj, Color color)
    {
        // Unhighlight previous
        if (currentSelectedButton != null)
        {
            Transform oldBorder = currentSelectedButton.transform.Find("border");
            if (oldBorder) oldBorder.gameObject.SetActive(false);
        }

        // Highlight new
        Transform newBorder = buttonObj.transform.Find("border");
        if (newBorder) newBorder.gameObject.SetActive(true);

        currentSelectedButton = buttonObj;

        SelectedColor = color;
        // --- NEW ---
        // Invoke the event
        OnColorChanged?.Invoke(SelectedColor);
        // -----------

        Debug.Log("Selected color: " + color + " from " + buttonObj.name);
    }

    void AddBorderToButton(GameObject buttonObj)
    {
        GameObject border = new GameObject("border");
        border.transform.SetParent(buttonObj.transform);
        border.transform.SetAsFirstSibling(); // Behind button image

        RectTransform rect = border.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(-5, -5);
        rect.offsetMax = new Vector2(5, 5);

        Image img = border.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.4f); // White with alpha
        img.raycastTarget = false;

        border.SetActive(false);
    }
}
