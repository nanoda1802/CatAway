using _Scripts.Result._Data;
using _Scripts.Stage._Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Result.UI
{
    public class ResultMemberCard : MonoBehaviour
    {
        [SF] private RectTransform cardRectTr;
        [SF] private Image namePlateImage;
        [SF] private TextMeshProUGUI nameText;
        [SF] private Image aceIconImage;

        private ResultViewData  _viewData;
        private RectTransform _viewRectTr;
        private Camera _mainCam;
        
        [Inject]
        private void Construct(
            ResultViewData viewData,
            RectTransform canvasRectTr)
        {
            _viewData = viewData;
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
            var screenPoint = _mainCam.WorldToScreenPoint(worldPos + _viewData.OffsetY * Vector3.up);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _viewRectTr, 
                screenPoint, 
                null,
                out Vector2 localPoint
            );
            
            cardRectTr.anchoredPosition = localPoint;
        }

        public ResultMemberCard SetTeamTheme(Team team)
        {
            namePlateImage.color = _viewData.GetNamePlateColor(team);
            return this;
        }

        public ResultMemberCard SetName(string memberName)
        {
            nameText.SetText(memberName);
            return this;
        }
        
        public ResultMemberCard SetAceIcon(bool isAce)
        {
            aceIconImage.enabled = isAce;
            return this;
        }
    }
}