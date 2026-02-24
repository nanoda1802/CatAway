using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Widget.TableAlert
{
    public class TableAlertWidget : WidgetBase
    {
        [SF] private Image alertImg;
        [SF] private RectTransform rectTr; // [임시]
        
        private TableAlertData _data;
        
        private float _scaleModifier; // [임시]
        private float _speed = 0.8f; // [임시]
        
        [Inject]
        private void Construct(TableAlertData data)
        {
            _data = data;
        }

        private void Update()
        {
            float pingPong = Mathf.PingPong(Time.time * _speed, 0.4f);

            _scaleModifier = 0.8f + pingPong;

            rectTr.localScale = _scaleModifier * Vector3.one;
        }

        public override void Show()
        {
            base.Show();
            // 등장 Tween
            // 알파값이랑 스케일이랑 둠칫둠칫하는 Loop Tween 작동
        }

        public override void Hide()
        {
            // 연출 중이던 Loop Tween 중단
            // 퇴장 Tween
            rectTr.localScale = Vector3.one; // [임시]
            base.Hide();
        }

        public override void UpdatePosition(Vector3 worldPos)
        {
            base.UpdatePosition(worldPos + _data.Offset);
        }
    }
}