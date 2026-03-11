using _Scripts.Stage;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Lobby.UI.Room
{
    public class MemberCardProvider : IProvider
    {
        private IObjectResolver _resolver;
        private RoomViewUiData _data;
        private RectTransform _canvasTr;
        private MemberCard _cardPrefab;
        
        private IObjectPool<MemberCard> _pool;
        
        [Inject]
        private void Construct(
            IObjectResolver container,
            RoomViewUiData roomViewUiData,
            RectTransform canvasTr,
            MemberCard prefab)
        {
            _resolver = container;
            _data = roomViewUiData;
            _canvasTr = canvasTr;
            _cardPrefab = prefab;
            
            InitPool();
        }
       
        public void InitPool()
        {
            _pool = new ObjectPool<MemberCard>(
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
        
        private MemberCard CreateCard()
        {
            var card = _resolver.Instantiate(_cardPrefab, _canvasTr);
            card.name = $"MemberCard_{card.GetHashCode()}";
            return card;
        }

        private void OnGetCard(MemberCard card) { }

        private void OnReleaseCard(MemberCard card) { }

        private void OnDestroyCard(MemberCard card) { }

        public MemberCard GetCard(Vector3 worldPos)
        {
            var memberCard = _pool.Get();
            memberCard.UpdatePosition(worldPos);
            return memberCard;
        }

        public void ReleaseCard(MemberCard card)
        {
            _pool.Release(card);
        }
    }
}