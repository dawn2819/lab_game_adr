using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Màn hình riêng biệt ngoài gameplay (mỗi loại là 1 scene): Home (chọn cấp), Settings, History, Achievements.
/// Toàn bộ UI dựng bằng code khi scene mở.
/// </summary>
public class MenuScreen : MonoBehaviour
{
    public enum Kind { Home, Settings, History, Achievements }

    public const string HomeScene = "MainMenu";
    public const string SettingsScene = "Settings";
    public const string HistoryScene = "History";
    public const string AchievementsScene = "Achievements";
    public const string GameScene = "SampleScene";

    public Kind kind = Kind.Home;

    private static readonly Color BgColor = new Color(0.06f, 0.07f, 0.14f, 1f);
    private static readonly Color CardColor = new Color(1f, 1f, 1f, 0.08f);
    private static readonly Color ButtonColor = new Color(0.16f, 0.36f, 0.75f, 1f);
    private static readonly Color Gold = new Color(1f, 0.8f, 0.2f);

    private Transform root;

    public static void Go(string scene)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    public static void Play(int level)
    {
        GameManager.SelectedLevel = level;
        Go(GameScene);
    }

    public static void Continue()
    {
        Play(SaveSystem.Load().unlockedLevel);
    }

    public static string FormatTime(float seconds)
    {
        int s = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
    }

    private void Start()
    {
        Time.timeScale = 1f;
        root = UIBuilder.CreateCanvas("MenuCanvas").transform;
        UIBuilder.Panel(root, "Background", BgColor);

        switch (kind)
        {
            case Kind.Home: BuildHome(); break;
            case Kind.Settings: BuildSettings(); break;
            case Kind.History: BuildHistory(); break;
            case Kind.Achievements: BuildAchievements(); break;
        }
    }

    private void Title(string text, string subtitle)
    {
        UIBuilder.Label(root, text, 96, Gold, new Vector2(0, 420), new Vector2(1600, 130));
        if (!string.IsNullOrEmpty(subtitle))
            UIBuilder.Label(root, subtitle, 34, Color.white, new Vector2(0, 330), new Vector2(1600, 60));
    }

    // ---------------- HOME ----------------
    private void BuildHome()
    {
        SaveSystem.Data d = SaveSystem.Load();
        Title("LAB NRO 2D", "Clear all 3 worlds to win the game");

        for (int i = 0; i < LevelConfig.Count; i++)
        {
            int level = i;
            LevelConfig cfg = LevelConfig.All[i];
            bool locked = i > d.unlockedLevel;
            bool cleared = SaveSystem.IsCleared(i);

            Vector2 center = new Vector2(-560 + i * 560, 40);
            Transform card = UIBuilder.Box(root, "Card_" + i, locked ? new Color(0f, 0f, 0f, 0.4f) : CardColor, center, new Vector2(520, 360)).transform;

            UIBuilder.Label(card, "LEVEL " + (i + 1), 28, Gold, new Vector2(0, 145), new Vector2(480, 40));
            UIBuilder.Label(card, cfg.name, 46, Color.white, new Vector2(0, 100), new Vector2(480, 60));
            UIBuilder.Label(card, cfg.description, 24, new Color(0.85f, 0.85f, 0.85f), new Vector2(0, 30), new Vector2(460, 80));

            string status = locked ? "LOCKED" : cleared ? "CLEARED  -  best " + d.bestScore[i] + " pts / " + FormatTime(d.bestTime[i]) : "NOT CLEARED YET";
            UIBuilder.Label(card, status, 24, locked ? Color.gray : cleared ? Color.green : Color.yellow, new Vector2(0, -45), new Vector2(480, 40));

            Button play = UIBuilder.Button(card, locked ? "LOCKED" : "PLAY", new Vector2(0, -125), new Vector2(260, 70), ButtonColor, () => Play(level));
            play.interactable = !locked;
        }

        UIBuilder.ButtonRow(root, -330, new Vector2(300, 80), 30, ButtonColor,
            ("CONTINUE", Continue),
            ("SETTINGS", () => Go(SettingsScene)),
            ("HISTORY", () => Go(HistoryScene)),
            ("ACHIEVEMENTS", () => Go(AchievementsScene)),
            ("QUIT", Application.Quit));
    }

    // ---------------- SETTINGS ----------------
    private void BuildSettings()
    {
        Title("SETTINGS", "Audio and save data");
        AudioManager am = AudioManager.Instance;

        Button musicBtn = null;
        musicBtn = UIBuilder.Button(root, "", new Vector2(-300, 170), new Vector2(360, 80), ButtonColor, () =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.ToggleMusic();
            RefreshToggle(musicBtn, "MUSIC", AudioManager.Instance == null || !AudioManager.Instance.IsMusicMuted);
        });
        RefreshToggle(musicBtn, "MUSIC", am == null || !am.IsMusicMuted);
        CreateSlider(new Vector2(200, 170), am != null ? am.MusicVolume : 1f, v => { if (AudioManager.Instance != null) AudioManager.Instance.MusicVolume = v; });

        Button soundBtn = null;
        soundBtn = UIBuilder.Button(root, "", new Vector2(-300, 50), new Vector2(360, 80), ButtonColor, () =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.ToggleSound();
            RefreshToggle(soundBtn, "SOUND", AudioManager.Instance == null || !AudioManager.Instance.IsSoundMuted);
        });
        RefreshToggle(soundBtn, "SOUND", am == null || !am.IsSoundMuted);
        CreateSlider(new Vector2(200, 50), am != null ? am.SfxVolume : 1f, v => { if (AudioManager.Instance != null) AudioManager.Instance.SfxVolume = v; });

        // Xóa dữ liệu cần bấm 2 lần để tránh bấm nhầm
        bool armed = false;
        Button resetBtn = null;
        resetBtn = UIBuilder.Button(root, "RESET PROGRESS", new Vector2(0, -110), new Vector2(560, 80), new Color(0.6f, 0.15f, 0.15f), () =>
        {
            Text t = resetBtn.GetComponentInChildren<Text>();
            if (!armed)
            {
                armed = true;
                t.text = "TAP AGAIN TO CONFIRM";
                return;
            }
            SaveSystem.ResetProgress();
            armed = false;
            t.text = "PROGRESS CLEARED";
        });

        UIBuilder.ButtonRow(root, -330, new Vector2(320, 80), 30, ButtonColor,
            ("HOME", () => Go(HomeScene)),
            ("PLAY", Continue),
            ("HISTORY", () => Go(HistoryScene)),
            ("ACHIEVEMENTS", () => Go(AchievementsScene)));
    }

    private static void RefreshToggle(Button btn, string what, bool on)
    {
        btn.GetComponentInChildren<Text>().text = what + ": " + (on ? "ON" : "OFF");
        btn.GetComponent<Image>().color = on ? new Color(0.15f, 0.55f, 0.25f) : new Color(0.55f, 0.15f, 0.15f);
    }

    private void CreateSlider(Vector2 pos, float value, UnityEngine.Events.UnityAction<float> onChange)
    {
        GameObject obj = DefaultControls.CreateSlider(new DefaultControls.Resources());
        obj.transform.SetParent(root, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(520, 36);

        Slider slider = obj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;
        slider.fillRect.GetComponent<Image>().color = Gold;
        slider.handleRect.sizeDelta = new Vector2(36, 0);
        slider.onValueChanged.AddListener(onChange);
    }

    // ---------------- HISTORY ----------------
    private void BuildHistory()
    {
        SaveSystem.Data d = SaveSystem.Load();
        Title("HISTORICAL PROGRESS",
            "Runs: " + d.history.Count + "    Wins: " + d.wins + "    Losses: " + d.losses +
            "    Total kills: " + d.totalKills + "    Unlocked: Level " + (d.unlockedLevel + 1));

        float[] colX = { -620, -330, -60, 180, 380, 560 };
        string[] header = { "DATE", "LEVEL", "RESULT", "SCORE", "KILLS", "TIME" };
        float y = 240;
        for (int c = 0; c < header.Length; c++)
            UIBuilder.Label(root, header[c], 30, Gold, new Vector2(colX[c], y), new Vector2(300, 44));

        if (d.history.Count == 0)
        {
            UIBuilder.Label(root, "No games played yet. Press PLAY to start!", 34, Color.white, new Vector2(0, 100), new Vector2(1200, 60));
        }

        int rows = Mathf.Min(d.history.Count, 11);
        for (int i = 0; i < rows; i++)
        {
            SaveSystem.RunRecord r = d.history[i];
            y -= 44;
            string levelName = r.level >= 0 && r.level < LevelConfig.Count ? LevelConfig.All[r.level].name : "?";
            string[] cells = { r.date, levelName, r.won ? "WIN" : "LOSE", r.score.ToString(), r.kills.ToString(), FormatTime(r.seconds) };
            for (int c = 0; c < cells.Length; c++)
            {
                Color col = c == 2 ? (r.won ? Color.green : new Color(1f, 0.35f, 0.35f)) : Color.white;
                UIBuilder.Label(root, cells[c], 26, col, new Vector2(colX[c], y), new Vector2(300, 40));
            }
        }

        UIBuilder.ButtonRow(root, -400, new Vector2(320, 80), 30, ButtonColor,
            ("HOME", () => Go(HomeScene)),
            ("PLAY", Continue),
            ("ACHIEVEMENTS", () => Go(AchievementsScene)),
            ("SETTINGS", () => Go(SettingsScene)));
    }

    // ---------------- ACHIEVEMENTS ----------------
    private void BuildAchievements()
    {
        SaveSystem.Data d = SaveSystem.Load();
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        Title("HIGH ACHIEVEMENTS", "HIGH SCORE: " + highScore);

        for (int i = 0; i < LevelConfig.Count; i++)
        {
            string line = "Level " + (i + 1) + " - " + LevelConfig.All[i].name + ":   best score " + d.bestScore[i] +
                          "   best time " + (SaveSystem.IsCleared(i) ? FormatTime(d.bestTime[i]) : "--:--");
            UIBuilder.Label(root, line, 32, SaveSystem.IsCleared(i) ? Color.green : Color.white, new Vector2(0, 230 - i * 50), new Vector2(1600, 46));
        }

        (string title, string desc, bool unlocked)[] badges =
        {
            ("First Blood", "Defeat your first enemy", d.totalKills >= 1),
            ("Monster Hunter", "Defeat 50 enemies in total", d.totalKills >= 50),
            ("City Guardian", "Clear West City", SaveSystem.IsCleared(0)),
            ("Desert Survivor", "Clear Desert Canyon", SaveSystem.IsCleared(1)),
            ("Namek Hero", "Clear Planet Namek - win the game", SaveSystem.IsCleared(2)),
            ("Untouchable", "Clear a level without taking damage", d.flawlessClear),
            ("Score Master", "Reach a high score of 2000", highScore >= 2000),
            ("Veteran", "Play 10 games", d.history.Count >= 10)
        };

        for (int i = 0; i < badges.Length; i++)
        {
            Vector2 pos = new Vector2(-630 + (i % 4) * 420, -10 - (i / 4) * 160);
            var b = badges[i];
            Transform box = UIBuilder.Box(root, "Badge_" + i, b.unlocked ? new Color(0.85f, 0.65f, 0.1f, 0.9f) : new Color(1f, 1f, 1f, 0.08f), pos, new Vector2(390, 140)).transform;
            UIBuilder.Label(box, b.title, 32, b.unlocked ? Color.white : Color.gray, new Vector2(0, 30), new Vector2(370, 44));
            UIBuilder.Label(box, b.desc, 22, b.unlocked ? Color.white : Color.gray, new Vector2(0, -12), new Vector2(370, 40));
            UIBuilder.Label(box, b.unlocked ? "UNLOCKED" : "LOCKED", 22, b.unlocked ? Color.black : Color.gray, new Vector2(0, -48), new Vector2(370, 30));
        }

        UIBuilder.ButtonRow(root, -400, new Vector2(320, 80), 30, ButtonColor,
            ("HOME", () => Go(HomeScene)),
            ("PLAY", Continue),
            ("HISTORY", () => Go(HistoryScene)),
            ("SETTINGS", () => Go(SettingsScene)));
    }
}
