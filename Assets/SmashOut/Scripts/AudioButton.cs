using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class AudioButton : MonoBehaviour
{
    public bool _isEfx;
    public Sprite _musicOnSpriteAsset, _musicOffSpriteAsset, _efxOnSpriteAsset, _efxOffSpriteAsset;
    public Image _spriteButtonImage;


    //set button sprite
    void Start()
    {
        SetButtonSpriteDisplay();
    }

    void OnEnable()
    {
        StartCoroutine(RefreshNextFrame());
    }

    IEnumerator RefreshNextFrame()
    {
        yield return null;
        SetButtonSpriteDisplay();
    }

    public void OnMusicButtonClicked()
    {
        AudioManager.S_Instance.MuteMusicEffect();
        AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.ButtonClickAudio);
        SetButtonSpriteDisplay();
    }

    public void OnEfxButtonClicked()
    {
        AudioManager.S_Instance.MuteEfx();
        AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.ButtonClickAudio);
        SetButtonSpriteDisplay();
    }

    void SetButtonSpriteDisplay()
    {
        if ((!AudioManager.S_Instance.IsMusicMuted() && !_isEfx) || (!AudioManager.S_Instance.IsEfxMuted() && _isEfx))
            if (_isEfx)
                _spriteButtonImage.sprite = _efxOnSpriteAsset;
            else
                _spriteButtonImage.sprite = _musicOnSpriteAsset;
        else
            if (_isEfx)
            _spriteButtonImage.sprite = _efxOffSpriteAsset;
        else
            _spriteButtonImage.sprite = _musicOffSpriteAsset;
    }
}
