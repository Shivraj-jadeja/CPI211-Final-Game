using UnityEngine;

public class MapTracker : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public RectTransform mapPaperRect;
    public RectTransform playerMarkerRect;

    [Header("World Bounds")]
    public float worldMinX = 1f;
    public float worldMaxX = 1000f;
    public float worldMinZ = -1f;
    public float worldMaxZ = 999f;

    [Header("Map Settings")]
    public bool invertX = true;
    public bool invertZ = true;
    public Vector2 markerOffset;

    void Update()
    {
        if (player == null || mapPaperRect == null || playerMarkerRect == null) return;

        float normalizedX = Mathf.InverseLerp(worldMinX, worldMaxX, player.position.x);
        float normalizedZ = Mathf.InverseLerp(worldMinZ, worldMaxZ, player.position.z);

        if (invertX) normalizedX = 1f - normalizedX;
        if (invertZ) normalizedZ = 1f - normalizedZ;

        float mapX = (normalizedX * mapPaperRect.rect.width) - (mapPaperRect.rect.width / 2f);
        float mapY = (normalizedZ * mapPaperRect.rect.height) - (mapPaperRect.rect.height / 2f);

        playerMarkerRect.anchoredPosition = new Vector2(mapX, mapY) + markerOffset;

        float yaw = player.eulerAngles.y;
        playerMarkerRect.localRotation = Quaternion.Euler(0f, 0f, -yaw + 180f);
    }
}