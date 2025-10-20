using UnityEngine;

public class PlayerSkinApplier : MonoBehaviour
{
    [Header("Player Visuals")]
    public SpriteRenderer playerRenderer;
    public TrailRenderer trailRenderer;
    public Material defaultTrailMaterial; // optional fallback when skin has no trail material

    void OnEnable()
    {
        if (SkinService.Instance != null)
            SkinService.Instance.EquippedChanged += OnEquippedChanged;

        // Apply immediately if possible
        ApplyEquippedNow();
    }

    void Start()
    {
        // Ensure visuals are correct even if OnEnable ran before SkinService finished init
        ApplyEquippedNow();
    }

    void OnDisable()
    {
        if (SkinService.Instance != null)
            SkinService.Instance.EquippedChanged -= OnEquippedChanged;
    }

    void OnEquippedChanged(SkinData skin)
    {
        ApplySkin(skin);
    }

    public void ApplyEquippedNow()
    {
        if (SkinService.Instance != null)
            ApplySkin(SkinService.Instance.Equipped);
    }

    public void ApplySkin(SkinData skin)
    {
        if (skin == null)
            return;

        // Apply color
        if (playerRenderer != null)
            playerRenderer.color = skin.playerColor;

        // Apply optional sprite
        if (playerRenderer != null && skin.playerSprite != null)
            playerRenderer.sprite = skin.playerSprite;

        // Apply trail material if provided, else restore default if set
        if (trailRenderer != null)
        {
            if (skin.trailMaterial != null)
                trailRenderer.material = skin.trailMaterial;
            else if (defaultTrailMaterial != null)
                trailRenderer.material = defaultTrailMaterial;
        }
    }
}


