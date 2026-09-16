using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Đại diện cho 1 KHỐI VẬT CẢN ĐẶC (Solid Block) lơ lửng trên không trung (vatcan.png)
/// - Chiều cao mặc định, chiều dài biến thiên ngẫu nhiên (2*A <= L <= 1/3 L_goc)
/// - Bay chậm từ biên này sang biên kia (Left-to-Right hoặc Right-to-Left)
/// - Biến mất ngay khi vừa trôi hết qua biên đối diện
/// - Hai vật cản va chạm nhau sẽ bị dội ngược lại (Bounce), không đi xuyên nhau
/// - Là một KHỐI ĐẶC hoàn chỉnh 4 mặt:
///   + Mặt trên: A có thể nhảy lên đứng, di chuyển và trôi cùng bệ
///   + Mặt dưới: A nhảy từ dưới lên sẽ cộp đầu vào đáy bệ và rơi xuống
///   + 4 mặt ngoài: Bị chặn đứng tuyệt đối, đẩy lùi Kẻ địch B, không cho B đi xuyên
///   + C đi xuyên qua khối bệ không bị cản trở
/// </summary>
public class FloatingObstacle : MonoBehaviour
{
    public static List<FloatingObstacle> ActiveObstacles = new List<FloatingObstacle>();

    [Header("Cấu hình di chuyển")]
    public float flySpeed = 1.5f;
    public int flyDirection = 1; // +1: sang phải, -1: sang trái

    [Header("Kích thước bệ")]
    public float platformWidth = 3.5f;
    public float platformHeight = 1.14f;

    public SpriteRenderer spriteRenderer;

    private bool hasEnteredScreen = false;
    private float lifeTime = 0f;

    public float TopSurfaceY
    {
        get { return transform.position.y + (platformHeight * 0.5f); }
    }

    public float BottomSurfaceY
    {
        get { return transform.position.y - (platformHeight * 0.5f); }
    }

    public float LeftX
    {
        get { return transform.position.x - (platformWidth * 0.5f); }
    }

    public float RightX
    {
        get { return transform.position.x + (platformWidth * 0.5f); }
    }

    public Vector3 Velocity
    {
        get { return Vector3.right * (flyDirection * flySpeed); }
    }

    private void OnEnable()
    {
        if (!ActiveObstacles.Contains(this))
        {
            ActiveObstacles.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveObstacles.Remove(this);
    }

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = -4; // Nằm trước diahinh (-5), nằm sau nhân vật (>= 0)
    }

    public void Init(Sprite sprite, float width, float height, float speed, int direction, float startY, float startX)
    {
        platformWidth = width;
        platformHeight = height;
        flySpeed = speed;
        flyDirection = direction;

        transform.position = new Vector3(startX, startY, 0f);

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = -4;

        // Tính tỉ lệ co giãn
        float nativeSpriteH = sprite.rect.height / sprite.pixelsPerUnit;
        float scaleY = height / nativeSpriteH;

        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        spriteRenderer.tileMode = SpriteTileMode.Continuous;
        spriteRenderer.size = new Vector2(width / scaleY, nativeSpriteH);
        transform.localScale = new Vector3(scaleY, scaleY, 1f);
        hasEnteredScreen = false;
        lifeTime = 0f;
    }

    private void Update()
    {
        lifeTime += Time.deltaTime;

        // 1. Di chuyển bệ sang trái hoặc phải với tốc độ chậm
        transform.position += Vector3.right * (flyDirection * flySpeed * Time.deltaTime);

        // 2. Kiểm tra vào màn hình và TỰ HỦY NGAY KHI VỪA TRÔI QUA BIÊN ("các vật khi đi qua 2 biên sẽ biến mất")
        if (!hasEnteredScreen)
        {
            if (RightX > BackgroundManager.LeftBorderX && LeftX < BackgroundManager.RightBorderX)
            {
                hasEnteredScreen = true;
            }
        }
        else
        {
            if (flyDirection > 0 && LeftX >= BackgroundManager.RightBorderX)
            {
                Destroy(gameObject);
                return;
            }
            else if (flyDirection < 0 && RightX <= BackgroundManager.LeftBorderX)
            {
                Destroy(gameObject);
                return;
            }
            else if (transform.position.x > BackgroundManager.RightBorderX + (platformWidth * 0.5f) + 0.5f ||
                     transform.position.x < BackgroundManager.LeftBorderX - (platformWidth * 0.5f) - 0.5f)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Tự hủy nếu kẹt quá lâu ngoài màn hình
        if (lifeTime > 45f)
        {
            Destroy(gameObject);
            return;
        }

        // 3. Xử lý va chạm giữa 2 vật cản (Dội ngược lại dứt khoát, không thể đi xuyên nhau)
        CheckObstacleCollisions();

        // 4. Khối đặc cản và đẩy Kẻ địch B (B không thể đi xuyên qua)
        ResolveEnemyCollision();
    }

    private void LateUpdate()
    {
        // Đảm bảo ở bước render cuối cùng, B không bao giờ nằm trong lòng khối vật cản
        ResolveEnemyCollision();
    }

    /// <summary>
    /// Kiểm tra va chạm giữa các khối vật cản với nhau:
    /// Nếu 2 khối chạm nhau thì dội ngược lại (Bounce) dứt khoát, tách nhau ra ngay lập tức
    /// </summary>
    private void CheckObstacleCollisions()
    {
        for (int i = 0; i < ActiveObstacles.Count; i++)
        {
            FloatingObstacle other = ActiveObstacles[i];
            if (other == null || other == this) continue;

            // Xử lý mỗi cặp 1 lần duy nhất bằng GetInstanceID() để tránh bị đảo chiều 2 lần trong 1 frame
            if (this.GetInstanceID() < other.GetInstanceID())
            {
                bool overlapX = (LeftX < other.RightX) && (RightX > other.LeftX);
                bool overlapY = (BottomSurfaceY < other.TopSurfaceY) && (TopSurfaceY > other.BottomSurfaceY);

                if (overlapX && overlapY)
                {
                    FloatingObstacle leftObs = (transform.position.x < other.transform.position.x) ? this : other;
                    FloatingObstacle rightObs = (transform.position.x < other.transform.position.x) ? other : this;

                    // Tính độ xuyên thấu trên trục X và tách rời ngay lập tức với đệm an toàn
                    float penetrationX = leftObs.RightX - rightObs.LeftX;
                    if (penetrationX > 0f)
                    {
                        float push = (penetrationX * 0.5f) + 0.08f;
                        leftObs.transform.position += Vector3.left * push;
                        rightObs.transform.position += Vector3.right * push;
                    }

                    // Ép buộc dội ngược lại theo 2 hướng đối diện: vật bên trái bay sang trái, vật bên phải bay sang phải
                    leftObs.flyDirection = -1;
                    rightObs.flyDirection = 1;
                }
            }
        }
    }

    /// <summary>
    /// Xử lý va chạm khối đặc 4 mặt với Kẻ địch B:
    /// B bị chặn hoàn toàn ở 4 mặt ngoài và bị bệ đẩy dồn khi bệ di chuyển
    /// </summary>
    private void ResolveEnemyCollision()
    {
        EnemyB[] enemies = FindObjectsByType<EnemyB>(FindObjectsSortMode.None);
        float halfW = platformWidth * 0.5f;
        float halfH = platformHeight * 0.5f;

        float bHalfW = 0.30f;
        float bHalfH = 0.40f;

        float boxLeft = transform.position.x - halfW;
        float boxRight = transform.position.x + halfW;
        float boxBottom = transform.position.y - halfH;
        float boxTop = transform.position.y + halfH;

        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyB b = enemies[i];
            if (b == null || !b.gameObject.activeInHierarchy) continue;

            Vector3 bPos = b.transform.position;

            bool overlapX = (bPos.x + bHalfW > boxLeft) && (bPos.x - bHalfW < boxRight);
            bool overlapY = (bPos.y + bHalfH > boxBottom) && (bPos.y - bHalfH < boxTop);

            if (overlapX && overlapY)
            {
                // Khi bệ đang bay và ép vào B theo phương ngang:
                if (flyDirection > 0)
                {
                    // Đẩy B về phía trước bệ bên phải
                    bPos.x = boxRight + bHalfW + 0.05f;
                    b.transform.position = bPos;
                    b.OnBlockedByObstacle(this, EnemyB.BlockedFace.Right);
                }
                else if (flyDirection < 0)
                {
                    // Đẩy B về phía trước bệ bên trái
                    bPos.x = boxLeft - bHalfW - 0.05f;
                    b.transform.position = bPos;
                    b.OnBlockedByObstacle(this, EnemyB.BlockedFace.Left);
                }
                else
                {
                    // Đẩy B ra khỏi mặt gần nhất
                    float penLeft   = (bPos.x + bHalfW) - boxLeft;
                    float penRight  = boxRight - (bPos.x - bHalfW);
                    float penBottom = (bPos.y + bHalfH) - boxBottom;
                    float penTop    = boxTop - (bPos.y - bHalfH);

                    float minPen = Mathf.Min(Mathf.Min(penLeft, penRight), Mathf.Min(penBottom, penTop));
                    if (minPen == penTop)
                    {
                        bPos.y = boxTop + bHalfH + 0.05f;
                        b.OnBlockedByObstacle(this, EnemyB.BlockedFace.Top);
                    }
                    else if (minPen == penBottom)
                    {
                        bPos.y = boxBottom - bHalfH - 0.05f;
                        b.OnBlockedByObstacle(this, EnemyB.BlockedFace.Bottom);
                    }
                    else if (minPen == penLeft)
                    {
                        bPos.x = boxLeft - bHalfW - 0.05f;
                        b.OnBlockedByObstacle(this, EnemyB.BlockedFace.Left);
                    }
                    else
                    {
                        bPos.x = boxRight + bHalfW + 0.05f;
                        b.OnBlockedByObstacle(this, EnemyB.BlockedFace.Right);
                    }
                    b.transform.position = bPos;
                }
            }
        }
    }
}
