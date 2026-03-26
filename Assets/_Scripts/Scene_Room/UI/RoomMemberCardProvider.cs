using _Scripts.Scene_Room.Data;
using _Scripts.Scene_Stage;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Scene_Room.UI
{
    public class RoomMemberCardProvider : IProvider
    {
        private IObjectResolver _resolver;
        private RoomViewData _data;
        private RectTransform _canvasTr;
        private RoomMemberCard _cardPrefab;
        
        private IObjectPool<RoomMemberCard> _pool;
        
        [Inject]
        private void Construct(
            IObjectResolver container,
            RoomViewData roomViewData,
            RectTransform canvasTr,
            RoomMemberCard prefab)
        {
            _resolver = container;
            _data = roomViewData;
            _canvasTr = canvasTr;
            _cardPrefab = prefab;
            
            InitPool();
        }
       
        public void InitPool()
        {
            _pool = new ObjectPool<RoomMemberCard>(
                CreateCard
                , OnGetCard
                , OnReleaseCard
                , OnDestroyCard,
                true,
                _data.DefaultCount,
                _data.MaxCount);

            for (int i = 0; i < _data.DefaultCount; i++)
            {
                var memberCard = CreateCard();
                memberCard.gameObject.SetActive(false);
                ReleaseCard(memberCard);
            }
        }
        
        private RoomMemberCard CreateCard()
        {
            var card = _resolver.Instantiate(_cardPrefab, _canvasTr);
            card.name = $"MemberCard_{card.GetHashCode()}";
            return card;
        }

        private void OnGetCard(RoomMemberCard card) { }

        private void OnReleaseCard(RoomMemberCard card) { }

        private void OnDestroyCard(RoomMemberCard card) { }

        public RoomMemberCard GetCard(Vector3 worldPos)
        {
            var memberCard = _pool.Get();
            memberCard.UpdatePosition(worldPos);
            return memberCard;
        }

        public void ReleaseCard(RoomMemberCard card)
        {
            _pool.Release(card);
        }
    }
}