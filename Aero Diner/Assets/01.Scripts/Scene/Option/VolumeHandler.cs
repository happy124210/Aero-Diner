using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeHandler : MonoBehaviour
{
    [Header("BGM 설정")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TMP_Text bgmPercentageText;

    [Header("SFX 설정")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxPercentageText;

    private void Start()
    {
        // 초기 슬라이더 값 설정
        float bgmVolume = BGMManager.Instance.GetVolume();
        float sfxVolume = SFXManager.Instance.GetVolume();

        bgmSlider.SetValueWithoutNotify(bgmVolume);
        sfxSlider.SetValueWithoutNotify(sfxVolume);

        UpdateBGMVolumeUI(bgmVolume);
        UpdateSFXVolumeUI(sfxVolume);

        // 이벤트 등록
        bgmSlider.onValueChanged.AddListener(UpdateBGMVolumeUI);
        sfxSlider.onValueChanged.AddListener(UpdateSFXVolumeUI);
    }

    private void UpdateBGMVolumeUI(float value)
    {
        BGMManager.Instance.SetVolume(value);
        bgmPercentageText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }

    private void UpdateSFXVolumeUI(float value)
    {
        SFXManager.Instance.SetVolume(value);
        sfxPercentageText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }
}
