using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource warningSource;

    private bool isMusicMuted = false;
    private bool isSoundMuted = false;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private AudioClip shootClip;
    private AudioClip warningClip;
    private AudioClip explosionClip;
    private AudioClip gameOverClip;
    private AudioClip victoryClip;
    private AudioClip clickClip;

    // Luôn có AudioManager kể cả khi game mở từ scene menu (bản trong SampleScene sẽ tự hủy vì trùng)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null) new GameObject("AudioManager").AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            InitializeAudioSources();
            GenerateSimulatedSounds();
            ApplySettings();
            // Nhạc nền bị dừng lúc thắng/thua sẽ phát lại khi sang scene mới
            SceneManager.sceneLoaded += (scene, mode) => { if (bgmSource != null && !bgmSource.isPlaying) bgmSource.UnPause(); };
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadSettings()
    {
        isMusicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        isSoundMuted = PlayerPrefs.GetInt("SoundMuted", 0) == 1;
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("MusicMuted", isMusicMuted ? 1 : 0);
        PlayerPrefs.SetInt("SoundMuted", isSoundMuted ? 1 : 0);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void ApplySettings()
    {
        bgmSource.mute = isMusicMuted;
        sfxSource.mute = isSoundMuted;
        warningSource.mute = isSoundMuted;
        bgmSource.volume = 0.5f * musicVolume;
        sfxSource.volume = 0.8f * sfxVolume;
        warningSource.volume = sfxVolume;
    }

    public float MusicVolume
    {
        get => musicVolume;
        set { musicVolume = Mathf.Clamp01(value); ApplySettings(); SaveSettings(); }
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set { sfxVolume = Mathf.Clamp01(value); ApplySettings(); SaveSettings(); }
    }

    private void InitializeAudioSources()
    {
        AudioSource[] existingSources = GetComponents<AudioSource>();
        if (existingSources != null && existingSources.Length >= 3)
        {
            bgmSource = existingSources[0];
            sfxSource = existingSources[1];
            warningSource = existingSources[2];
            return;
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = 0.5f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.volume = 0.8f;

        warningSource = gameObject.AddComponent<AudioSource>();
        warningSource.loop = false;
        warningSource.volume = 1f;
    }

    private void GenerateSimulatedSounds()
    {
        shootClip = AudioClip.Create("Shoot", 44100 / 4, 1, 44100, false);
        float[] shootData = new float[44100 / 4];
        for (int i = 0; i < shootData.Length; i++)
        {
            shootData[i] = Mathf.Sin(2 * Mathf.PI * 800 * i / 44100f) * Mathf.Exp(-i * 0.001f);
        }
        shootClip.SetData(shootData, 0);

        warningClip = AudioClip.Create("Warning", 44100 / 4, 1, 44100, false);
        float[] warnData = new float[44100 / 4];
        for (int i = 0; i < warnData.Length; i++)
        {
            warnData[i] = Mathf.Sin(2 * Mathf.PI * 1200 * i / 44100f) * Mathf.Exp(-i * 0.0005f);
        }
        warningClip.SetData(warnData, 0);

        explosionClip = AudioClip.Create("Explode", 44100 / 2, 1, 44100, false);
        float[] expData = new float[44100 / 2];
        for (int i = 0; i < expData.Length; i++)
        {
            expData[i] = (Random.value * 2f - 1f) * Mathf.Exp(-i * 0.0002f);
        }
        explosionClip.SetData(expData, 0);

        // Thua: 4 nốt đi xuống (G4 -> E4 -> C4 -> G3), nốt cuối kéo dài
        gameOverClip = CreateMelody("GameOver", new[] { 392f, 329.63f, 261.63f, 196f }, new[] { 0.25f, 0.25f, 0.25f, 0.9f });
        // Thắng: hợp âm rải đi lên (C5 -> E5 -> G5 -> C6) kiểu fanfare
        victoryClip = CreateMelody("Victory", new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, new[] { 0.12f, 0.12f, 0.12f, 0.7f });
        clickClip = CreateMelody("Click", new[] { 1400f }, new[] { 0.05f });

        // Tạo nhạc nền (BGM) điện tử 8-bit lặp lại (dài 4 giây)
        AudioClip bgmClip = AudioClip.Create("BGM", 44100 * 4, 1, 44100, false);
        float[] bgmData = new float[44100 * 4];
        float[] notes = { 220f, 261.63f, 329.63f, 392.00f }; // A3, C4, E4, G4
        for (int i = 0; i < bgmData.Length; i++)
        {
            float time = i / 44100f;
            // Thay đổi nốt nhạc mỗi 0.25 giây
            int noteIndex = Mathf.FloorToInt(time * 4) % notes.Length;
            float freq = notes[noteIndex];
            
            // Tạo sóng vuông (square wave) đặc trưng của retro 8-bit
            float wave = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * freq * time));
            
            // Thêm phách trống kick drum ảo mỗi 0.5 giây
            float kickEnv = Mathf.Exp(-(time % 0.5f) * 10f);
            float kick = Mathf.Sin(2 * Mathf.PI * 100f * (time % 0.5f)) * kickEnv;

            bgmData[i] = (wave * 0.15f + kick * 0.3f) * 0.5f; // Giảm âm lượng 50%
        }
        bgmClip.SetData(bgmData, 0);

        if (bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.Play(); // Tắt nhạc dùng mute để bật lại vẫn còn nhạc (trạng thái mute được lưu giữa các lần mở game)
        }
    }

    public void PlayBGM(AudioClip clip = null)
    {
        if (clip != null) bgmSource.clip = clip;
        if (!isMusicMuted && !bgmSource.isPlaying && bgmSource.clip != null)
        {
            bgmSource.Play();
        }
    }

    /// <summary>Chuỗi nốt sóng tam giác có envelope tắt dần, dùng cho hiệu ứng thắng/thua/bấm nút.</summary>
    private AudioClip CreateMelody(string clipName, float[] freqs, float[] durations)
    {
        const int rate = 44100;
        int total = 0;
        foreach (float d in durations) total += Mathf.CeilToInt(d * rate);

        float[] data = new float[total];
        int offset = 0;
        for (int n = 0; n < freqs.Length; n++)
        {
            int len = Mathf.CeilToInt(durations[n] * rate);
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)rate;
                float tri = Mathf.PingPong(t * freqs[n] * 4f, 2f) - 1f;
                float env = Mathf.Min(1f, i / 200f) * Mathf.Exp(-t * 3f / durations[n]);
                data[offset + i] = tri * env * 0.4f;
            }
            offset += len;
        }

        AudioClip clip = AudioClip.Create(clipName, total, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public void ToggleMusic()
    {
        SetMusicMute(!isMusicMuted);
    }

    public void SetMusicMute(bool mute)
    {
        isMusicMuted = mute;
        bgmSource.mute = mute;
        SaveSettings();
    }

    public void ToggleSound()
    {
        SetSoundMute(!isSoundMuted);
        Debug.Log("[AudioManager] ToggleSound. isSoundMuted = " + isSoundMuted);
    }

    public void SetSoundMute(bool mute)
    {
        isSoundMuted = mute;
        sfxSource.mute = mute;
        warningSource.mute = mute;
        SaveSettings();
    }

    public void PlayGameOver()
    {
        bgmSource.Pause();
        if (!isSoundMuted) sfxSource.PlayOneShot(gameOverClip);
    }

    public void PlayVictory()
    {
        bgmSource.Pause();
        if (!isSoundMuted) sfxSource.PlayOneShot(victoryClip);
    }

    public void PlayClick()
    {
        if (!isSoundMuted) sfxSource.PlayOneShot(clickClip);
    }

    public bool IsMusicMuted => isMusicMuted;
    public bool IsSoundMuted => isSoundMuted;

    public void PlayShoot()
    {
        if (isSoundMuted) return;
        if (shootClip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(shootClip);
        }
    }

    public void PlayExplosion()
    {
        if (!isSoundMuted && explosionClip != null)
        {
            sfxSource.PlayOneShot(explosionClip);
        }
    }

    private bool isWarningPlaying = false;
    public void PlayWarningBeeps()
    {
        if (!isWarningPlaying && !isSoundMuted)
        {
            StartCoroutine(WarningRoutine());
        }
    }

    private IEnumerator WarningRoutine()
    {
        isWarningPlaying = true;
        int beeps = Random.Range(3, 7); // 3 to 6
        for (int i = 0; i < beeps; i++)
        {
            if (!isSoundMuted && warningClip != null)
            {
                warningSource.PlayOneShot(warningClip);
            }
            yield return new WaitForSeconds(0.4f);
        }
        isWarningPlaying = false;
    }
}
