using UnityEngine;
using UnityEngine.EventSystems; // Required for IsPointerOverGameObject
using UnityEngine.UI; // Required for Slider (and Toggle if you had it)

public class DrawableWall : MonoBehaviour
{
    [Header("Drawing Settings")]
    public Camera drawingCamera;
    public LayerMask drawableLayer = 1;
    
    public static Texture2D BrushTexture { get; set; }
    
    [Header("Render Texture Settings")]
    public int textureSize = 1024;

    // --- MODIFIED: Now references a Slider ---
    [Header("Toggle Paint Settings")]
    public Slider paintToggleSlider; // Assign your "Toggle Paint" UI Slider here
    private bool canDraw = true; // Controls if drawing is active for THIS wall
    // -----------

    private RenderTexture canvasTexture;
    private RenderTexture tempTexture;
    private Material drawingMaterial;
    private Renderer wallRenderer;
    private bool isDrawing = false;

    void Awake()
    {
        SetupDrawingSystem();
        
        ColorManager.OnColorChanged += UpdateDrawingColor;

        // --- MODIFIED: Subscribe to Slider's onValueChanged event ---
        if (paintToggleSlider != null)
        {
            paintToggleSlider.onValueChanged.AddListener(SetCanDrawFromSlider); // Changed method name
            // Initialize canDraw based on slider's initial value (0 or 1)
            canDraw = (paintToggleSlider.value >= 0.5f); // Treat 1 as true, 0 as false
        }
        else
        {
            Debug.LogWarning("Paint Toggle Slider not assigned in DrawableWall on " + gameObject.name + ". Drawing will always be active.", this);
        }
        // -----------
        
        UpdateDrawingColor(ColorManager.SelectedColor);

        if (BrushTexture == null)
        {
            BrushTexture = CreateDefaultBrush();
        }
    }

    void SetupDrawingSystem()
    {
        wallRenderer = GetComponent<Renderer>();

        canvasTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        canvasTexture.Create();

        tempTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        tempTexture.Create();

        RenderTexture.active = canvasTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;

        drawingMaterial = new Material(Shader.Find("Unlit/Texture"));

        if (wallRenderer.material.HasProperty("_MainTex"))
        {
            wallRenderer.material.mainTexture = canvasTexture;
        }
    }

    void Update()
    {
        if (canDraw)
        {
            HandleInput();
        }
        else if (isDrawing)
        {
            StopDrawing();
        }
    }

    void HandleInput()
    {
        Vector3 inputPosition;
        bool isMouseInput = Input.GetMouseButton(0);
        bool isTouchInput = Input.touchCount > 0;

        if (isMouseInput)
        {
            inputPosition = Input.mousePosition;
        }
        else if (isTouchInput)
        {
            inputPosition = Input.GetTouch(0).position;
        }
        else
        {
            StopDrawing();
            return;
        }

        bool isOverUI = EventSystem.current != null && (isMouseInput ? EventSystem.current.IsPointerOverGameObject() : EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId));
        if (isOverUI)
        {
            StopDrawing();
            return;
        }

        if (Input.GetMouseButtonDown(0) || (isTouchInput && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            StartDrawing();
        }
        else if (Input.GetMouseButton(0) || (isTouchInput && Input.GetTouch(0).phase == TouchPhase.Moved))
        {
            if (isDrawing)
            {
                ContinueDrawing();
            }
        }
        else if (Input.GetMouseButtonUp(0) || (isTouchInput && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            StopDrawing();
        }
    }

    void StartDrawing()
    {
        Vector2 hitPoint;
        if (GetHitPoint(out hitPoint))
        {
            isDrawing = true;
            DrawAtPosition(hitPoint);
        }
    }

    void ContinueDrawing()
    {
        Vector2 hitPoint;
        if (GetHitPoint(out hitPoint))
        {
            DrawAtPosition(hitPoint);
        }
    }

    void StopDrawing()
    {
        isDrawing = false;
    }

    bool GetHitPoint(out Vector2 hitPoint)
    {
        hitPoint = Vector2.zero;

        Vector3 inputPosition;
        if (Input.touchCount > 0)
        {
            inputPosition = Input.GetTouch(0).position;
        }
        else
        {
            inputPosition = Input.mousePosition;
        }

        Ray ray = drawingCamera.ScreenPointToRay(inputPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, drawableLayer))
        {
            if (hit.collider.gameObject == gameObject)
            {
                hitPoint = hit.textureCoord;
                return true;
            }
        }
        return false;
    }

    void DrawAtPosition(Vector2 uvPosition)
    {
        int pixelX = Mathf.RoundToInt(uvPosition.x * textureSize);
        int pixelY = Mathf.RoundToInt(uvPosition.y * textureSize);

        int brushPixelSize = Mathf.RoundToInt(SpraySizeManager.CurrentSpraySize * textureSize / 10f);
        
        if (brushPixelSize < 1) brushPixelSize = 1;

        Graphics.CopyTexture(canvasTexture, tempTexture);

        RenderTexture.active = canvasTexture;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, textureSize, textureSize, 0);

        drawingMaterial.mainTexture = BrushTexture;
        drawingMaterial.SetPass(0);

        GL.Color(ColorManager.SelectedColor);

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0, 0); GL.Vertex3(pixelX - brushPixelSize / 2, pixelY - brushPixelSize / 2, 0);
        GL.TexCoord2(1, 0); GL.Vertex3(pixelX + brushPixelSize / 2, pixelY - brushPixelSize / 2, 0);
        GL.TexCoord2(1, 1); GL.Vertex3(pixelX + brushPixelSize / 2, pixelY + brushPixelSize / 2, 0);
        GL.TexCoord2(0, 1); GL.Vertex3(pixelX - brushPixelSize / 2, pixelY + brushPixelSize / 2, 0);
        GL.End();

        GL.PopMatrix();
        RenderTexture.active = null;
    }

    Texture2D CreateDefaultBrush()
    {
        int size = 64;
        Texture2D brush = new Texture2D(size, size, TextureFormat.ARGB32, false);

        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                float alpha = Mathf.Clamp01(1f - (distance / radius));
                alpha = Mathf.SmoothStep(0f, 1f, alpha);

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        brush.SetPixels(pixels);
        brush.Apply();

        return brush;
    }

    public void ClearCanvas()
    {
        RenderTexture.active = canvasTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;
    }

    public void UpdateDrawingColor(Color newColor)
    {
        // No direct field update needed as GL.Color reads static SelectedColor
    }
    
    // --- MODIFIED: Method now accepts float from Slider ---
    public void SetCanDrawFromSlider(float value)
    {
        canDraw = (value >= 0.5f); // Convert 0 or 1 float to bool
        Debug.Log("DrawableWall drawing on " + gameObject.name + " enabled: " + canDraw + " (from slider value: " + value + ")");
        if (!canDraw && isDrawing)
        {
            StopDrawing();
        }
    }
    // -----------

    void OnDestroy()
    {
        if (canvasTexture != null)
        {
            canvasTexture.Release();
        }
        if (tempTexture != null)
        {
            tempTexture.Release();
        }
        ColorManager.OnColorChanged -= UpdateDrawingColor;

        // --- MODIFIED: Remove listener for Slider ---
        if (paintToggleSlider != null)
        {
            paintToggleSlider.onValueChanged.RemoveListener(SetCanDrawFromSlider);
        }
        // -----------
    }
}