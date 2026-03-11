using System;
using System.Collections.Generic;
using System.Threading;
using _Scripts.Stage.Item.Ingredient;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Board.Order
{
    public class OrderBoard : MonoBehaviour, IBoard<AddOrderMessage>, IBoard<RemoveOrderMessage>
    {
        // Data
        private StageData _stageData;
        private OrderCardData _cardData;
        private OrderCard _prefab;
        // Component
        [SF] private Team team;
        [SF] private RectTransform rectTr;
        // Dependency
        private IObjectResolver _resolver;
        // Caching
        private CancellationTokenSource _tweenCts = new();
        private readonly List<Sprite> _requiredSprites = new(5);
        private readonly DisposableBagBuilder _disposableBag = DisposableBag.CreateBuilder();
        // Status
        private Vector2 _firstCardPos;
        private Vector2 _cardSize;
        private readonly Dictionary<int, OrderCard> _activeCardDic = new();
        private readonly Queue<OrderCard> _inactiveCardQueue = new();

        [Inject]
        private void Construct(
            StageData stageData,
            OrderCardData cardData,
            OrderCard prefab,
            IObjectResolver container,
            ISubscriber<AddOrderMessage> addSub,
            ISubscriber<RemoveOrderMessage> removeSub)
        {
            _stageData = stageData;
            _cardData = cardData;
            _prefab = prefab;
            _resolver = container;

            addSub.Subscribe(Apply, new TeamMessageFilter<AddOrderMessage>(team)).AddTo(_disposableBag);
            removeSub.Subscribe(Apply, new TeamMessageFilter<RemoveOrderMessage>(team)).AddTo(_disposableBag);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_stageData == null || rectTr == null) return;

            Debug.Log($"[OnDimension] boardWidth: {rectTr.rect.width} / borderHeight: {rectTr.rect.height}");

            var boardWidth = rectTr.rect.width;

            _cardSize = new Vector2(boardWidth / _stageData.OrderInfo.MaxActiveOrderCount, rectTr.rect.height);

            _firstCardPos = (team != Team.Red)
                ? new Vector2((_cardSize.x - boardWidth) / 2, 0)
                : new Vector2((boardWidth - _cardSize.x) / 2, 0);

            InitBoard(_cardSize.x, _cardSize.y);
        }

        private void Start()
        {
            if (_stageData == null || rectTr == null) return;

            Debug.Log($"[Start] boardWidth: {rectTr.rect.width} / borderHeight: {rectTr.rect.height}");

            var boardWidth = rectTr.rect.width;

            _cardSize = new Vector2(boardWidth / _stageData.OrderInfo.MaxActiveOrderCount, rectTr.rect.height);

            _firstCardPos = (team != Team.Red)
                ? new Vector2((_cardSize.x - boardWidth) / 2, 0)
                : new Vector2((boardWidth - _cardSize.x) / 2, 0);

            InitBoard(_cardSize.x, _cardSize.y);
        }

        private void OnDestroy()
        {
            _disposableBag.Build().Dispose();

            _tweenCts?.Cancel();
            _tweenCts?.Dispose();
        }

        private void InitBoard(float cardWidth, float cardHeight)
        {
            if (this.transform.childCount > 0)
            {
                foreach (var card in _inactiveCardQueue)
                {
                    card.SetCardSize(cardWidth, cardHeight);
                }
                return;
            }
            
            TeamTheme theme = (team) switch
            {
                Team.None => _cardData.CoopTheme,
                Team.Blue => _cardData.BlueTheme,
                Team.Red => _cardData.RedTheme,
                _ => _cardData.CoopTheme,
            };

            for (int i = 0; i < _stageData.OrderInfo.MaxActiveOrderCount; i++)
            {
                var card = CreateCard()
                    .SetCardSize(cardWidth, cardHeight)
                    .SetTeamTheme(theme.ImageColor, theme.FillOrigin);

                _inactiveCardQueue.Enqueue(card);
            }
        }

        public void Apply(AddOrderMessage data)
        {
            // CancelTween();
            ActivateCard(data);
        }

        public void Apply(RemoveOrderMessage data)
        {
            // CancelTween();
            DeactivateCard(data.TargetId);
            SortActiveCards();
        }

        private void CancelTween()
        {
            _tweenCts?.Cancel();
            _tweenCts?.Dispose();
            _tweenCts = new CancellationTokenSource();
        }

        private OrderCard CreateCard()
        {
            var card = _resolver.Instantiate(_prefab, this.transform);
            card.name = $"OrderCard_{card.GetHashCode()}";
            card.gameObject.SetActive(false);
            return card;
        }
        
        private void ActivateCard(AddOrderMessage msg)
        {
            var icons = this.GetRequiredIcons(msg.Recipe);

            var targetCard = _inactiveCardQueue.Dequeue()
                .ApplyIconSprites(icons)
                .ApplyOrderInfo(msg.Duration, msg.OrderTime);

            var pos = this.CalculatePos(targetCard.transform.GetSiblingIndex());

            _activeCardDic.Add(msg.Id, targetCard);

            targetCard.Show(pos, _tweenCts.Token).Forget(); // [임시]
        }

        private void DeactivateCard(int targetId)
        {
            if (!_activeCardDic.Remove(targetId, out var targetCard)) return;

            targetCard.InitStatus()
                .Hide(_tweenCts.Token).Forget(); // [임시]

            _inactiveCardQueue.Enqueue(targetCard);
        }

        private void SortActiveCards()
        {
            foreach (var card in _activeCardDic.Values)
            {
                var pos = CalculatePos(card.transform.GetSiblingIndex());
                card.Move(pos, _tweenCts.Token).Forget(); // [임시]
            }
        }

        private Vector2 CalculatePos(int cardIdx)
        {
            var pos = _firstCardPos;
            pos += (team != Team.Red)
                ? Vector2.right * (_cardSize.x * cardIdx)
                : Vector2.left * (_cardSize.x * cardIdx);

            return pos;
        }
        
        private List<Sprite> GetRequiredIcons(IngredientType recipe)
        {
            _requiredSprites.Clear();

            // 만약 재료 유형이 31개가 넘어가는 경우, 부호용 비트가 생겨서 딱 재료 타입만 담을 수 없게 됨
            // 딱 순수한 비트 데이터만 다루기 위해선 uint로 연산하는 게 관례
            uint uRecipe = (uint)recipe;

            while (uRecipe > 0)
            {
                // LSB Least Significant Beat : 활성화된 비트 중 가장 작은 값 (= 비트마스크에서 제일 오른쪽의 1)
                // uint는 음수가 불가능하기 때문에 잠시 int로 갔다가 다시 uint로 캐스팅
                // 눈에 보이는 숫자 자체는 int일 땐 -12였던 게 uint가 되면 막 4294967284로 변하지만,
                // 실제 비트 데이터 자체는 변하지 않는다는 게 포인트
                uint lsb = uRecipe & (uint)-(int)uRecipe;

                IngredientType curType = (IngredientType)lsb;

                if (_cardData.TryGetSprite(curType, out Sprite sprite))
                {
                    _requiredSprites.Add(sprite);
                }

                uRecipe ^= lsb; // XOR 연산으로 처리 완료된 LSB 제거 
            }

            return _requiredSprites;
        }
    }
}