using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SliderValue : MonoBehaviour
{
    [Header("绑定组件")]
    public Slider targetSlider;    // 拖入你的 Slider
    public TMP_Text valueText;     // 拖入你的 Value_Text

    [Header("显示格式")]
    [Tooltip("F0=整数, F1=一位小数, F2=两位小数")]
    public string numberFormat = "F0";
    [Tooltip("是否显示百分号? (例如音量需要)")]
    public bool showPercent = false;

    void Start()
    {
        // 自动查找组件 (如果没拖的话)
        if (targetSlider == null) targetSlider = GetComponentInParent<Slider>();
        if (valueText == null) valueText = GetComponent<TMP_Text>();

        if (targetSlider != null)
        {
            // 1. 初始化显示
            UpdateText(targetSlider.value);

            // 2. 监听数值变化 (当滑块动的时候，自动调用 UpdateText)
            targetSlider.onValueChanged.AddListener(UpdateText);
        }
    }

    // 更新文本的核心方法
    public void UpdateText(float val)
    {
        if (valueText != null)
        {
            if (showPercent)
            {
                // 如果是百分比模式 (0~1 显示为 0%~100%)
                valueText.text = Mathf.RoundToInt(val * 100) + "%";
            }
            else
            {
                // 普通数值模式
                valueText.text = val.ToString(numberFormat);
            }
        }
    }
}