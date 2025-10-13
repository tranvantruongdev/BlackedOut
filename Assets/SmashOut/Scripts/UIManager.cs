using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [Header("GUI Components")]
    public GameObject mainMenuGui;
    public GameObject gameplayGui, gameOverGui;

    public GameState gameState;

    bool clicked;

    // Use this for initialization
    void Start()
    {
        mainMenuGui.SetActive(true);
        gameplayGui.SetActive(false);
        gameOverGui.SetActive(false);
        gameState = GameState.IN_MENU;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && gameState == GameState.IN_MENU && !clicked)
        {
            if (IsButton())
                return;

            ShowGameplay();
            GameManager.Instance.Slide();
        }
        else if (Input.GetMouseButtonUp(0) && clicked && gameState == GameState.IN_MENU)
            clicked = false;
    }

    //show main menu
    public void ShowMainMenu()
    {
        ScoreManager.Instance.ResetCurrentScore();
        clicked = true;
        mainMenuGui.SetActive(true);
        gameplayGui.SetActive(false);
        gameOverGui.SetActive(false);

        gameState = GameState.IN_MENU;
        AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.ButtonClickAudio);

        GameManager.Instance.OnHomeClicked();
    }

    //show gameplay gui
    public void ShowGameplay()
    {
        mainMenuGui.SetActive(false);
        gameplayGui.SetActive(true);
        gameOverGui.SetActive(false);
        gameState = GameState.PLAYING_GAME;
        AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.ButtonClickAudio);
        AudioManager.S_Instance.PlayMusicClip(AudioManager.S_Instance.GameMusicAudio);
        GameManager.Instance.canMove = true;
    }

    //show game over gui
    public void ShowGameOver()
    {
        mainMenuGui.SetActive(false);
        gameplayGui.SetActive(false);
        gameOverGui.SetActive(true);
        gameState = GameState.GAME_OVER;
    }

    //check if user click any menu button
    public bool IsButton()
    {
        bool temp = false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult item in results)
        {
            temp |= item.gameObject.GetComponent<Button>() != null;
        }

        return temp;
    }
}
