using UnityEngine;

public class ModelSceneManipulator : MonoBehaviour
{
    [Header("Manipulation Settings")]
    public bool enableDragging = true;
    public bool enableScaling = true;
    public LayerMask groundLayer = 1; // What layer to raycast against for positioning

    [Header("Scale Settings")]
    public float minScale = 0.1f;
    public float maxScale = 10f;
    public float scaleSpeed = 0.1f;

    [Header("Visual Feedback")]
    public GameObject selectionOutline;
    public Color selectedColor = Color.yellow;
    public Color hoveredColor = Color.white;

    private ModelContent associatedModelContent;
    private Camera mainCamera;
    private bool isDragging = false;
    private bool isSelected = false;
    private Vector3 dragOffset;
    private Vector3 originalScale;
    private Renderer[] modelRenderers;
    private Color[] originalColors;

    void Start()
    {
        mainCamera = Camera.main;
        originalScale = transform.localScale;

        // Get all renderers for visual feedback
        modelRenderers = GetComponentsInChildren<Renderer>();
        StoreOriginalColors();

        // Create selection outline if not provided
        if (selectionOutline == null)
        {
            CreateSelectionOutline();
        }

        SetSelected(false);
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (!isSelected) return;

        // Handle scaling with mouse wheel
        if (enableScaling && Input.mouseScrollDelta.y != 0f)
        {
            bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (altPressed) // Alt + Scroll = Scale model
            {
                ScaleModel(Input.mouseScrollDelta.y * scaleSpeed);
            }
        }

        // Handle rotation with Q/E keys
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, -90f * Time.deltaTime, 0);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, 90f * Time.deltaTime, 0);
        }

        // Handle reset with R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTransform();
        }
    }

    void OnMouseDown()
    {
        if (!enableDragging) return;

        SelectModel();

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        if (mouseWorldPos != Vector3.zero)
        {
            dragOffset = transform.position - mouseWorldPos;
            isDragging = true;
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging || !enableDragging) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        if (mouseWorldPos != Vector3.zero)
        {
            Vector3 newPosition = mouseWorldPos + dragOffset;

            // Constrain to reasonable bounds
            newPosition.x = Mathf.Clamp(newPosition.x, -100f, 100f);
            newPosition.z = Mathf.Clamp(newPosition.z, -100f, 100f);

            transform.position = newPosition;

            // Update associated model content if needed
            NotifyPositionChanged();
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void OnMouseEnter()
    {
        if (!isSelected && modelRenderers != null)
        {
            HighlightModel(hoveredColor);
        }
    }

    void OnMouseExit()
    {
        if (!isSelected && modelRenderers != null)
        {
            RestoreOriginalColors();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) return Vector3.zero;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Try to hit the ground/floor
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            return hit.point;
        }

        // Fallback: project onto a plane at current Y level
        Plane plane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    private void ScaleModel(float scaleDelta)
    {
        Vector3 currentScale = transform.localScale;
        float scaleMultiplier = 1f + scaleDelta;
        Vector3 newScale = currentScale * scaleMultiplier;

        // Clamp
        float avgScale = (newScale.x + newScale.y + newScale.z) / 3f;
        if (avgScale < minScale || avgScale > maxScale)
        {
            var min = Vector3.one * minScale;
            var max = Vector3.one * maxScale;
            newScale = Vector3.Max(min, Vector3.Min(newScale, max));
        }

        transform.localScale = newScale;
        NotifyScaleChanged();

        Debug.Log($"Model {gameObject.name} scaled to {newScale}");
    }


    private void ResetTransform()
    {
        transform.localScale = originalScale;
        transform.rotation = Quaternion.Euler(0, 180, 0); // Default rotation from ModelContent

        // Reset position to center
        var timelineGrid = FindObjectOfType<TimelineGrid>();
        if (timelineGrid != null && timelineGrid.playbackStackRoot != null)
        {
            Vector3 worldCenter = timelineGrid.playbackStackRoot.transform.position;
            worldCenter.z = -80f;
            transform.position = worldCenter;
        }

        NotifyTransformChanged();
        Debug.Log($"Model {gameObject.name} transform reset");
    }

    public void SelectModel()
    {
        // Deselect other models first
        var allManipulators = FindObjectsOfType<ModelSceneManipulator>();
        foreach (var manipulator in allManipulators)
        {
            if (manipulator != this)
            {
                manipulator.SetSelected(false);
            }
        }

        SetSelected(true);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectionOutline != null)
        {
            selectionOutline.SetActive(selected);
        }

        if (selected)
        {
            HighlightModel(selectedColor);
        }
        else
        {
            RestoreOriginalColors();
        }
    }

    private void CreateSelectionOutline()
    {
        // Create a simple wireframe outline
        selectionOutline = new GameObject("SelectionOutline");
        selectionOutline.transform.SetParent(transform);
        selectionOutline.transform.localPosition = Vector3.zero;
        selectionOutline.transform.localRotation = Quaternion.identity;
        selectionOutline.transform.localScale = Vector3.one * 1.05f; // Slightly larger

        // Add a simple wireframe renderer or outline effect
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            var outlineRenderer = selectionOutline.AddComponent<MeshRenderer>();
            var meshFilter = selectionOutline.AddComponent<MeshFilter>();
            meshFilter.mesh = GetComponent<MeshFilter>()?.mesh;

            // Create outline material
            var outlineMaterial = new Material(Shader.Find("Standard"));
            outlineMaterial.color = selectedColor;
            outlineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Front);
            outlineRenderer.material = outlineMaterial;
        }

        selectionOutline.SetActive(false);
    }

    private void StoreOriginalColors()
    {
        if (modelRenderers == null) return;

        originalColors = new Color[modelRenderers.Length];
        for (int i = 0; i < modelRenderers.Length; i++)
        {
            if (modelRenderers[i].material != null)
            {
                originalColors[i] = modelRenderers[i].material.color;
            }
        }
    }

    private void HighlightModel(Color highlightColor)
    {
        if (modelRenderers == null) return;

        for (int i = 0; i < modelRenderers.Length; i++)
        {
            if (modelRenderers[i].material != null)
            {
                modelRenderers[i].material.color = Color.Lerp(originalColors[i], highlightColor, 0.5f);
            }
        }
    }

    private void RestoreOriginalColors()
    {
        if (modelRenderers == null || originalColors == null) return;

        for (int i = 0; i < modelRenderers.Length && i < originalColors.Length; i++)
        {
            if (modelRenderers[i].material != null)
            {
                modelRenderers[i].material.color = originalColors[i];
            }
        }
    }

    // Callbacks for timeline integration
    private void NotifyPositionChanged()
    {
        if (associatedModelContent != null)
        {
            // Could store position data for XML export
            Debug.Log($"Model position changed: {transform.position}");
        }
    }

    private void NotifyScaleChanged()
    {
        if (associatedModelContent != null)
        {
            // Could store scale data for XML export
            Debug.Log($"Model scale changed: {transform.localScale}");
        }
    }

    private void NotifyTransformChanged()
    {
        NotifyPositionChanged();
        NotifyScaleChanged();
    }

    // Public methods for external control
    public void SetAssociatedModelContent(ModelContent modelContent)
    {
        associatedModelContent = modelContent;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        NotifyPositionChanged();
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = new Vector3(
            Mathf.Clamp(scale.x, minScale, maxScale),
            Mathf.Clamp(scale.y, minScale, maxScale),
            Mathf.Clamp(scale.z, minScale, maxScale)
        );
        NotifyScaleChanged();
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
        NotifyTransformChanged();
    }

    // Getters for XML serialization
    public Vector3 GetPosition() => transform.position;
    public Vector3 GetScale() => transform.localScale;
    public Quaternion GetRotation() => transform.rotation;
    public bool IsSelected() => isSelected;

    void OnDestroy()
    {
        // Clean up materials if we created any
        if (selectionOutline != null)
        {
            var renderer = selectionOutline.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                DestroyImmediate(renderer.material);
            }
        }
    }
}