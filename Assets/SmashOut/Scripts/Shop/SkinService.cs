using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkinService : MonoBehaviour
{
    public static SkinService Instance { get; private set; }

    [Header("Catalog (optional)")]
    public SkinCatalog catalog; // If assigned, Initialize() is called automatically

    // Events
    public event Action<SkinData> EquippedChanged;
    public event Action<HashSet<string>> UnlockedChanged;
    public event Action<int> HighScoreChanged;

    // Public state
    public SkinData Equipped { get; private set; }
    public IReadOnlyList<SkinData> All => _allSkins;
    public HashSet<string> UnlockedIds => _unlockedIds;
    public int HighScore => _highScore;

    // Internal state
    readonly List<SkinData> _allSkins = new List<SkinData>();
    readonly HashSet<string> _unlockedIds = new HashSet<string>();
    int _highScore;

    // PlayerPrefs keys
    const string KeyEquipped = "SkinService.EquippedSkinId";
    const string KeyUnlocked = "SkinService.UnlockedSkinIds";
    const string KeyHighScore = "SkinService.HighScore";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        // Khởi tạo trong Start() để đảm bảo tất cả objects đã được setup
        if (catalog != null)
            Initialize(catalog);
    }

    // Initialize from a catalog (idempotent; safe to call multiple times)
    public void Initialize(SkinCatalog skinCatalog)
    {
        _allSkins.Clear();
        if (skinCatalog != null && skinCatalog.skins != null)
            _allSkins.AddRange(skinCatalog.skins.Where(s => s != null));

        LoadState();

        // Unlock all skins from the beginning
        foreach (var skin in _allSkins)
        {
            if (!string.IsNullOrEmpty(GetId(skin)))
                _unlockedIds.Add(GetId(skin));
        }

        // Pick equipped: saved id, else first default, else first skin
        Equipped = ResolveEquipped();
        SaveState();

        EquippedChanged?.Invoke(Equipped);
        UnlockedChanged?.Invoke(new HashSet<string>(_unlockedIds));
        HighScoreChanged?.Invoke(_highScore);
    }

    public void OnScoreCommitted(int highScore)
    {
        if (highScore <= _highScore)
            return;
        _highScore = highScore;
        PlayerPrefs.SetInt(KeyHighScore, _highScore);
        HighScoreChanged?.Invoke(_highScore);

        // Auto-unlock by score
        bool changed = false;
        foreach (var skin in _allSkins)
        {
            if (skin == null) continue;
            var id = GetId(skin);
            if (string.IsNullOrEmpty(id)) continue;
            if (_unlockedIds.Contains(id)) continue;
            if (CanUnlock(skin))
            {
                _unlockedIds.Add(id);
                changed = true;
            }
        }
        if (changed)
        {
            SaveUnlocked();
            UnlockedChanged?.Invoke(new HashSet<string>(_unlockedIds));
        }
    }

    public bool IsUnlocked(SkinData skin)
    {
        if (skin == null) return false;
        if (skin.isDefault) return true;
        var id = GetId(skin);
        return !string.IsNullOrEmpty(id) && _unlockedIds.Contains(id);
    }

    public bool CanUnlock(SkinData skin)
    {
        if (skin == null) return false;
        if (IsUnlocked(skin)) return false;
        return _highScore >= Mathf.Max(0, skin.requiredScore);
    }

    public bool TryUnlock(SkinData skin)
    {
        if (!CanUnlock(skin))
            return false;
        var id = GetId(skin);
        if (string.IsNullOrEmpty(id))
            return false;
        if (_unlockedIds.Add(id))
        {
            SaveUnlocked();
            UnlockedChanged?.Invoke(new HashSet<string>(_unlockedIds));
            return true;
        }
        return false;
    }

    public bool TryEquip(SkinData skin)
    {
        if (skin == null) 
        {
            Debug.LogWarning("TryEquip: skin is null");
            return false;
        }
        
        if (!IsUnlocked(skin)) 
        {
            Debug.LogWarning($"TryEquip: skin {skin.displayName} is not unlocked");
            return false;
        }
        
        if (Equipped == skin) 
        {
            Debug.Log($"TryEquip: skin {skin.displayName} is already equipped");
            return true;
        }

        var oldEquipped = Equipped;
        Equipped = skin;
        PlayerPrefs.SetString(KeyEquipped, GetId(skin));
        PlayerPrefs.Save(); // Đảm bảo lưu ngay lập tức
        
        Debug.Log($"TryEquip: Successfully equipped {skin.displayName} (was {(oldEquipped?.displayName ?? "none")})");
        
        EquippedChanged?.Invoke(Equipped);
        return true;
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(KeyEquipped);
        PlayerPrefs.DeleteKey(KeyUnlocked);
        PlayerPrefs.DeleteKey(KeyHighScore);
        _unlockedIds.Clear();
        _highScore = 0;

        foreach (var skin in _allSkins)
        {
            if (skin != null && skin.isDefault && !string.IsNullOrEmpty(GetId(skin)))
                _unlockedIds.Add(GetId(skin));
        }
        Equipped = ResolveEquipped();

        EquippedChanged?.Invoke(Equipped);
        UnlockedChanged?.Invoke(new HashSet<string>(_unlockedIds));
        HighScoreChanged?.Invoke(_highScore);
    }

    // --- Persistence helpers ---

    void LoadState()
    {
        _highScore = PlayerPrefs.GetInt(KeyHighScore, 0);
        _unlockedIds.Clear();
        var unlockedCsv = PlayerPrefs.GetString(KeyUnlocked, string.Empty);
        if (!string.IsNullOrEmpty(unlockedCsv))
        {
            foreach (var part in unlockedCsv.Split(','))
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    _unlockedIds.Add(trimmed);
            }
        }
        // Equipped will be resolved after catalog load
    }

    void SaveState()
    {
        SaveUnlocked();
        if (Equipped != null)
            PlayerPrefs.SetString(KeyEquipped, GetId(Equipped));
        PlayerPrefs.SetInt(KeyHighScore, _highScore);
    }

    void SaveUnlocked()
    {
        var csv = string.Join(",", _unlockedIds);
        PlayerPrefs.SetString(KeyUnlocked, csv);
    }

    SkinData ResolveEquipped()
    {
        var savedId = PlayerPrefs.GetString(KeyEquipped, string.Empty);
        if (!string.IsNullOrEmpty(savedId))
        {
            var saved = _allSkins.FirstOrDefault(s => GetId(s) == savedId);
            if (saved != null && (IsUnlocked(saved) || saved.isDefault))
                return saved;
        }
        var firstDefault = _allSkins.FirstOrDefault(s => s != null && s.isDefault);
        if (firstDefault != null)
            return firstDefault;
        return _allSkins.FirstOrDefault(s => s != null);
    }

    static string GetId(SkinData skin)
    {
        return skin != null ? (!string.IsNullOrEmpty(skin.id) ? skin.id : skin.name) : string.Empty;
    }
}


