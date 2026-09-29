using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    private Text objectiveText;

    public Image hpBar;
    public Image kiBar;
    public Text hpText;
    public Text kiText;
    public Text scoreText;
    public Text highScoreText;

    public Button soundButton;
    public Button musicButton;

    private Color soundOnColor = Color.green;
    private Color soundOffColor = Color.red;
    private Color musicOnColor = Color.green;
    private Color musicOffColor = Color.red;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Ép HUDManager giãn toàn màn hình để các Neo (Anchors) của con hoạt động đúng
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
            }

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (soundButton != null)
        {
            soundButton.onClick.RemoveAllListeners();
            soundButton.onClick.AddListener(ToggleSound);
        }
        if (musicButton != null)
        {
            musicButton.onClick.RemoveAllListeners();
            musicButton.onClick.AddListener(ToggleMusic);
        }
        UpdateAudioButtons();
    }

    public void UpdateHP(float current, float max)
    {
        if (hpBar != null)
        {
            RectTransform rt = hpBar.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 maxAnchor = rt.anchorMax;
                maxAnchor.x = Mathf.Clamp01(current / max);
                rt.anchorMax = maxAnchor;
            }
        }
        if (hpText != null)
        {
            hpText.text = "HP: " + Mathf.RoundToInt(current) + "/" + Mathf.RoundToInt(max);
        }
    }

    public void UpdateKi(float current, float max)
    {
        if (kiBar != null)
        {
            RectTransform rt = kiBar.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 maxAnchor = rt.anchorMax;
                maxAnchor.x = Mathf.Clamp01(current / max);
                rt.anchorMax = maxAnchor;
            }
        }
        if (kiText != null)
        {
            kiText.text = "KI: " + Mathf.RoundToInt(current) + "/" + Mathf.RoundToInt(max);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score;
        }
    }

    public void UpdateHighScore(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = "HIGH SCORE: " + highScore;
        }
    }

    public void ToggleSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSound();
            UpdateAudioButtons();
        }
    }

    public void ToggleMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusic();
            UpdateAudioButtons();
        }
    }

    private void UpdateAudioButtons()
    {
        if (AudioManager.Instance == null) return;

        if (soundButton != null)
        {
            Text btnText = soundButton.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = AudioManager.Instance.IsSoundMuted ? "SOUND OFF" : "SOUND ON";
            
            ColorBlock cb = soundButton.colors;
            cb.normalColor = AudioManager.Instance.IsSoundMuted ? soundOffColor : soundOnColor;
            soundButton.colors = cb;
        }

        if (musicButton != null)
        {
            Text btnText = musicButton.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = AudioManager.Instance.IsMusicMuted ? "MUSIC OFF" : "MUSIC ON";

            ColorBlock cb = musicButton.colors;
            cb.normalColor = AudioManager.Instance.IsMusicMuted ? musicOffColor : musicOnColor;
            musicButton.colors = cb;
        }
    }

    /// <summary>Dòng mục tiêu cấp độ ở giữa phía trên màn hình (tạo khi cần lần đầu).</summary>
    public void SetObjective(string text)
    {
        if (objectiveText == null)
        {
            objectiveText = UIBuilder.Label(transform, "", 30, Color.white, new Vector2(0, -30), new Vector2(1000, 44));
            RectTransform rt = objectiveText.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
        }
        objectiveText.text = text;
    }

    /// <summary>Tiêu đề cấp độ hiện giữa màn hình rồi mờ dần khi vào màn.</summary>
    public void ShowLevelBanner(string title, string description)
    {
        Text t = UIBuilder.Label(transform, title + "\n<size=32>" + description + "</size>", 64, new Color(1f, 0.85f, 0.2f), new Vector2(0, 120), new Vector2(1600, 200));
        t.supportRichText = true;
        StartCoroutine(FadeOutAndDestroy(t, 2f, 1f));
    }

    private IEnumerator FadeOutAndDestroy(Text t, float hold, float fade)
    {
        yield return new WaitForSecondsRealtime(hold);
        Color c = t.color;
        for (float e = 0f; e < fade && t != null; e += Time.unscaledDeltaTime)
        {
            c.a = 1f - e / fade;
            t.color = c;
            yield return null;
        }
        if (t != null) Destroy(t.gameObject);
    }

    /// <summary>
    /// Bảng kết thúc trên màn hình chơi: chớp màn hình, chữ tiêu đề nảy (thua: đỏ + rung, thắng: vàng + pháo giấy)
    /// và hàng nút điều hướng sang các màn hình khác. Chạy bằng thời gian thực vì game đã dừng (timeScale = 0).
    /// </summary>
    public void ShowEndPanel(bool won, string title, string subtitle, params (string text, UnityAction action)[] buttons)
    {
        // Gắn thẳng vào Canvas và đặt cuối cùng để che (và chặn bấm) D-Pad, nút chiêu bên dưới
        Canvas canvas = GetComponentInParent<Canvas>();
        Image dim = UIBuilder.Panel(canvas != null ? canvas.transform : transform, won ? "WinPanel" : "GameOverPanel", new Color(0f, 0f, 0f, 0f));
        dim.transform.SetAsLastSibling();

        Image flash = UIBuilder.Panel(dim.transform, "Flash", won ? new Color(1f, 0.9f, 0.4f, 0.9f) : new Color(1f, 0f, 0f, 0.9f));
        flash.raycastTarget = false;

        Color titleColor = won ? new Color(1f, 0.85f, 0.1f) : new Color(1f, 0.2f, 0.2f);
        Text titleText = UIBuilder.Label(dim.transform, title, 130, titleColor, new Vector2(0, 170), new Vector2(1600, 180));
        Text subText = UIBuilder.Label(dim.transform, subtitle, 36, Color.white, new Vector2(0, 10), new Vector2(1600, 140));

        UIBuilder.ButtonRow(dim.transform, -180, new Vector2(300, 90), 30,
            won ? new Color(0.15f, 0.5f, 0.2f) : new Color(0.55f, 0.12f, 0.12f), buttons);

        if (won) StartCoroutine(ConfettiRoutine(dim.transform));
        StartCoroutine(EndPanelRoutine(dim, flash, titleText, subText, won));
    }

    private IEnumerator EndPanelRoutine(Image dim, Image flash, Text title, Text sub, bool won)
    {
        RectTransform titleRt = title.rectTransform;
        Vector2 basePos = titleRt.anchoredPosition;
        float e = 0f;
        while (dim != null)
        {
            e += Time.unscaledDeltaTime;

            // Chớp màn hình 0.4s đầu, nền tối dần tới 0.75
            flash.color = new Color(flash.color.r, flash.color.g, flash.color.b, Mathf.Lerp(0.9f, 0f, e / 0.4f));
            dim.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.75f, e / 0.6f));

            // Tiêu đề phóng từ 3x về 1x rồi nhịp đập
            float punch = Mathf.Lerp(3f, 1f, Mathf.Clamp01(e / 0.35f));
            float pulse = e > 0.35f ? 1f + Mathf.Sin(e * 6f) * 0.06f : 1f;
            titleRt.localScale = Vector3.one * punch * pulse;

            // Thua: tiêu đề rung trong 0.8s đầu
            titleRt.anchoredPosition = !won && e < 0.8f ? basePos + Random.insideUnitCircle * 14f * (1f - e / 0.8f) : basePos;

            sub.color = new Color(1f, 1f, 1f, Mathf.Clamp01((e - 0.3f) / 0.4f));
            yield return null;
        }
    }

    private IEnumerator ConfettiRoutine(Transform parent)
    {
        Color[] colors = { Color.yellow, Color.cyan, Color.magenta, Color.green, new Color(1f, 0.5f, 0f), Color.white };
        RectTransform[] pieces = new RectTransform[60];
        Vector2[] vel = new Vector2[pieces.Length];
        for (int i = 0; i < pieces.Length; i++)
        {
            Image img = UIBuilder.Box(parent, "Confetti", colors[i % colors.Length], new Vector2(Random.Range(-960f, 960f), Random.Range(560f, 1100f)), new Vector2(18, 30));
            img.raycastTarget = false;
            img.transform.SetSiblingIndex(1); // Nằm sau chữ và nút
            pieces[i] = img.rectTransform;
            vel[i] = new Vector2(Random.Range(-80f, 80f), Random.Range(-450f, -250f));
        }

        while (parent != null)
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < pieces.Length; i++)
            {
                RectTransform p = pieces[i];
                p.anchoredPosition += vel[i] * dt;
                p.Rotate(0f, 0f, 360f * dt * (i % 2 == 0 ? 1f : -1f));
                if (p.anchoredPosition.y < -600f) p.anchoredPosition = new Vector2(Random.Range(-960f, 960f), 600f);
            }
            yield return null;
        }
    }
}
