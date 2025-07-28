using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class SprayPainter : MonoBehaviour
{
    [Header("Spray Settings")]
    [Tooltip("The default decal prefab to instantiate. Will be overridden by BrushTypeManager.")]
    public GameObject defaultDecalPrefab;

    // --- FIX: Changed 'private set;' to 'set;' ---
    public static GameObject CurrentDecalPrefab { get; set; } // Now publicly settable
    // ---------------------------------------------

    [Tooltip("Layer Mask to only hit walls.")]
    public LayerMask wallsLayer;
    [Tooltip("Time in seconds between each spray mark when holding down the mouse.")]
    public float sprayRate = 0.05f;
    [Tooltip("Small offset from the wall surface to prevent Z-fighting.")]
    public float decalOffset = 0.01f;
    [Tooltip("Randomness multiplier for decal size (e.g., 0.8 to 1.2 for +/- 20%).")]
    public float sizeRandomness = 0.2f;
    [Tooltip("Optional: Maximum raycast distance.")]
    public float maxSprayDistance = 10f;

    [Header("UI Blocking")]
    public Canvas uiCanvas;
    public LayerMask blockingUILayer;

    [Header("Toggle Spray Settings")]
    public Slider sprayToggleSlider;
    private bool canSpray = true;

    private float nextSprayTime;
    private Camera mainCamera;
    private GraphicRaycaster uiRaycaster;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found! Please ensure your camera is tagged 'MainCamera'.");
        }

        if (uiCanvas != null)
        {
            uiRaycaster = uiCanvas.GetComponent<GraphicRaycaster>();
            if (uiRaycaster == null)
            {
                Debug.LogError("The assigned UI Canvas does not have a GraphicRaycaster component! Please add one.", uiCanvas);
            }
        }
        else
        {
            Debug.LogError("UI Canvas not assigned in SprayPainter script. UI blocking will not work.", this);
        }

        nextSprayTime = Time.time;

        if (sprayToggleSlider != null)
        {
            sprayToggleSlider.onValueChanged.AddListener(SetCanSprayFromSlider);
            canSpray = (sprayToggleSlider.value >= 0.5f);
        }
        else
        {
            Debug.LogWarning("Spray Toggle Slider not assigned in SprayPainter. Spraying will always be active.", this);
        }

        // Initialize CurrentDecalPrefab
        if (defaultDecalPrefab != null)
        {
            CurrentDecalPrefab = defaultDecalPrefab;
        }
        else
        {
            Debug.LogError("Default Decal Prefab is not assigned in SprayPainter. Spraying may not work.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && !IsPointerOverBlockingUI() && canSpray)
        {
            if (Time.time >= nextSprayTime)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, maxSprayDistance, wallsLayer))
                {
                    PerformSpray(hit);
                    nextSprayTime = Time.time + sprayRate;
                }
            }
        }
    }

    private bool IsPointerOverBlockingUI()
    {
        if (EventSystem.current == null || uiRaycaster == null)
        {
            return false;
        }

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();

        uiRaycaster.Raycast(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if ((blockingUILayer.value & (1 << result.gameObject.layer)) > 0)
            {
                return true;
            }
        }
        return false;
    }

    void PerformSpray(RaycastHit hitInfo)
    {
        if (CurrentDecalPrefab == null)
        {
            Debug.LogWarning("CurrentDecalPrefab is null. Cannot spray.");
            return;
        }
        GameObject decalInstance = Instantiate(CurrentDecalPrefab);

        decalInstance.transform.position = hitInfo.point + hitInfo.normal * decalOffset;
        decalInstance.transform.forward = hitInfo.normal;
        decalInstance.transform.Rotate(Vector3.forward, Random.Range(0f, 360f));

        float baseSpraySize = SpraySizeManager.CurrentSpraySize;
        float randomFactor = 1f + Random.Range(-sizeRandomness, sizeRandomness);
        float finalSpraySize = baseSpraySize * randomFactor;

        decalInstance.transform.localScale = Vector3.one * finalSpraySize;

        Material decalMaterial = decalInstance.GetComponent<MeshRenderer>().material;

        decalMaterial.color = ColorManager.SelectedColor;
    }

    public void SetCanSprayFromSlider(float value)
    {
        canSpray = (value >= 0.5f);
        Debug.Log("SprayPainter spraying enabled: " + canSpray + " (from slider value: " + value + ")");
    }

    void OnDestroy()
    {
        if (sprayToggleSlider != null)
        {
            sprayToggleSlider.onValueChanged.RemoveListener(SetCanSprayFromSlider);
        }
    }
}