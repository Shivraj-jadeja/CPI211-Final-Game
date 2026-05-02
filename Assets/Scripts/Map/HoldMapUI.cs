using UnityEngine;

public class HoldMapUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform mapHolder;

    [Header("Input")]
    public KeyCode mapKey = KeyCode.Tab;

    [Header("Animation")]
    public Vector2 shownPosition = Vector2.zero;
    public Vector2 hiddenPosition = new Vector2(0f, -600f);
    public float slideSpeed = 10f;

    private bool isShowing = false;

    void Start()
    {
        if (mapHolder != null)
        {
            mapHolder.anchoredPosition = hiddenPosition;
        }
    }

    void Update()
    {
        if (mapHolder == null) return;

        isShowing = Input.GetKey(mapKey);

        Vector2 targetPosition = isShowing ? shownPosition : hiddenPosition;
        mapHolder.anchoredPosition = Vector2.Lerp(
            mapHolder.anchoredPosition,
            targetPosition,
            slideSpeed * Time.deltaTime
        );
    }
}