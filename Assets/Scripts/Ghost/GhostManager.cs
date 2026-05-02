using UnityEngine;

public class GhostManager : MonoBehaviour
{
    public static GhostManager Instance;

    public bool isPlayerHidden = false;
    public HideZone currentHideZone;

    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayerHidden(bool hidden, HideZone hideZone)
    {
        isPlayerHidden = hidden;
        currentHideZone = hidden ? hideZone : null;
    }
}