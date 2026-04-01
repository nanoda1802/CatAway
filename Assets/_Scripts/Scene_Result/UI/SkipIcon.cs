using System.Threading;
using _Scripts.Scene_Result.Data;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.Scene_Result.UI
{
    public class SkipIcon : MonoBehaviour
    {
        private ResultViewData _viewData;
        private Image _iconImg;

        [Inject]
        private void Construct(ResultViewData viewData)
        {
            _iconImg = GetComponent<Image>();
            
            _viewData = viewData;
            
            SetIcon(false).Hide();
        }

        public async UniTaskVoid Show(CancellationToken token)
        {
            if (isActiveAndEnabled) return; // 이미 활성화된 상태면 크기 커졌다 줄어드는 트윈만 하고 리턴
            
            // 비활성화된 상태면 활성화 후 크기 커졌다 줄어드는 트윈
            gameObject.SetActive(true);
            await UniTask.Yield(token);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public SkipIcon SetIcon(bool isAgree)
        {
            _iconImg.sprite = isAgree ? _viewData.CheckIcon : _viewData.CrossIcon;
            _iconImg.color = isAgree ? _viewData.CheckColor : _viewData.CrossColor;
            
            return this;
        }
    }
}