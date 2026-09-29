using UnityEngine;

/// <summary>
/// Điều phối 1 lượt chơi trong SampleScene: nạp cấp độ (nền, vật lý, NPC), đếm mục tiêu,
/// xử lý thắng/thua và lưu tiến trình. Sống theo scene; tiến trình lâu dài nằm trong SaveSystem.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Cấp độ được chọn từ menu; -1 = chơi tiếp cấp cao nhất đã mở
    public static int SelectedLevel = -1;

    public int Score = 0;
    public int HighScore = 0;

    public int LevelIndex { get; private set; }
    public LevelConfig Level => LevelConfig.All[LevelIndex];
    public int Kills { get; private set; }
    public float Elapsed { get; private set; }
    public bool IsOver { get; private set; }

    public static bool IsPlaying => Instance == null || !Instance.IsOver;

    private PlayerA player;
    private bool tookDamage = false;
    private int lastShownSecond = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        HighScore = PlayerPrefs.GetInt("HighScore", 0);

        int wanted = SelectedLevel >= 0 ? SelectedLevel : SaveSystem.Load().unlockedLevel;
        LevelIndex = Mathf.Clamp(wanted, 0, LevelConfig.Count - 1);
        ApplyLevel();

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateScore(Score);
            HUDManager.Instance.UpdateHighScore(HighScore);
            HUDManager.Instance.ShowLevelBanner("LEVEL " + (LevelIndex + 1) + " - " + Level.name, Level.description);
        }
        RefreshObjective();
    }

    /// <summary>Áp cấu hình cấp độ lên các hệ thống đã có trong scene.</summary>
    private void ApplyLevel()
    {
        LevelConfig cfg = Level;

        // 1. Trạng thái A mang từ cấp trước (HP, Ki, điểm)
        player = FindFirstObjectByType<PlayerA>();
        SaveSystem.PlayerState entry = SaveSystem.EntryState(LevelIndex);
        Score = entry.score;
        if (player != null)
        {
            player.currentHp = Mathf.Clamp(entry.hp, 1f, player.maxHp);
            player.currentKi = Mathf.Clamp(entry.ki, 0f, player.maxKi);
            player.gravity *= cfg.gravityScale;
            player.windX = cfg.windX;
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.UpdateHP(player.currentHp, player.maxHp);
                HUDManager.Instance.UpdateKi(player.currentKi, player.maxKi);
            }
        }

        // 2. Thế giới: nền riêng của từng cấp + màu trời
        BackgroundManager bg = FindFirstObjectByType<BackgroundManager>();
        if (bg != null)
        {
            Sprite top = string.IsNullOrEmpty(cfg.topSprite) ? null : Resources.Load<Sprite>(cfg.topSprite);
            Sprite ground = string.IsNullOrEmpty(cfg.groundSprite) ? null : Resources.Load<Sprite>(cfg.groundSprite);
            if (top != null) bg.bgTopSprite = top;
            if (ground != null) bg.bgGroundSprite = ground;
            bg.scrollSpeed = cfg.scrollSpeed;
            bg.SetupPieces();
        }
        if (Camera.main != null) Camera.main.backgroundColor = cfg.skyColor;

        // 3. Cơ chế: tốc độ bệ bay, tần suất vật phẩm
        ObstacleManager om = FindFirstObjectByType<ObstacleManager>();
        if (om != null)
        {
            om.minFlySpeed *= cfg.obstacleSpeedScale;
            om.maxFlySpeed *= cfg.obstacleSpeedScale;
        }
        ItemSpawner spawner = FindFirstObjectByType<ItemSpawner>();
        if (spawner != null) spawner.spawnInterval = cfg.itemInterval;

        // 4. Đội hình NPC của cấp độ
        if (player != null)
        {
            player.enemyBCount = cfg.enemies.Length;
            player.EnsureEnemyBCount();
        }
        EnemyB[] enemies = FindObjectsByType<EnemyB>(FindObjectsSortMode.InstanceID);
        for (int i = 0; i < enemies.Length; i++)
        {
            if (i < cfg.enemies.Length) enemies[i].Configure(cfg.enemies[i], cfg.enemySpeedScale);
            else Destroy(enemies[i].gameObject);
        }
    }

    private void Update()
    {
        if (IsOver) return;

        Elapsed += Time.deltaTime;
        if (Level.timeLimit > 0f)
        {
            if (Elapsed >= Level.timeLimit)
            {
                GameOver("TIME UP!");
                return;
            }
            int sec = Mathf.CeilToInt(Level.timeLimit - Elapsed);
            if (sec != lastShownSecond)
            {
                lastShownSecond = sec;
                RefreshObjective();
            }
        }
    }

    private void RefreshObjective()
    {
        if (HUDManager.Instance == null) return;
        string text = "LV" + (LevelIndex + 1) + " " + Level.name + "   DEFEAT " + Kills + "/" + Level.killsToWin;
        if (Level.timeLimit > 0f) text += "   TIME " + MenuScreen.FormatTime(Level.timeLimit - Elapsed);
        HUDManager.Instance.SetObjective(text);
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

    /// <summary>A hạ gục 1 NPC: cộng điểm, tính mục tiêu, đủ số lượng thì thắng.</summary>
    public void RegisterKill(int scoreReward)
    {
        if (IsOver) return;
        Kills++;
        AddScore(scoreReward);
        RefreshObjective();
        if (Kills >= Level.killsToWin) Win();
    }

    public void OnPlayerDamaged()
    {
        tookDamage = true;
    }

    public void OnPlayerDied()
    {
        GameOver("YOU WERE DEFEATED");
    }

    private void GameOver(string reason)
    {
        if (IsOver) return;
        IsOver = true;
        Time.timeScale = 0f;

        SaveSystem.RecordRun(LevelIndex, false, Score, Kills, Elapsed, null, false);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowEndPanel(false, "GAME OVER", reason + "\nScore " + Score + "   Kills " + Kills + "/" + Level.killsToWin + "   Time " + MenuScreen.FormatTime(Elapsed),
                ("REPLAY", Replay),
                ("HOME", () => MenuScreen.Go(MenuScreen.HomeScene)),
                ("SETTINGS", () => MenuScreen.Go(MenuScreen.SettingsScene)),
                ("HISTORY", () => MenuScreen.Go(MenuScreen.HistoryScene)));
        }
    }

    private void Win()
    {
        if (IsOver) return;
        IsOver = true;
        Time.timeScale = 0f;

        // Lưu trạng thái A sau khi hoàn thành cấp để mang sang cấp tiếp theo
        SaveSystem.PlayerState end = new SaveSystem.PlayerState
        {
            hp = player != null ? player.currentHp : 100f,
            ki = player != null ? player.currentKi : 100f,
            score = Score
        };
        SaveSystem.RecordRun(LevelIndex, true, Score, Kills, Elapsed, end, !tookDamage);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayVictory();

        if (HUDManager.Instance == null) return;

        string stats = "Score " + Score + "   Kills " + Kills + "   Time " + MenuScreen.FormatTime(Elapsed) + (tookDamage ? "" : "   FLAWLESS!");
        bool finalLevel = LevelIndex + 1 >= LevelConfig.Count;
        if (finalLevel)
        {
            HUDManager.Instance.ShowEndPanel(true, "VICTORY!", "You cleared all 3 worlds!\n" + stats,
                ("REPLAY", Replay),
                ("HOME", () => MenuScreen.Go(MenuScreen.HomeScene)),
                ("ACHIEVEMENTS", () => MenuScreen.Go(MenuScreen.AchievementsScene)),
                ("HISTORY", () => MenuScreen.Go(MenuScreen.HistoryScene)));
        }
        else
        {
            HUDManager.Instance.ShowEndPanel(true, "LEVEL COMPLETE!", stats + "\nHP " + Mathf.RoundToInt(end.hp) + " and score carried to " + LevelConfig.All[LevelIndex + 1].name,
                ("NEXT LEVEL", () => MenuScreen.Play(LevelIndex + 1)),
                ("REPLAY", Replay),
                ("HOME", () => MenuScreen.Go(MenuScreen.HomeScene)),
                ("ACHIEVEMENTS", () => MenuScreen.Go(MenuScreen.AchievementsScene)),
                ("HISTORY", () => MenuScreen.Go(MenuScreen.HistoryScene)));
        }
    }

    private void Replay()
    {
        MenuScreen.Play(LevelIndex);
    }
}
