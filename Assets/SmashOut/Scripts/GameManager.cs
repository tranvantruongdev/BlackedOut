using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager S_Instance { get; set; }

    public UIManager UIManagerReference;
    public ScoreManager ScoreManagerReference;
    public SkinService SkinServiceReference;

    [Header("Game settings")]
    public GameObject PlayerHolder;
    public GameObject GameOverlayHolder;
    [Space(5)]
    public GameObject ObstaclePrefab;
    [Space(5)]
    public int MinNumberOfSmasherConfig = 6;
    public int MaxNumberOfSmasherConfig = 10;//can be higher value, but then the player ball needs to be smaller (depends on screen width)
    [Space(5)]
    public List<GameObject> UpperGameObjects, BottomGameObjects;
    [Space(5)]
    public float MoveDistanceConfig = 3f; //distance between upper and bottom smashers when they are max opened
    [Space(5)]
    public float ObstacleMoveSpeedConfig = 0.3f;
    public bool IsCanMove;

    public List<float> SmashersStartPositionConfigs, SmashersTargetPositionConfigs;
    GameObject _lastObstacle;
    Vector3 _screenSizeVector3;
    int _indexShorterObstacle, _currentPositionInt, _numberOfObstacles;
    bool _isPositionUp; //false - down, true - up

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (S_Instance == null)
            S_Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Đảm bảo SkinService được khởi tạo trước khi game bắt đầu
        if (SkinServiceReference != null && SkinServiceReference.catalog != null)
        {
            SkinServiceReference.Initialize(SkinServiceReference.catalog);
        }

        InitializeGame();
    }

    void InitializeGame()
    {
        Physics2D.gravity = new Vector2(0, 0f);

        Application.targetFrameRate = 30;

        _screenSizeVector3 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        // Đảm bảo PlayerHolder có PlayerSkinApplier
        SetupPlayerSkinApplier();

        StartCoroutine(CreateTheScene());
    }

    void SetupPlayerSkinApplier()
    {
        if (PlayerHolder != null)
        {
            var playerSkinApplier = PlayerHolder.GetComponent<PlayerSkinApplier>();
            if (playerSkinApplier == null)
            {
                playerSkinApplier = PlayerHolder.AddComponent<PlayerSkinApplier>();

                // Setup references
                var spriteRenderer = PlayerHolder.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    playerSkinApplier.playerRenderer = spriteRenderer;
                }

                // Tìm TrailRenderer nếu có
                var trailRenderer = PlayerHolder.GetComponent<TrailRenderer>();
                if (trailRenderer != null)
                {
                    playerSkinApplier.trailRenderer = trailRenderer;
                }

                Debug.Log("Added PlayerSkinApplier to PlayerHolder");
            }

            // Áp dụng skin hiện tại nếu có
            if (SkinServiceReference != null && SkinServiceReference.Equipped != null)
            {
                playerSkinApplier.ApplySkin(SkinServiceReference.Equipped);
                Debug.Log($"Applied initial skin to player: {SkinServiceReference.Equipped.displayName}");
            }
        }
    }

    void Update()
    {
        if (UIManagerReference.GameStateEnum == GameState.PLAYING_GAME && Input.GetMouseButtonDown(0) && IsCanMove)
        {
            if (UIManagerReference.IsButtonClicked())
                return;

            if (Input.mousePosition.x > Screen.width * 0.5f)
            {
                if (_currentPositionInt < _numberOfObstacles)
                {
                    _currentPositionInt++;
                    AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.ButtonClickAudio);
                }
            }
            else
            {
                if (_currentPositionInt > 0)
                {
                    _currentPositionInt--;
                    AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.ButtonClickAudio);
                }
            }

            if (_currentPositionInt >= _numberOfObstacles)
                _currentPositionInt = _numberOfObstacles - 1;

            IsCanMove = false;
            PlayerHolder.transform.position = new Vector2(BottomGameObjects[_currentPositionInt].transform.position.x, BottomGameObjects[_currentPositionInt].transform.position.y + BottomGameObjects[_currentPositionInt].transform.localScale.y / 2 + PlayerHolder.GetComponent<SpriteRenderer>().size.y / 2);
        }

        if (UIManagerReference.GameStateEnum == GameState.MOVING_SMASHERS)
        {
            for (int i = 0; i < UpperGameObjects.Count; i++)
            {
                UpperGameObjects[i].transform.position = Vector2.Lerp(UpperGameObjects[i].transform.position, new Vector2(UpperGameObjects[i].transform.position.x, SmashersTargetPositionConfigs[i]), ObstacleMoveSpeedConfig);
            }

            //check if smashers reach position
            if (Mathf.Abs(UpperGameObjects[0].transform.position.y - SmashersTargetPositionConfigs[0]) < .001f)
            {
                for (int i = 0; i < UpperGameObjects.Count; i++)
                {
                    UpperGameObjects[i].transform.position = new Vector2(UpperGameObjects[i].transform.position.x, SmashersTargetPositionConfigs[i]);
                }

                _isPositionUp = !_isPositionUp;

                if (!_isPositionUp)
                {
                    StartCoroutine(NewScene());
                    return;
                }
                else
                {
                    _currentPositionInt = Random.Range(0, _numberOfObstacles);
                    PlayerHolder.transform.position = new Vector2(BottomGameObjects[_currentPositionInt].transform.position.x, BottomGameObjects[_currentPositionInt].transform.position.y + BottomGameObjects[_currentPositionInt].transform.localScale.y / 2 + PlayerHolder.GetComponent<SpriteRenderer>().size.y / 2);
                    ShowThePlayer();
                }

                if (UIManagerReference.MainMenuGuiGameObject.activeInHierarchy) //if main menu is active
                    UIManagerReference.GameStateEnum = GameState.IN_MENU;
                else
                {
                    StartCoroutine(SlideDownAction());
                    UIManagerReference.GameStateEnum = GameState.PLAYING_GAME;
                }
            }
        }

        if (UIManagerReference.GameStateEnum == GameState.PLAYING_GAME && Input.GetMouseButtonUp(0))
        {
            IsCanMove = true;
        }
    }

    public void OnHomeClickedCoroutine()
    {
        StartCoroutine(RestartTheGame(false));
    }

    //create new scene
    IEnumerator CreateTheScene()
    {
        //Debug.Log("Creating new scene...");

        HideThePlayer();

        _isPositionUp = false;

        UIManagerReference.GameStateEnum = GameState.CREATING_SCENE;

        _numberOfObstacles = Random.Range(MinNumberOfSmasherConfig, MaxNumberOfSmasherConfig + 1);

        float smasherWidth = (_screenSizeVector3.x * 2) / _numberOfObstacles; // calculate smasher width
        float smasherHeight;
        //Debug.Log(obstacleWidth);

        _indexShorterObstacle = Random.Range(0, _numberOfObstacles); //choose index for shorter smasher

        //create bottom smashers
        for (int i = 0; i < _numberOfObstacles; i++)
        {
            smasherHeight = Random.Range(_screenSizeVector3.y - .6f * _screenSizeVector3.y, _screenSizeVector3.y - .2f * _screenSizeVector3.y); //random height depends on screen height and percent of screen height - in this case .4f of screen height
            _lastObstacle = Instantiate(ObstaclePrefab);
            _lastObstacle.transform.localScale = new Vector2(smasherWidth, smasherHeight);
            _lastObstacle.transform.position = new Vector2(-_screenSizeVector3.x + ((i + .5f) * smasherWidth), -_screenSizeVector3.y + smasherHeight / 2);
            BottomGameObjects.Add(_lastObstacle);
        }

        //create top smashers
        for (int i = 0; i < _numberOfObstacles; i++)
        {
            smasherHeight = 2 * _screenSizeVector3.y - BottomGameObjects[i].transform.localScale.y;

            _lastObstacle = Instantiate(ObstaclePrefab);
            _lastObstacle.transform.localScale = new Vector2(smasherWidth, smasherHeight);

            if (i == _indexShorterObstacle) //one of the smashers needs to be put a little higher than others shorter
            {
                _lastObstacle.transform.position = new Vector2(-_screenSizeVector3.x + ((i + .5f) * smasherWidth), BottomGameObjects[i].transform.position.y + BottomGameObjects[i].transform.localScale.y / 2 + _lastObstacle.transform.localScale.y / 2 + 1f);
            }
            else
                _lastObstacle.transform.position = new Vector2(-_screenSizeVector3.x + ((i + .5f) * smasherWidth), BottomGameObjects[i].transform.position.y + BottomGameObjects[i].transform.localScale.y / 2 + _lastObstacle.transform.localScale.y / 2);

            SmashersStartPositionConfigs.Add(_lastObstacle.transform.position.y); //save last smasher position
            SmashersTargetPositionConfigs.Add(_lastObstacle.transform.position.y + MoveDistanceConfig);

            UpperGameObjects.Add(_lastObstacle);
        }

        AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.SlideAudio);
        UIManagerReference.GameStateEnum = GameState.MOVING_SMASHERS;

        yield return new WaitForSeconds(.1f);

        GameOverlayHolder.GetComponent<Animator>().Play("GameOverlayHide");
    }

    public void SlideCoroutine()
    {
        StartCoroutine(SlideDownAction());
    }

    IEnumerator NewScene()
    {
        UIManagerReference.GameStateEnum = GameState.CREATING_SCENE;
        ScoreManager.S_Instance.UpdateScoreValue(1);

        yield return new WaitForSeconds(.6f);

        ClearTheScene();
        StartCoroutine(CreateTheScene());
    }

    IEnumerator SlideDownAction()
    {
        AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.CounterAudio);

        for (int i = 0; i < _numberOfObstacles; i++)
        {
            SmashersTargetPositionConfigs[i] = SmashersStartPositionConfigs[i];
        }

        yield return new WaitForSeconds(3.5f);

        AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.SlideAudio);
        UIManagerReference.GameStateEnum = GameState.MOVING_SMASHERS;
    }

    public void ShowThePlayer()
    {
        PlayerHolder.GetComponent<SpriteRenderer>().enabled = true;
        PlayerHolder.GetComponent<CircleCollider2D>().enabled = true;

        // Đảm bảo skin được áp dụng khi hiển thị player
        var playerSkinApplier = PlayerHolder.GetComponent<PlayerSkinApplier>();
        if (playerSkinApplier != null && SkinServiceReference != null && SkinServiceReference.Equipped != null)
        {
            playerSkinApplier.ApplySkin(SkinServiceReference.Equipped);
        }
    }

    public void HideThePlayer()
    {
        PlayerHolder.GetComponent<SpriteRenderer>().enabled = false;
        PlayerHolder.GetComponent<CircleCollider2D>().enabled = false;
    }

    //restart game, reset score, update platform position
    public void RestartTheGame()
    {
        StartCoroutine(RestartTheGame(true));
    }

    IEnumerator RestartTheGame(bool startGame)
    {
        GameOverlayHolder.GetComponent<Animator>().Play("GameOverlayShow");

        yield return new WaitForSeconds(.6f);

        if (startGame)
            UIManagerReference.ShowGameplayUI();

        ClearTheScene();
        ScoreManagerReference.ResetTheCurrentScoreValue();
        StartCoroutine(CreateTheScene());
    }


    //clear all scene elements
    public void ClearTheScene()
    {
        SmashersStartPositionConfigs.Clear();
        SmashersTargetPositionConfigs.Clear();
        UpperGameObjects.Clear();
        BottomGameObjects.Clear();

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }
    }

    //show game over gui
    public void GameOverAction()
    {
        if (UIManagerReference.GameStateEnum == GameState.PLAYING_GAME || UIManagerReference.GameStateEnum == GameState.MOVING_SMASHERS)
        {
            StopAllCoroutines();
            AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.GameOverAudio);
            AudioManager.S_Instance.PlayEffectsAudio(AudioManager.S_Instance.SmashAudio);
            AudioManager.S_Instance.PlayMusicClip(AudioManager.S_Instance.MenuMusicAudio);
            UIManagerReference.ShowGameOver();
            PlayerHolder.GetComponent<SpriteRenderer>().enabled = false;
            ScoreManagerReference.UpdateScoreGameoverState();
        }
    }
}
