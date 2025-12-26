using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class SceneLoding : MonoBehaviour
{
    [Header("UI组件")]
    public Slider progressBar;
    public TMP_Text progressText;

    [Header("加载音效")]
    [Tooltip("这里拖入那段激昂的 Loading 音效")]
    public AudioClip loadingClip;
    private AudioSource audioSource;

    public static string SceneToLoad;

    [Range(1, 10)]
    public float minLoadTime = 5.0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // 播放加载音效
        if (loadingClip != null && audioSource != null)
        {
            // 【核心修复】使用 bgmVolume
            float vol = 1.0f;
            if (SettingPanel.Instance != null)
                vol = SettingPanel.CurrentSettings.bgmVolume;

            audioSource.volume = vol;
            audioSource.clip = loadingClip;
            audioSource.Play();
        }

        if (!string.IsNullOrEmpty(SceneToLoad))
        {
            StartCoroutine(LoadAsync(SceneToLoad));
        }
    }

    IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (operation.progress < 0.9f || timer < minLoadTime)
        {
            timer += Time.deltaTime;
            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(timer / minLoadTime);
            float finalDisplayProgress = Mathf.Min(loadProgress, timeProgress);

            if (progressBar) progressBar.value = finalDisplayProgress;
            if (progressText) progressText.text = "稍等片刻，声声正在努力..." + (finalDisplayProgress * 100).ToString("F0") + "%";

            yield return null;
        }

        if (progressBar) progressBar.value = 1;
        if (progressText) progressText.text = "努力完毕!...100%";

        yield return new WaitForSeconds(0.2f);

        operation.allowSceneActivation = true;
    }

    public static void LoadLevel(string sceneName)
    {
        SceneToLoad = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}