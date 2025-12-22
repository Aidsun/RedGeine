using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SceneLoding : MonoBehaviour
{
    [Header("UI组件")]
    public Slider progressBar;
    public TMP_Text progressText;

    // 静态变量
    public static string SceneToLoad;

    // 【新增】设置最小加载时间（秒）
    [Range(1, 10)]
    public float minLoadTime = 5.0f;

    void Start()
    {
        if (!string.IsNullOrEmpty(SceneToLoad))
        {
            StartCoroutine(LoadAsync(SceneToLoad));
        }
    }

    IEnumerator LoadAsync(string sceneName)
    {
        // 1. 开始加载，但暂时"锁住"画面，不允许自动跳转
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;

        // 2. 循环等待
        // 只有当加载进度达到 0.9 (代表加载完毕) 并且 计时器超过 5秒，才退出循环
        while (operation.progress < 0.9f || timer < minLoadTime)
        {
            timer += Time.deltaTime;

            // 计算进度：前0.9是加载，后0.1是由于 allowSceneActivation=false 卡住的
            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // 计算时间进度：0~5秒 映射到 0~1
            float timeProgress = Mathf.Clamp01(timer / minLoadTime);

            // 【核心技巧】取两个进度中较小的那个，这样进度条就会慢慢走，不会一下满
            float finalDisplayProgress = Mathf.Min(loadProgress, timeProgress);

            // 更新UI
            if (progressBar) progressBar.value = finalDisplayProgress;
            if (progressText) progressText.text = "稍等片刻，声声正在努力..."+(finalDisplayProgress * 100).ToString("F0") + "%";

            yield return null;
        }

        // 3. 时间到了，进度也满了，放行！
        // 更新到100%
        if (progressBar) progressBar.value = 1;
        if (progressText) progressText.text = "努力完毕!...100%";

        // 等一小会儿让玩家看到100%
        yield return new WaitForSeconds(0.2f);

        operation.allowSceneActivation = true;
    }
    //加载场景供别人调用
    public static void LoadLevel(string sceneName)
    {
        SceneToLoad = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}