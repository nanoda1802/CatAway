using System;
using System.Collections.Generic;
using _Scripts.Stage.Item.Ingredient;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Widget.PlatingIcon
{
    public class PlatingIconWidget : WidgetBase
    {
        [SF] private Image[] icons;
        
        [SF] private Vector3 offset = new Vector3(0,0.5f,0);

        [SF] private SerializedDictionary<IngredientType, Sprite> iconSpriteDic;
        private Transform _plateTr;
        
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
            icon.sprite = this.GetSprite(type);
        }

        public override void UpdatePosition(Vector3 worldPos)
        {
            // [임시]
            // Dirty 체크 해서 불필요한 갱신 막기
            // prevWorldPos 캐싱해두고 새 worldPos와 sprMagnitude 비교, dirtyThreshold 보다 커야 갱신
            base.UpdatePosition(worldPos + offset);
        }
        
        public Sprite GetSprite(IngredientType key)
        {
            return iconSpriteDic.GetValueOrDefault(key, null); // [수정] default sprite 정해주기
        }

    }
}