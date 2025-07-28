using UnityEngine;
using UnityEngine.UI; // Required for Slider

public class SpraySizeManager : MonoBehaviour
{
    // Public static property to hold the currently selected spray size
    public static float CurrentSpraySize { get; private set; } = 3f; // Default size

    // Reference to the Slider UI component
    public Slider spraySizeSlider;

    void Awake()
    {
        // Get the Slider component if not assigned in the Inspector
        if (spraySizeSlider == null)
        {
            spraySizeSlider = GetComponent<Slider>();
        }

        // Ensure the slider exists before adding listeners
        if (spraySizeSlider != null)
        {
            // Set the static size to the slider's initial value
            CurrentSpraySize = spraySizeSlider.value;
            // Add a listener to update CurrentSpraySize when the slider's value changes
            spraySizeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
        else
        {
            Debug.LogError("SpraySizeSlider not assigned or found on this GameObject. Spray size will use default.");
        }
    }

    private void OnSliderValueChanged(float newValue)
    {
        CurrentSpraySize = newValue;
        Debug.Log("Spray size set to: " + newValue);
    }
}