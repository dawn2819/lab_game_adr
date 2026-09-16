using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Điều khiển Đối tượng C (Chưởng lực Kamehameha):
/// - Tự động đuổi theo B (Homing), đổi hướng ngay cả khi B respawn.
/// - Tốc độ bay nhanh (speed = 16).
/// - Đổi sprite theo góc: Small59 (thẳng), Small60 (40 độ), Small61 (80 độ).
/// - Lật ảnh khi B ở dưới A.
/// - Khi gần tới B: đổi thành Small62.
/// - Khi chạm B: nổ xoay tròn cực nhanh (0.45s) theo chuỗi:
///   Small63 -> Small64 -> (Small65 + Small77 + Small78 cùng lúc) -> Small53 -> Small54 -> Small55 rồi kết thúc!
/// </summary>
public class KamehamehaC : MonoBehaviour
{
    [Header("Cấu hình bay (Bay nhanh & kết thúc nhanh)")]
    public float speed = 16f;
    [Range(0.05f, 1.5f)]
    [Tooltip("Kích thước đạn C (Mặc định 0.2 đã tăng x2)")]
    public float projectileScale = 0.2f;
    public float hitDistance = 0.3f;
    public float nearDistance = 1.0f;
    public float explosionDuration = 0.45f;

    [Header("Sprite Bay (Góc & Gần Đích)")]
    public Sprite spriteStraight;   // Small59.png (Thẳng ngang)
    public Sprite spriteAngle40;    // Small60.png (Góc 40 độ)
    public Sprite spriteAngle80;    // Small61.png (Góc 80 độ)
    public Sprite spriteNearTarget; // Small62.png (Gần mục tiêu)

    [Header("Sprite Phát Nổ (Small63 -> Small55)")]
    public Sprite exp63; // Small63.png
    public Sprite exp64; // Small64.png
    public Sprite exp65; // Small65.png
    public Sprite exp77; // Small77.png (cùng lúc với 65)
    public Sprite exp78; // Small78.png (cùng lúc với 65)
    public Sprite exp53; // Small53.png
    public Sprite exp54; // Small54.png
    public Sprite exp55; // Small55.png

    private Transform targetEnemyB;
    private SpriteRenderer mainRenderer;
    private SpriteRenderer extraRenderer1;
    private SpriteRenderer extraRenderer2;
    private bool isExploding = false;
    private float bounceVelocityY = 0f;

    private void Awake()
    {
        if (projectileScale <= 0.01f || Mathf.Approximately(projectileScale, 0.1f)) projectileScale = 0.2f;
        transform.localScale = new Vector3(projectileScale, projectileScale, 1f);
        mainRenderer = GetComponent<SpriteRenderer>();
        if (mainRenderer == null) mainRenderer = gameObject.AddComponent<SpriteRenderer>();

        extraRenderer1 = CreateChildRenderer("ExtraExp1");
        extraRenderer2 = CreateChildRenderer("ExtraExp2");
    }

    private SpriteRenderer CreateChildRenderer(string childName)
    {
        GameObject child = new GameObject(childName);
        child.transform.SetParent(transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localScale = Vector3.one;
        SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
        sr.sortingOrder = mainRenderer.sortingOrder + 1;
        child.SetActive(false);
        return sr;
    }

    public void Init(Transform target)
    {
        targetEnemyB = target;
    }

    private void Update()
    {
        if (isExploding) return;

        if (targetEnemyB == null)
        {
            EnemyB[] enemies = FindObjectsByType<EnemyB>(FindObjectsSortMode.None);
            float closest = float.MaxValue;
            foreach (var e in enemies)
            {
                if (e == null) continue;
                float d = Vector3.Distance(transform.position, e.transform.position);
                if (d < closest)
                {
                    closest = d;
                    targetEnemyB = e.transform;
                }
            }
            if (targetEnemyB == null)
            {
                transform.position += Vector3.right * speed * Time.deltaTime;
                return;
            }
        }

        Vector3 diff = targetEnemyB.position - transform.position;
        float dist = diff.magnitude;

        if (dist <= hitDistance)
        {
            if (targetEnemyB != null)
            {
                EnemyB enemyB = targetEnemyB.GetComponent<EnemyB>();
                if (enemyB != null)
                {
                    enemyB.OnHitBySkill(explosionDuration);
                }
            }
            StartCoroutine(ExplosionRoutine());
            return;
        }

        UpdateFlightVisuals(diff, dist);

        // Quy tắc: "sẽ bị va đánh bật trở lại áp dụng với B và C"
        Vector3 curPos = transform.position;
        float groundLimit = BackgroundManager.GroundSurfaceY + 0.15f;
        float ceilingLimit = BackgroundManager.CeilingY;

        if (curPos.y <= groundLimit)
        {
            curPos.y = groundLimit;
            bounceVelocityY = Mathf.Max(bounceVelocityY, speed * 0.85f);
        }
        else if (curPos.y >= ceilingLimit)
        {
            curPos.y = ceilingLimit;
            bounceVelocityY = Mathf.Min(bounceVelocityY, -speed * 0.85f);
        }

        if (Mathf.Abs(bounceVelocityY) > 0.1f)
        {
            curPos.y += bounceVelocityY * Time.deltaTime;
            bounceVelocityY = Mathf.MoveTowards(bounceVelocityY, 0f, speed * 2.5f * Time.deltaTime);
            float stepX = Mathf.Sign(diff.x) * speed * Time.deltaTime;
            curPos.x += stepX;
            transform.position = curPos;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetEnemyB.position, speed * Time.deltaTime);
        }
    }

    private void UpdateFlightVisuals(Vector3 diff, float dist)
    {
        if (dist <= nearDistance && spriteNearTarget != null)
        {
            mainRenderer.sprite = spriteNearTarget;
            return;
        }

        float angleDeg = Mathf.Abs(Mathf.Atan2(diff.y, Mathf.Abs(diff.x)) * Mathf.Rad2Deg);

        if (angleDeg < 25f && spriteStraight != null)
        {
            mainRenderer.sprite = spriteStraight;
        }
        else if (angleDeg < 65f && spriteAngle40 != null)
        {
            mainRenderer.sprite = spriteAngle40;
        }
        else if (spriteAngle80 != null)
        {
            mainRenderer.sprite = spriteAngle80;
        }

        mainRenderer.flipY = (diff.y < -0.1f);
        mainRenderer.flipX = (diff.x < 0f);
    }

    /// <summary>
    /// Chuỗi phát nổ nhanh gọn dứt khoát (chỉ 0.45s) xoay tròn quanh B
    /// </summary>
    private IEnumerator ExplosionRoutine()
    {
        isExploding = true;
        mainRenderer.flipX = false;
        mainRenderer.flipY = false;

        if (targetEnemyB != null)
        {
            transform.position = targetEnemyB.position;
        }

        float frameTime = explosionDuration / 6.0f; // ~0.075s mỗi frame

        StartCoroutine(Rotate360Routine(explosionDuration));

        mainRenderer.sprite = exp63;
        yield return new WaitForSeconds(frameTime);

        mainRenderer.sprite = exp64;
        yield return new WaitForSeconds(frameTime);

        mainRenderer.sprite = exp65;
        if (extraRenderer1 != null && exp77 != null)
        {
            extraRenderer1.gameObject.SetActive(true);
            extraRenderer1.sprite = exp77;
        }
        if (extraRenderer2 != null && exp78 != null)
        {
            extraRenderer2.gameObject.SetActive(true);
            extraRenderer2.sprite = exp78;
        }
        yield return new WaitForSeconds(frameTime);

        if (extraRenderer1 != null) extraRenderer1.gameObject.SetActive(false);
        if (extraRenderer2 != null) extraRenderer2.gameObject.SetActive(false);

        mainRenderer.sprite = exp53;
        yield return new WaitForSeconds(frameTime);

        mainRenderer.sprite = exp54;
        yield return new WaitForSeconds(frameTime);

        mainRenderer.sprite = exp55;
        yield return new WaitForSeconds(frameTime);

        Destroy(gameObject);
    }

    private IEnumerator Rotate360Routine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.Rotate(0f, 0f, (720f / duration) * Time.deltaTime);
            yield return null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Tự Động Gán Toàn Bộ Sprite (1-Click)")]
    public void AutoAssignSprites()
    {
        string p = "Assets/Sprites/";
        spriteStraight   = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small59.png");
        spriteAngle40    = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small60.png");
        spriteAngle80    = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small61.png");
        spriteNearTarget = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small62.png");

        exp63 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small63.png");
        exp64 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small64.png");
        exp65 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small65.png");
        exp77 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small77.png");
        exp78 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small78.png");
        exp53 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small53.png");
        exp54 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small54.png");
        exp55 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p + "Small55.png");

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
