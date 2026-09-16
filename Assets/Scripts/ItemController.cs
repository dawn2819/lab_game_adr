using UnityEngine;
using System.Collections;

public enum ItemType
{
    X_SenzuBean,
    Y_SpikeTrap,
    Z_MysteryBox
}

public class ItemController : MonoBehaviour
{
    public ItemType itemType;
    private float fallSpeed = 3f;
    private float timeAlive = 0f;

    private void Update()
    {
        // Rơi tự do
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        timeAlive += Time.deltaTime;

        // Animation tùy loại
        if (itemType == ItemType.X_SenzuBean)
        {
            // Nhấp nhô bồng bềnh
            transform.position += new Vector3(0, Mathf.Sin(timeAlive * 5f) * 0.01f, 0);
        }
        else if (itemType == ItemType.Y_SpikeTrap)
        {
            // Xoay liên tục
            transform.Rotate(0, 0, 180f * Time.deltaTime);
        }
        else if (itemType == ItemType.Z_MysteryBox)
        {
            // Phóng to thu nhỏ (Thở)
            float scale = 0.5f + Mathf.Sin(timeAlive * 8f) * 0.1f;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        // Tự hủy nếu rớt quá mặt đất
        if (transform.position.y < BackgroundManager.GroundSurfaceY - 1f)
        {
            Destroy(gameObject);
            return;
        }

        CheckCollisionWithPlayer();
    }

    private void CheckCollisionWithPlayer()
    {
        PlayerA pA = FindFirstObjectByType<PlayerA>();
        if (pA != null && pA.gameObject.activeInHierarchy)
        {
            if (Vector3.Distance(transform.position, pA.transform.position) < 1.0f)
            {
                OnCollected(pA);
            }
        }
    }

    private void OnCollected(PlayerA player)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayShoot();

        switch (itemType)
        {
            case ItemType.X_SenzuBean:
                player.RestoreHp(player.maxHp * 0.5f);
                player.RestoreKi(player.maxKi);
                break;

            case ItemType.Y_SpikeTrap:
                player.TakeDamage(15f);
                player.StartCoroutine(SlowDownRoutine(player));
                break;

            case ItemType.Z_MysteryBox:
                // Hiệu ứng 5: Tăng nhiều tiền (Score) thay vì sinh bệ lỗi
                if (GameManager.Instance != null) GameManager.Instance.AddScore(500);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosion();
                break;
        }

        Destroy(gameObject);
    }

    private IEnumerator SlowDownRoutine(PlayerA player)
    {
        float originalSpeed = player.moveSpeed;
        player.moveSpeed = originalSpeed * 0.5f; // Giảm 50%
        yield return new WaitForSeconds(2f);
        player.moveSpeed = originalSpeed;
    }
}
