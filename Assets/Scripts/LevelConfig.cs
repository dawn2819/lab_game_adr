using UnityEngine;

public enum EnemyAIType { Roamer, Hunter, Sniper, Flanker }

/// <summary>
/// Dữ liệu 3 cấp độ: thế giới (đồ họa), cơ chế vật lý, đội hình NPC và mục tiêu thắng.
/// Sprite nền nằm trong Resources/Levels (null = dùng nền mặc định của scene).
/// </summary>
public class LevelConfig
{
    public string name;
    public string description;
    public string topSprite;
    public string groundSprite;
    public Color skyColor;
    public float scrollSpeed;
    public float gravityScale;
    public float windX;              // Lực gió đẩy A theo trục X (đơn vị/giây)
    public float itemInterval;
    public float obstacleSpeedScale;
    public float enemySpeedScale;
    public int killsToWin;
    public float timeLimit;          // 0 = không giới hạn thời gian
    public EnemyAIType[] enemies;

    public static readonly LevelConfig[] All =
    {
        new LevelConfig
        {
            name = "West City",
            description = "Defeat 8 enemies. A Hunter will track you down.",
            skyColor = new Color(0.19f, 0.30f, 0.47f),
            scrollSpeed = 2f, gravityScale = 1f, windX = 0f,
            itemInterval = 5f, obstacleSpeedScale = 1f, enemySpeedScale = 1f,
            killsToWin = 8, timeLimit = 0f,
            enemies = new[] { EnemyAIType.Hunter, EnemyAIType.Roamer, EnemyAIType.Roamer }
        },
        new LevelConfig
        {
            name = "Desert Canyon",
            description = "Sandstorm pushes you left. Defeat 12 enemies within 120s.",
            topSprite = "Levels/bg_desert", groundSprite = "Levels/ground_desert",
            skyColor = new Color(0.75f, 0.32f, 0.12f),
            scrollSpeed = 3.2f, gravityScale = 1f, windX = -1.6f,
            itemInterval = 4f, obstacleSpeedScale = 1.4f, enemySpeedScale = 1.15f,
            killsToWin = 12, timeLimit = 120f,
            enemies = new[] { EnemyAIType.Hunter, EnemyAIType.Sniper, EnemyAIType.Roamer }
        },
        new LevelConfig
        {
            name = "Planet Namek",
            description = "Low gravity, items rain twice as fast. Defeat 15 enemies.",
            topSprite = "Levels/bg_namek", groundSprite = "Levels/ground_namek",
            skyColor = new Color(0.05f, 0.25f, 0.08f),
            scrollSpeed = 1.4f, gravityScale = 0.6f, windX = 0f,
            itemInterval = 2.5f, obstacleSpeedScale = 0.8f, enemySpeedScale = 1.3f,
            killsToWin = 15, timeLimit = 0f,
            enemies = new[] { EnemyAIType.Hunter, EnemyAIType.Sniper, EnemyAIType.Flanker, EnemyAIType.Roamer }
        }
    };

    public static int Count => All.Length;
}
