using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AlmediaLink.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ATTPrePromptController : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private PopupAnimator _animator;

        [Header("Buttons")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _closeButton;

        [Header("Text Fields")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _rewardAmountText;
        [SerializeField] private TMP_Text _whyTitleText;
        [SerializeField] private TMP_Text _whyBodyText;
        [SerializeField] private TMP_Text _controlTitleText;
        [SerializeField] private TMP_Text _controlBodyText;
        [SerializeField] private TMP_Text _continueButtonLabel;

        [Header("Colors")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _continueButtonImage;

        private void Awake()
        {
            if (!Application.isPlaying) return;

            if (_continueButton != null)
                _continueButton.onClick.AddListener(HandleContinue);
            if (_closeButton != null)
                _closeButton.onClick.AddListener(HandleClose);

            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            ApplySettings();
#if UNITY_EDITOR
            AlmediaLinkSettings.SettingsChanged += ApplySettings;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            AlmediaLinkSettings.SettingsChanged -= ApplySettings;
#endif
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying) return;

            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(HandleContinue);
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(HandleClose);
        }

        public void Show()
        {
            ApplySettings();
            AlmediaLinkSDK.TrackATTPreliminaryShow();
            _animator.Show();
        }

        private void HandleContinue()
        {
            AlmediaLog.Info("ATT pre-prompt: user tapped Continue.");
            _animator.Hide(() =>
            {
                AlmediaLinkSDK.ContinueWithATT();
                Destroy(gameObject);
            });
        }

        private void HandleClose()
        {
            AlmediaLog.Info("ATT pre-prompt: user tapped Close. Skipping ATT.");
            _animator.Hide(() =>
            {
                AlmediaLinkSDK.SkipATT();
                Destroy(gameObject);
            });
        }

        private void ApplySettings()
        {
            var settings = AlmediaLinkSettings.Load();
            if (settings == null) return;

            // Settings drive the prefab at editor authoring time only. At runtime the
            // prefab is canonical, so the host's translation system (e.g. Unity Localization
            // Package's LocalizeStringEvent) and any runtime theming components can drive
            // labels and visuals without being overwritten on each Show().
            if (Application.isPlaying) return;

            // Text
            if (_titleText != null)
                _titleText.text = settings.AttPromptTitle;
            if (_rewardAmountText != null)
                _rewardAmountText.text = settings.AttRewardAmount;
            if (_whyTitleText != null)
                _whyTitleText.text = settings.AttWhyTitle;
            if (_whyBodyText != null)
                _whyBodyText.text = settings.AttWhyBody;
            if (_controlTitleText != null)
                _controlTitleText.text = settings.AttControlTitle;
            if (_controlBodyText != null)
                _controlBodyText.text = settings.AttControlBody;
            if (_continueButtonLabel != null)
                _continueButtonLabel.text = settings.AttContinueButtonText;

            // Colors
            if (_continueButtonLabel != null)
                _continueButtonLabel.color = settings.AttButtonTextColor;
            if (_backgroundImage != null)
                _backgroundImage.color = settings.AttBackgroundColor;
            if (_continueButtonImage != null)
                _continueButtonImage.color = settings.AttPrimaryButtonColor;
        }
    }
}
