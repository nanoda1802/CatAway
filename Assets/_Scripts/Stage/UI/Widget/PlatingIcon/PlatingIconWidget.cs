using System;
using _Scripts.Stage.Item.Ingredient;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Widget.PlatingIcon
{
    public class PlatingIconWidget : WidgetBase
    {
        [SF] private Image[] icons;
        
        private PlatingIconData _data;

        private Transform _plateTr;

        [Inject]
        private void Construct(PlatingIconData data)
        {
            _data = data;
        }

        private void LateUpdate() // [임시]
        {
            if (_plateTr is null) return;
            UpdatePosition(_plateTr.position);
        }

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            Disconnect();
            
            foreach (var icon in icons)
            {
                icon.enabled = false;
            }

            base.Hide();
        }

        public void ConnectWith(Transform plateTr)
        {
            _plateTr = plateTr;
        }

        private void Disconnect()
        {
            _plateTr = null;
        }

        public void AddIcon(int idx, IngredientType type)
        {
            var icon = icons[idx];
            icon.enabled = true;
            icon.sprite = _data.GetSprite(type);
        }

        public override void UpdatePosition(Vector3 worldPos)
        {
            // [임시]
            // Dirty 체크 해서 불필요한 갱신 막기
            // prevWorldPos 캐싱해두고 새 worldPos와 sprMagnitude 비교, dirtyThreshold 보다 커야 갱신
            base.UpdatePosition(worldPos + _data.Offset);
        }
    }
}