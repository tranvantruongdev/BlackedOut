using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [Header("UI Refs")]
    public Image icon;
    public Text nameText;
    public GameObject lockedIndicator;
    public GameObject selectedIndicator;
    public GameObject equippedIndicator;
    public Button selectButton;

    public SkinData Skin { get; private set; }
    ShopPanel _panel;

    public void Initialize(SkinData skin, ShopPanel panel)
    {
        Skin = skin;
        _panel = panel;
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnClick);
        }
        Refresh();
    }

    public void Refresh()
    {
        if (Skin == null) return;
        if (icon != null) icon.sprite = Skin.icon;
        if (nameText != null) nameText.text = string.IsNullOrEmpty(Skin.displayName) ? Skin.name : Skin.displayName;

        bool isUnlocked = SkinService.Instance != null && SkinService.Instance.IsUnlocked(Skin);
        bool isEquipped = SkinService.Instance != null && SkinService.Instance.Equipped == Skin;

        if (lockedIndicator != null) lockedIndicator.SetActive(!isUnlocked);
        if (equippedIndicator != null) equippedIndicator.SetActive(isEquipped);
        // selectedIndicator is toggled by panel when user selects
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(selected);
    }

    void OnClick()
    {
        if (_panel != null)
            _panel.SetSelected(Skin);
    }
}


