using _Scripts.Result._Data;
using _Scripts.Stage;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Result.UI
{
    public class ResultMemberCardProvider : IProvider
    {
        private IObjectResolver _resolver;
        private ResultViewData _data;
        private RectTransform _canvasTr;
        private ResultMemberCard _cardPrefab;
        
        private IObjectPool<ResultMemberCard> _pool;
        
        [Inject]
        private void Construct(
            IObjectResolver container,
            ResultViewData roomViewData,
            RectTransform canvasTr,
            ResultMemberCard prefab)
        {
            _resolver = container;
            _data = roomViewData;
            _canvasTr = canvasTr;
            _cardPrefab = prefab;
            
            InitPool();
        }
       
        public void InitPool()
        {
            _pool = new ObjectPool<ResultMemberCard>(
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
        
        private ResultMemberCard CreateCard()
        {
            var card = _resolver.Instantiate(_cardPrefab, _canvasTr);
            card.name = $"MemberCard_{card.GetHashCode()}";
            return card;
        }

        private void OnGetCard(ResultMemberCard card) { }

        private void OnReleaseCard(ResultMemberCard card) { }

        private void OnDestroyCard(ResultMemberCard card) { }

        public ResultMemberCard GetCard(Vector3 worldPos)
        {
            var memberCard = _pool.Get();
            memberCard.UpdatePosition(worldPos);
            return memberCard;
        }

        public void ReleaseCard(ResultMemberCard card)
        {
            _pool.Release(card);
        }
    }
}