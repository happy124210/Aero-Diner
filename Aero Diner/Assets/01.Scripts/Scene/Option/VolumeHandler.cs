using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeHandler : MonoBehaviour
{
    public static VolumeHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    [Header("BGM 설정")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TMP_Text bgmPercentageText;

    [Header("SFX 설정")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxPercentageText;

    private float originalBGMVolume;
    private float originalSFXVolume;
    private float pendingBGMVolume;
    private float pendingSFXVolume;

    private void Start()
    {
        // PlayerPrefs에서 로드
        originalBGMVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        originalSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        pendingBGMVolume = originalBGMVolume;
        pendingSFXVolume = originalSFXVolume;

        // 슬라이더 초기화
        bgmSlider.SetValueWithoutNotify(originalBGMVolume);
        sfxSlider.SetValueWithoutNotify(originalSFXVolume);

        UpdateBGMVolumeUI(originalBGMVolume);
        UpdateSFXVolumeUI(originalSFXVolume);

        // 변경 감지
        bgmSlider.onValueChanged.AddListener((v) =>
        {
            pendingBGMVolume = v;
            UpdateBGMVolumeUI(v);
        });

        sfxSlider.onValueChanged.AddListener((v) =>
        {
            pendingSFXVolume = v;
            UpdateSFXVolumeUI(v);
        });
    }

    private void UpdateBGMVolumeUI(float value)
    {
        bgmPercentageText.text = $"{Mathf.RoundToInt(value * 100)}%";
        BGMManager.Instance.SetVolume(value);
    }

    private void UpdateSFXVolumeUI(float value)
    {
        sfxPercentageText.text = $"{Mathf.RoundToInt(value * 100)}%";
        SFXManager.Instance.SetVolume(value);
    }
    public bool HasUnsavedChanges()
    {
        return !Mathf.Approximately(pendingBGMVolume, originalBGMVolume)
            || !Mathf.Approximately(pendingSFXVolume, originalSFXVolume);
    }
    /// <summary> 저장 버튼 클릭 시 호출 </summary>
    public void SaveVolumes()
    {
        originalBGMVolume = pendingBGMVolume;
        originalSFXVolume = pendingSFXVolume;

        PlayerPrefs.SetFloat("BGMVolume", originalBGMVolume);
        PlayerPrefs.SetFloat("SFXVolume", originalSFXVolume);
        PlayerPrefs.Save();

        Debug.Log($"[VolumeHandler] 볼륨 저장됨: BGM {originalBGMVolume}, SFX {originalSFXVolume}");
    }

    /// <summary> 리셋 버튼 클릭 시 호출 </summary>
    public void ResetVolumes()
    {
        pendingBGMVolume = 0.5f;
        pendingSFXVolume = 0.5f;

        bgmSlider.SetValueWithoutNotify(0.5f);
        sfxSlider.SetValueWithoutNotify(0.5f);

        UpdateBGMVolumeUI(0.5f);
        UpdateSFXVolumeUI(0.5f);

        SaveVolumes();
    }

    /// <summary> ESC로 옵션 창 닫을 때 호출 — 원래 값으로 롤백 </summary>
    public void RollbackVolumes()
    {
        pendingBGMVolume = originalBGMVolume;
        pendingSFXVolume = originalSFXVolume;

        bgmSlider.SetValueWithoutNotify(originalBGMVolume);
        sfxSlider.SetValueWithoutNotify(originalSFXVolume);

        UpdateBGMVolumeUI(originalBGMVolume);
        UpdateSFXVolumeUI(originalSFXVolume);

        Debug.Log("[VolumeHandler] 볼륨 롤백 완료");
    }
}