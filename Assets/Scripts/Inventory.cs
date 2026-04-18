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
        public Vector3 popupScale = new Vector3(0.8f, 0.8f, 0.8f);
    }

    [Header("Inventory Slots")]
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    [Header("3D Popup Panel")]
    public GameObject popupPanel;              // The UI Panel that shows/hides
    public Image panelBackground;              // Panel background image
    public TextMeshProUGUI panelTitleText;     // Text at top (item name)
    public Image panelItemImage;               // 3D object will be placed here
    public TextMeshProUGUI panelDescriptionText; // Text at bottom (description)
    public Transform panel3DContainer;         // Where the 3D object goes

    [Header("3D Popup Settings")]
    public List<Item3DPopupData> item3DPopups = new List<Item3DPopupData>();
    public Camera playerCamera;
    public float popupAnimationSpeed = 0.3f;

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
    private int currentActivePanelSlot = -1;  // Track which slot's panel is showing

    void Start()
    {
        ringSprite = CreateHollowRing();

        if (playerCamera == null)
            playerCamera = Camera.main;

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
            inventorySlots[0].item.useAction = () => Debug.Log(" Lighter used!");
        }

        if (inventorySlots.Count >= 2)
        {
            inventorySlots[1].slotName = "Medkit";
            if (inventorySlots[1].item == null)
                inventorySlots[1].item = new InventoryItem();
            inventorySlots[1].item.itemName = "Medkit";
            inventorySlots[1].item.itemDescription = "HOLD [2] FOR 4 SEC TO USE";
            inventorySlots[1].item.canHoldToUse = true;
            inventorySlots[1].item.useAction = () => Debug.Log(" Medkit used! +50 HP");
        }

        if (inventorySlots.Count >= 3)
        {
            inventorySlots[2].slotName = "Potion";
            if (inventorySlots[2].item == null)
                inventorySlots[2].item = new InventoryItem();
            inventorySlots[2].item.itemName = "Potion";
            inventorySlots[2].item.itemDescription = "HOLD [3] FOR 4 SEC TO USE";
            inventorySlots[2].item.canHoldToUse = true;
            inventorySlots[2].item.useAction = () => Debug.Log(" Potion used!");
        }

        if (inventorySlots.Count >= 4)
        {
            inventorySlots[3].slotName = "Key";
            if (inventorySlots[3].item == null)
                inventorySlots[3].item = new InventoryItem();
            inventorySlots[3].item.itemName = "Key";
            inventorySlots[3].item.itemDescription = "";
            inventorySlots[3].item.canHoldToUse = false;
            inventorySlots[3].item.useAction = () => Debug.Log(" Key used!");
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
                }
                CancelHold(currentHoldingSlot);
            }
        }
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

        // Clear existing 3D object
        if (current3DObject != null)
            Destroy(current3DObject);

        // Find and create new 3D object
        Item3DPopupData popupData = GetPopupDataByName(slot.item.itemName);
        if (popupData != null && popupData.item3DPrefab != null)
        {
            // Create 3D object in the panel's container
            if (panel3DContainer != null)
            {
                current3DObject = Instantiate(popupData.item3DPrefab, panel3DContainer);
                current3DObject.transform.localPosition = Vector3.zero;
                current3DObject.transform.localScale = popupData.popupScale;
                current3DObject.transform.localRotation = Quaternion.identity;

                // Make it rotate slowly
                StartCoroutine(Rotate3DObject(current3DObject));
            }
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
        float rotationSpeed = 30f;
        while (obj != null && obj.activeSelf)
        {
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
        float duration = 0.3f;

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
        float duration = 0.3f;

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
    }
}