using System;
using _Scripts.Lobby.UI.Messages;
using AYellowpaper.SerializedCollections;
using MessagePipe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI.Room
{
    public enum MemberIconType
    {
        Host,
        Ready,
        NonReady
    }

    public class MemberCard : MonoBehaviour
    {
        [Header("[ Components ]")]
        [SF] private RectTransform cardRectTr;
        [SF] private Image iconImg;
        [SF] private TextMeshProUGUI nameTxt;

        private RoomViewUiData _data;
        private RectTransform _viewRectTr;
        private Camera _mainCam;
        
        [Inject]
        private void Construct(
            RoomViewUiData data,
            RectTransform canvasRectTr)
        {
            _data = data;
            _viewRectTr = canvasRectTr;
            
            _mainCam = Camera.main;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void UpdatePosition(Vector3 worldPos)
        {
            var screenPoint = _mainCam.WorldToScreenPoint(worldPos + _data.OffsetY * Vector3.up);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _viewRectTr, 
                screenPoint, 
                null,
                out Vector2 localPoint
            );
            
            cardRectTr.anchoredPosition = localPoint;
        }
        
        public MemberCard SetIcon(MemberIconType iconType = MemberIconType.NonReady)
        {
            iconImg.sprite = iconType switch
            {
                MemberIconType.Host => _data.HostIcon,
                MemberIconType.Ready => _data.CheckIcon,
                MemberIconType.NonReady => _data.CrossIcon,
                _ => throw new Exception("[MemberCard.UpdateIcon] 정의되지 않은 MemberIconType 입니다.")
            };
            
            iconImg.color = iconType switch
            {
                MemberIconType.Host => _data.HostColor,
                MemberIconType.Ready => _data.CheckColor,
                MemberIconType.NonReady => _data.CrossColor,
                _ => throw new Exception("[MemberCard.UpdateIcon] 정의되지 않은 MemberIconType 입니다.")
            };
            
            return this;
        }

        public MemberCard SetName(string memberName)
        {
            nameTxt.text = memberName;
            return this;
        }
        
        public void SwitchReadyIcon(bool isReady)
        {
            iconImg.sprite = isReady
                ? _data.CheckIcon 
                : _data.CrossIcon;
            
            iconImg.color = isReady
                ? _data.CheckColor 
                : _data.CrossColor;
        }
    }
}