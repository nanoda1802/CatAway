using System.Collections.Generic;
using _Scripts.Scene_Stage.Data.UI;
using _Scripts.Scene_Stage.Enums;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.UI.Widget.PlatingIcon
{
    public class PlatingIconWidget : WidgetBase
    {
        [SF] private Image[] icons;
        
        // [SF] private Vector3 offset = new Vector3(0,0.5f,0);

        // [SF] private SerializedDictionary<IngredientType, Sprite> iconSpriteDic;
        private Transform _plateTr;
        
        private WidgetData _data;

        [Inject]
        private void Construct(WidgetData data)
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
            Unassign();
            
            foreach (var icon in icons)
            {
                icon.enabled = false;
            }

            base.Hide();
        }

        public void AssignTo(Transform plateTr)
        {
            _plateTr = plateTr;
        }

        private void Unassign()
        {
            _plateTr = null;
        }

        public void AddIcon(int idx, IngredientType type)
        {
            var icon = icons[idx];
            icon.enabled = true;
            // icon.sprite = this.GetSprite(type);
            icon.sprite = _data.GetIconSprite(type);
        }

        public override void UpdatePosition(Vector3 worldPos)
        {
            // [임시]
            // Dirty 체크 해서 불필요한 갱신 막기
            // prevWorldPos 캐싱해두고 새 worldPos와 sprMagnitude 비교, dirtyThreshold 보다 커야 갱신
            base.UpdatePosition(worldPos + _data.PlatingIconOffset);
        }
        
        // public Sprite GetSprite(IngredientType key)
        // {
        //     // return iconSpriteDic.GetValueOrDefault(key, null); // [수정] default sprite 정해주기
        // }

    }
}