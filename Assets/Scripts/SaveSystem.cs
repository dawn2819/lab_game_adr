using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lưu tiến trình người chơi (JSON trong PlayerPrefs): cấp độ đã mở, trạng thái A mang sang cấp sau,
/// kỷ lục từng cấp và lịch sử các lượt chơi.
/// </summary>
public static class SaveSystem
{
    private const string Key = "SaveData_v1";
    private const int MaxHistory = 30;

    [Serializable]
    public class PlayerState
    {
        public float hp = 100f;
        public float ki = 100f;
        public int score = 0;
    }

    [Serializable]
    public class RunRecord
    {
        public string date;
        public int level;
        public bool won;
        public int score;
        public int kills;
        public float seconds;
    }

    [Serializable]
    public class Data
    {
        public int unlockedLevel = 0;
        public PlayerState[] levelEntry = new PlayerState[0]; // Trạng thái A khi bắt đầu mỗi cấp
        public int[] bestScore = new int[0];
        public float[] bestTime = new float[0];               // 0 = chưa hoàn thành
        public int totalKills = 0;
        public int wins = 0;
        public int losses = 0;
        public bool flawlessClear = false;
        public List<RunRecord> history = new List<RunRecord>();
    }

    private static Data cache;

    public static Data Load()
    {
        if (cache != null) return cache;

        string json = PlayerPrefs.GetString(Key, "");
        try
        {
            cache = string.IsNullOrEmpty(json) ? new Data() : JsonUtility.FromJson<Data>(json);
        }
        catch (ArgumentException e)
        {
            Debug.LogWarning("[SaveSystem] Save data corrupted, starting fresh: " + e.Message);
            cache = null;
        }
        if (cache == null) cache = new Data();

        int n = LevelConfig.Count;
        if (cache.levelEntry == null || cache.levelEntry.Length < n) Array.Resize(ref cache.levelEntry, n);
        if (cache.bestScore == null || cache.bestScore.Length < n) Array.Resize(ref cache.bestScore, n);
        if (cache.bestTime == null || cache.bestTime.Length < n) Array.Resize(ref cache.bestTime, n);
        if (cache.history == null) cache.history = new List<RunRecord>();
        cache.unlockedLevel = Mathf.Clamp(cache.unlockedLevel, 0, n - 1);
        return cache;
    }

    public static void Save()
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(Load()));
        PlayerPrefs.Save();
    }

    /// <summary>Trạng thái A khi vào cấp độ: cấp 1 luôn mới, các cấp sau lấy từ lần hoàn thành cấp trước.</summary>
    public static PlayerState EntryState(int level)
    {
        Data d = Load();
        PlayerState s = level > 0 ? d.levelEntry[level] : null;
        return s != null && s.hp > 0f ? s : new PlayerState();
    }

    public static bool IsCleared(int level)
    {
        return Load().bestTime[level] > 0f;
    }

    /// <summary>Ghi nhận 1 lượt chơi. Thắng: mở khóa cấp sau và lưu trạng thái A để mang sang.</summary>
    public static void RecordRun(int level, bool won, int score, int kills, float seconds, PlayerState endState, bool flawless)
    {
        Data d = Load();
        d.history.Insert(0, new RunRecord
        {
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            level = level,
            won = won,
            score = score,
            kills = kills,
            seconds = seconds
        });
        if (d.history.Count > MaxHistory) d.history.RemoveRange(MaxHistory, d.history.Count - MaxHistory);

        d.totalKills += kills;
        d.bestScore[level] = Mathf.Max(d.bestScore[level], score);

        if (won)
        {
            d.wins++;
            if (d.bestTime[level] <= 0f || seconds < d.bestTime[level]) d.bestTime[level] = seconds;
            if (flawless) d.flawlessClear = true;
            if (level + 1 < LevelConfig.Count)
            {
                d.unlockedLevel = Mathf.Max(d.unlockedLevel, level + 1);
                d.levelEntry[level + 1] = endState;
            }
        }
        else
        {
            d.losses++;
        }
        Save();
    }

    public static void ResetProgress()
    {
        cache = new Data();
        Load();
        Save();
        PlayerPrefs.DeleteKey("HighScore");
    }
}
