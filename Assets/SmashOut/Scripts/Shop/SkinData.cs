using UnityEngine;

[CreateAssetMenu(fileName = "SkinData", menuName = "JumpUp/Shop/Skin Data")]
public class SkinData : ScriptableObject
{
    [Header("Identity")]
    public string id; // stable id for persistence (can mirror asset GUID via editor tool)
    public string displayName;
    public Sprite icon;

    [Header("Unlock")]
    public int requiredScore;
    public bool isDefault;

    [Header("Visuals")]
    public Color playerColor = Color.white;
    public Sprite playerSprite; // optional
    public Material trailMaterial; // optional
}


