using UnityEngine;

/// <summary>
/// Viên Ki của NPC Sniper: bay thẳng, trúng A thì trừ máu (khiên chặn được), chạm bệ vật cản hoặc ra khỏi màn hình thì biến mất.
/// </summary>
public class EnemyShot : MonoBehaviour
{
    public const float Speed = 9f;

    private Vector3 direction;
    private float damage;
    private float life = 3f;
    private PlayerA player;

    public static void Fire(Vector3 from, Vector3 target, float damage)
    {
        // Cùng kiểu dựng hình với Ki Blast của A, đổi màu tím để phân biệt đạn địch
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = "EnemyShot";
        obj.transform.position = from;
        obj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        Destroy(obj.GetComponent<Collider>());

        Renderer r = obj.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Sprites/Default"));
        r.material.color = new Color(0.8f, 0.2f, 1f, 1f);

        TrailRenderer tr = obj.AddComponent<TrailRenderer>();
        tr.time = 0.15f;
        tr.startWidth = 0.25f;
        tr.endWidth = 0f;
        tr.material = new Material(Shader.Find("Sprites/Default"));
        tr.startColor = new Color(0.8f, 0.2f, 1f, 1f);
        tr.endColor = new Color(0.3f, 0f, 0.6f, 0f);

        EnemyShot shot = obj.AddComponent<EnemyShot>();
        Vector3 dir = target - from;
        dir.z = 0f;
        shot.direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.left;
        shot.damage = damage;
    }

    private void Update()
    {
        if (!GameManager.IsPlaying) return;

        transform.position += direction * Speed * Time.deltaTime;
        life -= Time.deltaTime;

        if (player == null) player = FindFirstObjectByType<PlayerA>();
        if (player != null && Vector3.Distance(transform.position, player.transform.position + new Vector3(0f, 0.2f, 0f)) < 0.5f)
        {
            player.TakeDamage(damage);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosion();
            Destroy(gameObject);
            return;
        }

        Vector3 p = transform.position;
        foreach (FloatingObstacle obs in FloatingObstacle.ActiveObstacles)
        {
            if (obs != null && p.x > obs.LeftX && p.x < obs.RightX && p.y > obs.BottomSurfaceY && p.y < obs.TopSurfaceY)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (life <= 0f || p.x < BackgroundManager.LeftBorderX - 1f || p.x > BackgroundManager.RightBorderX + 1f || p.y < BackgroundManager.GroundSurfaceY)
        {
            Destroy(gameObject);
        }
    }
}
