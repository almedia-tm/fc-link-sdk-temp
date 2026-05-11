using AlmediaLink.Models;
using UnityEngine;
using UnityEngine.UI;

namespace AlmediaLink.UI
{
    [DisallowMultipleComponent]
    public class LinkButtonController : MonoBehaviour
    {
        [SerializeField] private Button _button;

        private PromoState? _lastPromoState;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
            AlmediaLinkSDK.OnInitialized += HandleInitialized;
            AlmediaLinkSDK.OnStatusChanged += HandleStatusChanged;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
            AlmediaLinkSDK.OnInitialized -= HandleInitialized;
            AlmediaLinkSDK.OnStatusChanged -= HandleStatusChanged;
        }

        private void HandleInitialized(AlmediaSDKState state)
        {
            bool show = state.IsAvailable && !state.IsLinked;
            SetVisible(show);
            FirePromoLoad(show, state.IsLinked);
        }

        private void HandleStatusChanged(AlmediaStatus status)
        {
            bool show = status == AlmediaStatus.Eligible;
            SetVisible(show);
            FirePromoLoad(show, status == AlmediaStatus.Linked);
        }

        private void FirePromoLoad(bool shown, bool isLinked)
        {
            PromoState state = shown ? PromoState.Eligible : (isLinked ? PromoState.Linked : PromoState.Hidden);
            if (state == _lastPromoState) return;
            _lastPromoState = state;
            AlmediaLinkSDK.TrackPromoLoad(state);
        }

        private void OnButtonClicked()
        {
            AlmediaLinkSDK.TrackPromoClick();
            AlmediaLinkUIManager.ShowLinkPopup();
        }

        private void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
