using _Scripts.Scene_Stage.Data.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.UI.Widget.ProgressBar
{
    public class ProgressBarWidget : WidgetBase
    {
        [SF] private Image fillBarImg;
        
        private WidgetData _data;

        [Inject]
        private void Construct(WidgetData data)
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
            base.UpdatePosition(worldPos + _data.ProgressBarOffset);
        }

        public void UpdateProgress(float prevValue, float curValue)
        {
            if (prevValue >= curValue) return;
            fillBarImg.fillAmount = Mathf.Lerp(0,1,curValue);
        }

        private void ResetProgress()
        {
            fillBarImg.fillAmount = 0;
        }
    }
}