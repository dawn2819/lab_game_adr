using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int Score = 0;
    public int HighScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateHighScore(HighScore);
        }
    }

    public void AddScore(int amount)
    {
        Score += amount;
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateScore(Score);
        }

        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
            PlayerPrefs.Save();
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.UpdateHighScore(HighScore);
            }
        }
    }
}
