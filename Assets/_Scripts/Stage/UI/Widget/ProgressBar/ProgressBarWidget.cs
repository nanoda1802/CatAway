using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Widget.ProgressBar
{
    public class ProgressBarWidget : WidgetBase
    {
        [SF] private Image fillBarImg;
        
        private ProgressBarData _data;

        [Inject]
        private void Construct(ProgressBarData data)
        {
            _data = data;
        }

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
            ResetProgress();
        }

        public override void UpdatePosition(Vector3 worldPos)
        {
            base.UpdatePosition(worldPos + _data.Offset);
        }

        public void UpdateProgress(float prevValue, float newValue)
        {
            if (prevValue >= newValue) return;
            fillBarImg.fillAmount = Mathf.Lerp(0,1,newValue);
        }

        private void ResetProgress()
        {
            fillBarImg.fillAmount = 0;
        }
    }
}