using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager S_Instance { get; set; }
    public Text CurrentScoreText, HighScoreText, CurrentScoreGameOverText, HighScoreGameOverText;

    public int CurrentScoreCounter, HighScoreCounter;
    // Start is called before the first frame update

    bool _isCounting;

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (S_Instance == null)
            S_Instance = this;
        else
            Destroy(gameObject);
    }

    //init and load highscore
    void Start()
    {
        if (!PlayerPrefs.HasKey("HighScore"))
            PlayerPrefs.SetInt("HighScore", 0);

        HighScoreCounter = PlayerPrefs.GetInt("HighScore");

        UpdateTheHighScore();
        ResetTheCurrentScoreValue();
    }

    //save and update highscore
    void UpdateTheHighScore()
    {
        if (CurrentScoreCounter > HighScoreCounter)
            HighScoreCounter = CurrentScoreCounter;

        HighScoreText.text = HighScoreCounter.ToString();
        PlayerPrefs.SetInt("HighScore", HighScoreCounter);
    }

    //update currentscore
    public void UpdateScoreValue(int value)
    {
        CurrentScoreCounter += value;
        CurrentScoreText.text = CurrentScoreCounter.ToString();
    }

    //reset current score
    public void ResetTheCurrentScoreValue()
    {
        CurrentScoreCounter = 0;
        UpdateScoreValue(0);
    }

    //update gameover scores
    public void UpdateScoreGameoverState()
    {
        UpdateTheHighScore();

        CurrentScoreGameOverText.text = CurrentScoreCounter.ToString();
        HighScoreGameOverText.text = HighScoreCounter.ToString();
    }
}
