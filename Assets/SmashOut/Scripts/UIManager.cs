using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("GUI Components")]
    public GameObject MainMenuGuiGameObject;
    public GameObject PauseGuiGameObject, GameOverGuiGameObject;

    public GameState GameStateEnum;

    bool _isClicked;

    // Use this for initialization
    void Start()
    {
        MainMenuGuiGameObject.SetActive(true);
        PauseGuiGameObject.SetActive(false);
        GameOverGuiGameObject.SetActive(false);
        GameStateEnum = GameState.IN_MENU;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && GameStateEnum == GameState.IN_MENU && !_isClicked)
        {
            if (IsButtonClicked())
                return;

            ShowGameplayUI();
            GameManager.S_Instance.SlideCoroutine();
        }
        else if (Input.GetMouseButtonUp(0) && _isClicked && GameStateEnum == GameState.IN_MENU)
            _isClicked = false;
    }

    //show main menu
    public void ShowMainMenuUI()
    {
        ScoreManager.S_Instance.ResetTheCurrentScoreValue();
        _isClicked = true;
        MainMenuGuiGameObject.SetActive(true);
        PauseGuiGameObject.SetActive(false);
        GameOverGuiGameObject.SetActive(false);

        GameStateEnum = GameState.IN_MENU;
        AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.ButtonClickAudio);

        GameManager.S_Instance.OnHomeClickedCoroutine();
    }

    //show gameplay gui
    public void ShowGameplayUI()
    {
        MainMenuGuiGameObject.SetActive(false);
        PauseGuiGameObject.SetActive(true);
        GameOverGuiGameObject.SetActive(false);
        GameStateEnum = GameState.PLAYING_GAME;
        AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.ButtonClickAudio);
        AudioManager.S_Instance.PlayMusicClip(AudioManager.S_Instance.GameMusicAudio);
        GameManager.S_Instance.IsCanMove = true;
    }

    //show game over gui
    public void ShowGameOver()
    {
        MainMenuGuiGameObject.SetActive(false);
        PauseGuiGameObject.SetActive(false);
        GameOverGuiGameObject.SetActive(true);
        GameStateEnum = GameState.GAME_OVER;
    }

    //check if user click any menu button
    public bool IsButtonClicked()
    {
        bool tmp = false;

        var pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (RaycastResult item in results)
        {
            tmp |= item.gameObject.GetComponent<Button>() != null;
        }

        return tmp;
    }
}
