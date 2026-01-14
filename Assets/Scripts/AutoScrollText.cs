using UnityEngine;
using TMPro;

public class AutoScrollText : MonoBehaviour
{
    private RectTransform textRect;
    private TMP_Text tmpText;
    private float textWidth;
    private float parentWidth;

    private float calculatedSpeed = 0f;
    private bool isScrolling = false;

    void Awake()
    {
        textRect = GetComponent<RectTransform>();
        tmpText = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// 根据给定的时长，自动计算速度并开始滚动
    /// </summary>
    /// <param name="duration">解说音频的时长(秒)</param>
    public void StartScrollingByDuration(float duration)
    {
        if (textRect == null || tmpText == null) return;

        // 1. 强制刷新网格，获取精准的文字宽度
        tmpText.ForceMeshUpdate();
        textWidth = tmpText.preferredWidth;

        // 2. 获取父物体（遮罩条）的宽度
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
        parentWidth = parentRect.rect.width;

        // 3. 设置初始位置：
        // 你的要求：开始时，文本的第一个字出现在右边 -> 即文本整体在遮罩右侧外
        // (假设 Text 的 Pivot X 是 0，Left 对齐)
        textRect.anchoredPosition = new Vector2(parentWidth, 0);

        // 4. 计算速度：
        // 你的要求：结束时，最后一个字刚好出现在屏幕右边。
        // 起点：文本左边对齐屏幕右边 (X = parentWidth)
        // 终点：文本右边对齐屏幕右边 (X = parentWidth - textWidth)
        // 也就是说，文本向左移动的距离 = 文本自身的长度 (textWidth)

        float distance = textWidth;

        if (duration > 0)
        {
            calculatedSpeed = distance / duration;
        }
        else
        {
            calculatedSpeed = 100f; // 如果没有音频时长，给个默认速度
        }

        isScrolling = true;
        Debug.Log($"[跑马灯] 文本长度:{textWidth}, 目标时长:{duration}s, 计算速度:{calculatedSpeed}");
    }

    void Update()
    {
        if (!isScrolling) return;

        // 向左移动
        textRect.anchoredPosition += Vector2.left * calculatedSpeed * Time.deltaTime;

        // 循环逻辑保护：如果为了防止文字跑太远找不到了，可以加个重置
        // 当文字完全跑出左边屏幕时 (Pos X < -textWidth)
        if (textRect.anchoredPosition.x < -textWidth)
        {
            // 停止或者重置? 这里让它循环，或者你可以选择直接 isScrolling = false;
            // textRect.anchoredPosition = new Vector2(parentWidth, 0);
        }
    }
}