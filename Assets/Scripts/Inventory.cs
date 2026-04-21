using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public string slotName;
        public int slotNumber;
        public Image backgroundImage;
        public Image itemIcon;
        public InventoryItem item;
        public Image ringProgressImage;

        [HideInInspector] public Vector3 originalScale;
        [HideInInspector] public RectTransform rectTransform;
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public string itemDescription;
        public Sprite itemSprite;
        public bool canHoldToUse;
        public System.Action useAction;
    }

    [System.Serializable]
    public class Item3DPopupData
    {
        public string itemName;
        public GameObject item3DPrefab;
        public Vector3 popupOffset = new Vector3(0, 0, 1.5f);
        public Vector3 popupScale = new Vector3(1f, 1f, 1f);
        public float sizeMultiplier = 1f;  // Easy size control
        public Color backgroundColor = new Color(0, 0, 0, 0.9f);
    }

    [Header("Inventory Slots")]
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    [Header("3D Popup Panel")]
    public GameObject popupPanel;              // The UI Panel that shows/hides
    public Image panelBackground;              // Panel background image
    public TextMeshProUGUI panelTitleText;     // Text at top (item name)
    public RawImage panel3DDisplay;            // RawImage for 3D display
    public TextMeshProUGUI panelDescriptionText; // Text at bottom (description)
    public Transform panel3DContainer;         // Reference kept for compatibility

    [Header("RenderTexture Bridge")]
    public Camera renderCamera;                // Dedicated camera for 3D objects
    public RenderTexture renderTexture;        // Bridge between 3D and UI
    public Transform worldStage;               // World space container for 3D objects

    [Header("3D Popup Settings")]
    public List<Item3DPopupData> item3DPopups = new List<Item3DPopupData>();
    public Camera playerCamera;
    public float popupAnimationSpeed = 0.3f;
    public float objectRotationSpeed = 30f;

    [Header("Scale Effect Settings")]
    public float selectedScale = 1.3f;
    public float scaleAnimationSpeed = 10f;

    [Header("Ring Progress Settings")]
    public float holdDuration = 4f;
    public Color ringColor = Color.white;

    private int currentSelectedSlot = -1;
    private bool isHolding = false;
    private int currentHoldingSlot = -1;
    private GameObject current3DObject;
    private float holdTime = 0f;
    private Sprite ringSprite;
    private int currentActivePanelSlot = -1;
    private Coroutine rotationCoroutine;

    void Start()
    {
        ringSprite = CreateHollowRing();

        if (playerCamera == null)
            playerCamera = Camera.main;

        // Setup the RenderTexture bridge
        SetupRenderTextureBridge();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].slotNumber = i + 1;
            if (inventorySlots[i].backgroundImage != null)
            {
                inventorySlots[i].rectTransform = inventorySlots[i].backgroundImage.GetComponent<RectTransform>();
                if (inventorySlots[i].rectTransform != null)
                    inventorySlots[i].originalScale = inventorySlots[i].rectTransform.localScale;
            }
            CreateRingProgress(i);
        }

        SetupInventoryItems();
        SelectFirstAvailableSlot();

        // Hide popup panel at start
        if (popupPanel != null)
            popupPanel.SetActive(false);

        Debug.Log("Inventory System Started! Press 1-4 to show/hide item panel");
    }

    void SetupRenderTextureBridge()
    {
        // Create RenderTexture at runtime
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(512, 512, 24);
            renderTexture.antiAliasing = 4;
            renderTexture.Create();
            Debug.Log("RenderTexture created");
        }

        // Create or get render camera
        if (renderCamera == null)
        {
            GameObject cameraObj = new GameObject("PopupRenderCamera");
            renderCamera = cameraObj.AddComponent<Camera>();
            Debug.Log("Render camera created");
        }

        // Configure render camera
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent background
        renderCamera.orthographic = false;
        renderCamera.fieldOfView = 45;
        renderCamera.targetTexture = renderTexture;
        renderCamera.enabled = true;

        // Create world stage if needed
        if (worldStage == null)
        {
            GameObject stage = new GameObject("WorldStage");
            worldStage = stage.transform;
            worldStage.position = new Vector3(1000, 1000, 1000); // Far from game world
            Debug.Log("World stage created");
        }

        // Position camera relative to stage
        renderCamera.transform.SetParent(worldStage);
        renderCamera.transform.localPosition = new Vector3(0, 0, -3);
        renderCamera.transform.localRotation = Quaternion.identity;

        // Set ambient light so objects are visible
        RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.6f);

        // Add lighting to the stage if none exists
        if (worldStage.GetComponentInChildren<Light>() == null)
        {
            // Main directional light
            GameObject mainLight = new GameObject("MainLight");
            mainLight.transform.SetParent(worldStage);
            mainLight.transform.localPosition = new Vector3(2, 3, 2);
            mainLight.transform.localRotation = Quaternion.Euler(50, 45, 0);
            Light light1 = mainLight.AddComponent<Light>();
            light1.type = LightType.Directional;
            light1.intensity = 1.2f;
            light1.shadows = LightShadows.Soft;

            // Fill light from below
            GameObject fillLight = new GameObject("FillLight");
            fillLight.transform.SetParent(worldStage);
            fillLight.transform.localPosition = new Vector3(0, -2, 0);
            Light light2 = fillLight.AddComponent<Light>();
            light2.type = LightType.Point;
            light2.intensity = 0.5f;
            light2.color = new Color(0.8f, 0.8f, 1f);

            // Back rim light
            GameObject rimLight = new GameObject("RimLight");
            rimLight.transform.SetParent(worldStage);
            rimLight.transform.localPosition = new Vector3(0, 1, -2);
            Light light3 = rimLight.AddComponent<Light>();
            light3.type = LightType.Point;
            light3.intensity = 0.6f;
            light3.color = new Color(1f, 0.9f, 0.8f);

            Debug.Log("Stage lighting added");
        }

        // Assign texture to RawImage
        if (panel3DDisplay != null)
        {
            panel3DDisplay.texture = renderTexture;
            Debug.Log("RawImage texture assigned");
        }
        else
        {
            Debug.LogError("Panel 3D Display (RawImage) is not assigned in Inspector!");
        }

        Debug.Log("RenderTexture bridge setup complete");
    }

    Sprite CreateHollowRing()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - size / 2f;
                float dy = y - size / 2f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > size / 2.5f && dist < size / 2f)
                    texture.SetPixel(x, y, Color.white);
                else
                    texture.SetPixel(x, y, Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void CreateRingProgress(int slotIndex)
    {
        if (slotIndex >= inventorySlots.Count) return;
        InventorySlot slot = inventorySlots[slotIndex];
        if (slot.itemIcon == null) return;

        GameObject ringObj = new GameObject("RingProgress");
        ringObj.transform.SetParent(slot.itemIcon.transform, false);

        Image ringImage = ringObj.AddComponent<Image>();
        ringImage.sprite = ringSprite;
        ringImage.color = ringColor;
        ringImage.type = Image.Type.Filled;
        ringImage.fillMethod = Image.FillMethod.Radial360;
        ringImage.fillOrigin = (int)Image.Origin360.Top;
        ringImage.fillClockwise = false;
        ringImage.fillAmount = 0f;
        ringImage.raycastTarget = false;
        ringImage.enabled = false;

        RectTransform rect = ringObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        slot.ringProgressImage = ringImage;
    }

    void SetupInventoryItems()
    {
        if (inventorySlots.Count >= 1)
        {
            inventorySlots[0].slotName = "Lighter";
            if (inventorySlots[0].item == null)
                inventorySlots[0].item = new InventoryItem();
            inventorySlots[0].item.itemName = "Lighter";
            inventorySlots[0].item.itemDescription = "Can light fires";
            inventorySlots[0].item.canHoldToUse = false;
            inventorySlots[0].item.useAction = () => Debug.Log(" Lighter used! Fire started!");
        }

        if (inventorySlots.Count >= 2)
        {
            inventorySlots[1].slotName = "Medkit";
            if (inventorySlots[1].item == null)
                inventorySlots[1].item = new InventoryItem();
            inventorySlots[1].item.itemName = "Medkit";
            inventorySlots[1].item.itemDescription = "HOLD [2] FOR 4 SEC TO USE";
            inventorySlots[1].item.canHoldToUse = true;
            inventorySlots[1].item.useAction = () => Debug.Log(" Medkit used!");
        }

        if (inventorySlots.Count >= 3)
        {
            inventorySlots[2].slotName = "Potion";
            if (inventorySlots[2].item == null)
                inventorySlots[2].item = new InventoryItem();
            inventorySlots[2].item.itemName = "Potion";
            inventorySlots[2].item.itemDescription = "HOLD [3] FOR 4 SEC TO USE";
            inventorySlots[2].item.canHoldToUse = true;
            inventorySlots[2].item.useAction = () => Debug.Log(" Potion used! ");
        }

        if (inventorySlots.Count >= 4)
        {
            inventorySlots[3].slotName = "Key";
            if (inventorySlots[3].item == null)
                inventorySlots[3].item = new InventoryItem();
            inventorySlots[3].item.itemName = "Key";
            inventorySlots[3].item.itemDescription = "Collect 3 keys";
            inventorySlots[3].item.canHoldToUse = false;
            inventorySlots[3].item.useAction = () => Debug.Log(" Key used! Door unlocked!");
        }
    }

    void SelectFirstAvailableSlot()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].item != null)
            {
                SelectSlot(i);
                break;
            }
        }
    }

    void Update()
    {
        // Number key presses - Toggle panel on/off
        for (int i = 1; i <= 4; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                int slotIndex = i - 1;
                if (slotIndex < inventorySlots.Count && inventorySlots[slotIndex] != null && inventorySlots[slotIndex].item != null)
                {
                    SelectSlot(slotIndex);

                    // Toggle panel: if same slot, hide; if different slot, show new
                    if (currentActivePanelSlot == slotIndex && popupPanel.activeSelf)
                    {
                        HidePanel();
                    }
                    else
                    {
                        ShowPanelForSlot(slotIndex);
                    }
                }
            }
        }

        // Hold inputs for Medkit and Potion
        if (Input.GetKey(KeyCode.Alpha2) && currentSelectedSlot == 1 && inventorySlots.Count > 1 && inventorySlots[1].item.canHoldToUse)
        {
            if (!isHolding) StartHolding(1);
        }
        else if (Input.GetKey(KeyCode.Alpha3) && currentSelectedSlot == 2 && inventorySlots.Count > 2 && inventorySlots[2].item.canHoldToUse)
        {
            if (!isHolding) StartHolding(2);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha2) && isHolding && currentHoldingSlot == 1)
        {
            CancelHold(1);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha3) && isHolding && currentHoldingSlot == 2)
        {
            CancelHold(2);
        }

        // Handle hold progress
        if (isHolding && currentHoldingSlot >= 0)
        {
            holdTime += Time.deltaTime;

            if (currentHoldingSlot < inventorySlots.Count && inventorySlots[currentHoldingSlot].ringProgressImage != null)
            {
                float progress = Mathf.Clamp01(holdTime / holdDuration);
                inventorySlots[currentHoldingSlot].ringProgressImage.fillAmount = progress;
            }

            if (holdTime >= holdDuration)
            {
                if (currentHoldingSlot < inventorySlots.Count && inventorySlots[currentHoldingSlot].item != null)
                {
                    inventorySlots[currentHoldingSlot].item.useAction?.Invoke();
                    StartCoroutine(FlashItemIcon(currentHoldingSlot));
                }
                CancelHold(currentHoldingSlot);
            }
        }
    }

    IEnumerator FlashItemIcon(int slotIndex)
    {
        if (slotIndex >= inventorySlots.Count) yield break;

        Image icon = inventorySlots[slotIndex].itemIcon;
        if (icon == null) yield break;

        Color originalColor = icon.color;
        icon.color = Color.green;
        yield return new WaitForSeconds(0.2f);
        icon.color = originalColor;
    }

    void ShowPanelForSlot(int slotIndex)
    {
        InventorySlot slot = inventorySlots[slotIndex];
        if (slot.item == null) return;

        // Update panel text
        if (panelTitleText != null)
            panelTitleText.text = slot.item.itemName;

        if (panelDescriptionText != null)
        {
            panelDescriptionText.text = slot.item.itemDescription;
            panelDescriptionText.color = slot.item.canHoldToUse ? Color.yellow : Color.white;
        }

        // Change panel background color based on item
        Item3DPopupData popupData = GetPopupDataByName(slot.item.itemName);
        if (panelBackground != null && popupData != null)
        {
            panelBackground.color = popupData.backgroundColor;
        }

        // Clear existing 3D object
        if (current3DObject != null)
            Destroy(current3DObject);

        // Stop existing rotation
        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);

        // Find and create new 3D object in WORLD SPACE (not UI container)
        if (popupData != null && popupData.item3DPrefab != null)
        {
            // Create 3D object in world stage (NOT in UI)
            if (worldStage != null)
            {
                current3DObject = Instantiate(popupData.item3DPrefab, worldStage);
                current3DObject.transform.localPosition = popupData.popupOffset;

                // Apply scale with size multiplier for easy size adjustment
                Vector3 finalScale = new Vector3(
                    popupData.popupScale.x * popupData.sizeMultiplier,
                    popupData.popupScale.y * popupData.sizeMultiplier,
                    popupData.popupScale.z * popupData.sizeMultiplier
                );
                current3DObject.transform.localScale = finalScale;
                current3DObject.transform.localRotation = Quaternion.identity;

                // Start rotation animation
                rotationCoroutine = StartCoroutine(Rotate3DObject(current3DObject));

                Debug.Log($"Created 3D object: {slot.item.itemName} with scale: {finalScale}");
            }
            else
            {
                Debug.LogError("WorldStage is null! Make sure it's assigned or created.");
            }
        }
        else
        {
            Debug.LogWarning($"No 3D prefab found for item: {slot.item.itemName}");
        }

        // Show panel with animation
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            StartCoroutine(AnimatePanelIn());
        }

        currentActivePanelSlot = slotIndex;
        Debug.Log($"Showing panel for: {slot.item.itemName}");
    }

    IEnumerator Rotate3DObject(GameObject obj)
    {
        float rotationSpeed = objectRotationSpeed;
        while (obj != null && popupPanel != null && popupPanel.activeSelf)
        {
            if (obj != null)
                obj.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator AnimatePanelIn()
    {
        if (popupPanel == null) yield break;

        RectTransform rect = popupPanel.GetComponent<RectTransform>();
        if (rect == null) yield break;

        // Start from bottom
        Vector2 startPos = new Vector2(0, -500);
        Vector2 endPos = new Vector2(0, 0);
        rect.anchoredPosition = startPos;

        float elapsed = 0;
        float duration = popupAnimationSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = Mathf.SmoothStep(0, 1, t);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);
            yield return null;
        }

        rect.anchoredPosition = endPos;
    }

    IEnumerator AnimatePanelOut()
    {
        if (popupPanel == null) yield break;

        RectTransform rect = popupPanel.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = new Vector2(0, -500);

        float elapsed = 0;
        float duration = popupAnimationSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = Mathf.SmoothStep(0, 1, t);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);
            yield return null;
        }

        popupPanel.SetActive(false);
    }

    void HidePanel()
    {
        StartCoroutine(AnimatePanelOut());

        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }

        if (current3DObject != null)
        {
            Destroy(current3DObject);
            current3DObject = null;
        }

        currentActivePanelSlot = -1;
        Debug.Log("Panel hidden");
    }

    Item3DPopupData GetPopupDataByName(string name)
    {
        foreach (var data in item3DPopups)
        {
            if (data.itemName == name)
                return data;
        }
        return null;
    }

    void SelectSlot(int slotIndex)
    {
        if (currentSelectedSlot != -1 && currentSelectedSlot < inventorySlots.Count)
        {
            var prevSlot = inventorySlots[currentSelectedSlot];
            if (prevSlot != null && prevSlot.rectTransform != null)
            {
                StartCoroutine(ScaleSlot(prevSlot.rectTransform, prevSlot.originalScale));
            }
        }

        currentSelectedSlot = slotIndex;
        if (slotIndex < inventorySlots.Count)
        {
            var newSlot = inventorySlots[slotIndex];
            if (newSlot != null && newSlot.rectTransform != null)
            {
                Vector3 targetScale = newSlot.originalScale * selectedScale;
                StartCoroutine(ScaleSlot(newSlot.rectTransform, targetScale));
            }
            Debug.Log($"Selected: {newSlot?.slotName}");
        }
    }

    IEnumerator ScaleSlot(RectTransform slotRect, Vector3 targetScale)
    {
        if (slotRect == null) yield break;

        Vector3 startScale = slotRect.localScale;
        float elapsed = 0;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * scaleAnimationSpeed;
            float t = Mathf.Min(1, elapsed / duration);
            if (slotRect != null)
                slotRect.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        if (slotRect != null)
            slotRect.localScale = targetScale;
    }

    void StartHolding(int slotIndex)
    {
        isHolding = true;
        currentHoldingSlot = slotIndex;
        holdTime = 0f;

        if (inventorySlots[slotIndex].ringProgressImage != null)
        {
            inventorySlots[slotIndex].ringProgressImage.fillAmount = 0f;
            inventorySlots[slotIndex].ringProgressImage.enabled = true;
        }

        Debug.Log($"Started holding {inventorySlots[slotIndex].item.itemName}");
    }

    void CancelHold(int slotIndex)
    {
        isHolding = false;

        if (slotIndex >= 0 && slotIndex < inventorySlots.Count && inventorySlots[slotIndex].ringProgressImage != null)
        {
            inventorySlots[slotIndex].ringProgressImage.enabled = false;
            inventorySlots[slotIndex].ringProgressImage.fillAmount = 0f;
        }

        currentHoldingSlot = -1;
        holdTime = 0f;

        Debug.Log($"Cancelled hold on slot {slotIndex}");
    }

    void OnDestroy()
    {
        // Clean up render texture
        if (renderTexture != null)
        {
            renderTexture.Release();
        }

        // Clean up camera
        if (renderCamera != null && renderCamera.gameObject != null)
        {
            Destroy(renderCamera.gameObject);
        }

        // Clean up stage
        if (worldStage != null && worldStage.gameObject != null)
        {
            Destroy(worldStage.gameObject);
        }
    }
}