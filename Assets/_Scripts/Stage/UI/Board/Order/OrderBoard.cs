using System.Collections.Generic;
using System.Threading;
using _Scripts.Stage._Data;
using _Scripts.Stage._Data.UI;
using _Scripts.Stage._Enums;
using _Scripts.Stage._Messages;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Board.Order
{
    public class OrderBoard: MonoBehaviour, IBoard<AddOrderMessage>, IBoard<RemoveOrderMessage>
    {
        // Data
        private StageData _stageData;
        private BoardUiData _boardData;
        private StageSfxListData _sfxList;
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
        private float[] _cardOffsetX;
        private float _showCardStartX;
        // Status
        private Vector2 _firstCardPos;
        private Vector2 _cardSize;
        private readonly Dictionary<int, OrderCard> _activeCardDic = new();
        private readonly Queue<OrderCard> _inactiveCardQueue = new();

        [Inject]
        private void Construct(
            StageData stageData,
            BoardUiData boardUiData,
            StageSfxListData sfxListData,
            OrderCardData cardData,
            OrderCard prefab,
            IObjectResolver container,
            ISubscriber<AddOrderMessage> addSub,
            ISubscriber<RemoveOrderMessage> removeSub,
            ISubscriber<EndStageMessage> endSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _stageData = stageData;
            _boardData = boardUiData;
            _sfxList = sfxListData;
            _cardData = cardData;
            _prefab = prefab;
            _resolver = container;

            addSub
                .Subscribe(Apply, new TeamMessageFilter<AddOrderMessage>(team))
                .AddTo(disposableBagBuilder);
            
            removeSub
                .Subscribe(Apply, new TeamMessageFilter<RemoveOrderMessage>(team))
                .AddTo(disposableBagBuilder);
            
            endSub
                .Subscribe(DeactivateBoard)
                .AddTo(disposableBagBuilder);

            InitBoard();
        }

        private void OnDestroy()
        {
            _tweenCts?.Cancel();
            _tweenCts?.Dispose();
        }

        private void InitBoard()
        {
            int maxOrder = _stageData.OrderInfo.MaxActiveOrderCount;
            float cardWidth = _prefab.GetComponent<RectTransform>().rect.width;
            
            _cardOffsetX = new float[maxOrder];
            
            this.rectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,cardWidth * maxOrder);
            
            float adjustment = (maxOrder - 1) * 0.5f;
            bool reversed = team == Team.Red;
            
            for (int i = 0; i < _cardOffsetX.Length; i++)
            {
                var offset = (i - adjustment) * cardWidth;
                _cardOffsetX[i] = reversed ? -offset : offset;
            }

            float sideOffset = _cardOffsetX[_cardOffsetX.Length-1];
            _showCardStartX = reversed ? sideOffset - cardWidth : sideOffset + cardWidth;
            
            TeamTheme theme = (team) switch
            {
                Team.None => _boardData.CoopTheme,
                Team.Blue => _boardData.BlueTheme,
                Team.Red => _boardData.RedTheme,
                _ => _boardData.CoopTheme,
            };

            for (int i = 0; i < maxOrder + 2; i++) // +2는 여유분
            {
                var card = CreateCard()
                    .InitStatus()
                    .SetTeamTheme(theme.ImageColor, theme.FillOrigin);

                _inactiveCardQueue.Enqueue(card);
            }
        }

        private void DeactivateBoard(EndStageMessage msg)
        {
            // CancelTween();
            
            foreach (var card in _activeCardDic.Values)
            {
                card.ApplyOrderInfo(-1f, -1f);
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
            DeactivateCard(data.TargetId, data.IsTimeout);
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

            var card = _inactiveCardQueue.Dequeue()
                .ApplyIconSprites(icons)
                .ApplyOrderInfo(msg.Duration, msg.OrderTime);

            var targetX = this.CalculatePosX(_activeCardDic.Count);

            _activeCardDic.Add(msg.OrderId, card);

            card.Show(_showCardStartX, targetX);
            
            _sfxList.Play(StageSfxType.NewOrder);
        }

        private void DeactivateCard(int targetId, bool isTimeout)
        {
            if (!_activeCardDic.Remove(targetId, out var targetCard)) return;
            
            targetCard.transform.SetAsLastSibling();
            
            targetCard.Hide();
            
            _inactiveCardQueue.Enqueue(targetCard);
            
            _sfxList.Play(isTimeout ? StageSfxType.OrderFailed : StageSfxType.OrderSuccess);
        }

        private void SortActiveCards()
        {
            foreach (var card in _activeCardDic.Values)
            {
                float targetX = CalculatePosX(card.transform.GetSiblingIndex());
                card.Move(targetX);
            }
        }
        
        private float CalculatePosX(int cardIdx)
        {
            return (cardIdx < 0 || cardIdx >= _cardOffsetX.Length) ? 0 : _cardOffsetX[cardIdx];
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