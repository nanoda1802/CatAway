using System.Threading;
using _Scripts.Result._Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.Result.UI
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
            if (isActiveAndEnabled) return; 
            
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