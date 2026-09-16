using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Quản lý sinh các vật cản lơ lửng trên không trung (vatcan.png)
/// - Bay từ 2 biên trái và phải ra với tốc độ chậm
/// - Chiều cao mặc định, chiều dài biến thiên ngẫu nhiên (2*A <= L <= 1/3 L_goc)
/// </summary>
public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager Instance { get; private set; }

    [Header("Sprite Vật Cản (Tự động nạp nếu trống)")]
    public Sprite obstacleSprite; // Assets/Sprites/vatcan.png

    [Header("Cấu hình chu kỳ sinh vật cản")]
    public float spawnIntervalMin = 3.5f;
    public float spawnIntervalMax = 6.0f;
    public int maxActiveObstacles = 4;

    [Header("Tốc độ bay chậm")]
    public float minFlySpeed = 1.2f;
    public float maxFlySpeed = 2.0f;

    [Header("Kích thước vật cản (theo quy định đề bài)")]
    [Tooltip("Chiều dài tối thiểu (không ngắn quá 2 lần A: ~1.5)")]
    public float minPlatformWidth = 1.5f;
    [Tooltip("Chiều dài tối đa (không vượt quá 1/3 chiều dài gốc: ~5.5)")]
    public float maxPlatformWidth = 5.2f;

    private float spawnTimer = 0f;
    private float nextSpawnInterval = 2.0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (FindFirstObjectByType<ObstacleManager>() == null)
        {
            GameObject obsGo = new GameObject("ObstacleManager");
            obsGo.AddComponent<ObstacleManager>();
        }
    }

    private void Awake()
    {
        Instance = this;
        AutoLoadSprite();
        nextSpawnInterval = 1.5f; // Sinh ngay bệ đầu tiên sau 1.5s
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= nextSpawnInterval)
        {
            spawnTimer = 0f;
            nextSpawnInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);

            if (FloatingObstacle.ActiveObstacles.Count < maxActiveObstacles)
            {
                SpawnObstacle();
            }
        }
    }

    public void SpawnObstacle()
    {
        if (obstacleSprite == null)
        {
            AutoLoadSprite();
            if (obstacleSprite == null) return;
        }

        // 1. Kích thước
        float width = Random.Range(minPlatformWidth, maxPlatformWidth);
        Camera cam = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
        float camH = cam != null ? (cam.orthographicSize * 2f) : 10f;
        float defaultHeight = camH * (157f / 1376f); // Chiều cao mặc định ~1.14f

        // 2. Hướng và vị trí xuất phát từ 2 biên
        int direction = (Random.value < 0.5f) ? 1 : -1; // 1: Trái -> Phải, -1: Phải -> Trái
        float startX;
        if (direction > 0)
        {
            // Xuất phát từ mép trái màn hình
            startX = BackgroundManager.LeftBorderX - (width * 0.5f) - 0.5f;
        }
        else
        {
            // Xuất phát từ mép phải màn hình
            startX = BackgroundManager.RightBorderX + (width * 0.5f) + 0.5f;
        }

        // 3. Phân làn cao độ (3 làn: Thấp, Trung, Cao) để bệ phân bố đều và tránh đè nhau
        float lane0 = BackgroundManager.GroundSurfaceY + 1.8f;
        float lane1 = (BackgroundManager.GroundSurfaceY + BackgroundManager.CeilingY) * 0.5f;
        float lane2 = BackgroundManager.CeilingY - 1.8f;
        float[] lanes = new float[] { lane0, lane1, lane2 };

        // Tìm làn ít vật cản nhất
        float chosenLaneY = lanes[Random.Range(0, lanes.Length)];
        for (int l = 0; l < lanes.Length; l++)
        {
            float candidateY = lanes[l];
            bool laneOccupied = false;
            for (int i = 0; i < FloatingObstacle.ActiveObstacles.Count; i++)
            {
                var o = FloatingObstacle.ActiveObstacles[i];
                if (o != null && Mathf.Abs(o.transform.position.y - candidateY) < 0.8f)
                {
                    laneOccupied = true;
                    break;
                }
            }
            if (!laneOccupied)
            {
                chosenLaneY = candidateY;
                break;
            }
        }
        float startY = chosenLaneY + Random.Range(-0.25f, 0.25f);

        // 4. Tốc độ bay chậm
        float speed = Random.Range(minFlySpeed, maxFlySpeed);

        // 5. Tạo GameObject bệ vật cản
        GameObject obsGo = new GameObject("FloatingObstacle_" + direction);
        obsGo.transform.SetParent(transform);
        FloatingObstacle obs = obsGo.AddComponent<FloatingObstacle>();
        obs.Init(obstacleSprite, width, defaultHeight, speed, direction, startY, startX);
    }

    public void AutoLoadSprite()
    {
#if UNITY_EDITOR
        if (obstacleSprite == null)
        {
            obstacleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/vatcan.png");
        }
#endif
        if (obstacleSprite == null)
        {
            foreach (var s in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (s != null && s.name == "vatcan")
                {
                    obstacleSprite = s;
                    break;
                }
            }
        }
    }
}
