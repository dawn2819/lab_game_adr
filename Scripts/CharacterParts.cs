using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý ghép nối 3 bộ phận: Đầu (Head), Thân (Body), Chân (Leg)
/// Tự động cập nhật sprite cho từng bộ phận và xử lý lật hướng (FlipX).
/// Tích hợp THANH TRƯỢT ĐỘ CAO ĐẦU (Head Height Slider) chỉnh trực tiếp mượt mà!
/// </summary>
[ExecuteAlways]
public class CharacterParts : MonoBehaviour
{
    [Header("Sprite Renderers của 3 bộ phận")]
    public SpriteRenderer headRenderer;
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer legRenderer;

    [Header("Kích Thước Toàn Bộ (Đã tăng x2 lên: 0.2)")]
    [Range(0.05f, 1.5f)]
    [Tooltip("Kéo thanh trượt để phóng to / thu nhỏ toàn bộ nhân vật (Mặc định 0.2)")]
    public float characterScale = 0.2f;

    [Header("Thanh Trượt Độ Cao Đầu (Kéo trực tiếp thấy ngay!)")]
    [Range(0.5f, 3.0f)]
    [Tooltip("Kéo thanh trượt này để đầu cao lên hoặc thấp xuống theo ý thích")]
    public float headHeight = 1.45f;

    [Header("Thanh Trượt Độ Cao Chân (Kéo trực tiếp thấy ngay!)")]
    [Range(-2.0f, 0.5f)]
    [Tooltip("Kéo thanh trượt này để chân cao lên hoặc thấp xuống theo ý thích (Mặc định: -0.6)")]
    public float legHeight = -0.6f;

    [Header("Căn chỉnh vị trí tương đối (Local Offsets)")]
    public Vector3 headOffset = new Vector3(0f, 1.45f, 0f);
    public Vector3 bodyOffset = new Vector3(0f, 0f, 0f);
    public Vector3 legOffset  = new Vector3(0f, -0.6f, 0f);

    [Header("Tỉ lệ to nhỏ của Đầu (Head Scale)")]
    [Range(0.4f, 1.5f)]
    public float headSize = 0.75f;
    public Vector3 headScale = new Vector3(0.75f, 0.75f, 1f);

    private bool facingRight = true;

    private void Awake()
    {
        FindRenderers();
        SyncSliders();
        ApplyOffsets();
    }

    private void OnValidate()
    {
        FindRenderers();
        SyncSliders();
        ApplyOffsets();
    }

    private void Update()
    {
        SyncSliders();
        ApplyOffsets();
    }

    private void LateUpdate()
    {
        SyncSliders();
        ApplyOffsets();
    }

    public void SyncAndApplyAll()
    {
        FindRenderers();
        SyncSliders();
        ApplyOffsets();
    }

    [HideInInspector]
    public Vector3 extraHeadOffset = Vector3.zero;
    [HideInInspector]
    public Vector3 extraBodyOffset = Vector3.zero;
    [HideInInspector]
    public Vector3 extraLegOffset = Vector3.zero;

    private void SyncSliders()
    {
        if (characterScale <= 0.01f || Mathf.Approximately(characterScale, 0.1f)) characterScale = 0.2f;
        if (headHeight <= 0.05f) headHeight = 1.45f;
        if (headSize <= 0.05f) headSize = 0.75f;
        if (legHeight >= 1f || legHeight <= -5f) legHeight = -0.6f;
        headOffset = new Vector3(0f, headHeight, 0f) + extraHeadOffset;
        bodyOffset = extraBodyOffset;
        legOffset = new Vector3(0f, legHeight, 0f) + extraLegOffset;
        headScale = new Vector3(headSize, headSize, 1f);
    }

    private void FindRenderers()
    {
        if (headRenderer == null)
        {
            Transform t = transform.Find("Head");
            if (t != null) headRenderer = t.GetComponent<SpriteRenderer>();
        }
        if (bodyRenderer == null)
        {
            Transform t = transform.Find("Body");
            if (t != null) bodyRenderer = t.GetComponent<SpriteRenderer>();
        }
        if (legRenderer == null)
        {
            Transform t = transform.Find("Leg");
            if (t != null) legRenderer = t.GetComponent<SpriteRenderer>();
        }
    }

    public void ApplyOffsets()
    {
        transform.localScale = new Vector3(characterScale * (facingRight ? 1f : -1f), characterScale, 1f);
        if (headRenderer != null)
        {
            headRenderer.transform.localPosition = headOffset;
            headRenderer.transform.localScale = headScale;
        }
        if (bodyRenderer != null)
        {
            bodyRenderer.transform.localPosition = bodyOffset;
            bodyRenderer.transform.localScale = Vector3.one;
        }
        if (legRenderer != null)
        {
            legRenderer.transform.localPosition = legOffset;
            legRenderer.transform.localScale = Vector3.one;
        }
    }

    public void SetPose(Sprite head, Sprite body, Sprite leg)
    {
        if (headRenderer != null && head != null) headRenderer.sprite = head;
        if (bodyRenderer != null && body != null) bodyRenderer.sprite = body;
        if (legRenderer != null && leg != null)   legRenderer.sprite = leg;
    }

    public void Flip(bool faceRight)
    {
        facingRight = faceRight;
        transform.localScale = new Vector3(characterScale * (faceRight ? 1f : -1f), characterScale, 1f);
    }

    public bool IsFacingRight()
    {
        return facingRight;
    }
}
