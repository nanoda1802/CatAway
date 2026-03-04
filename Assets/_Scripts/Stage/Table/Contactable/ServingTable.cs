using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.Table.Placable;
using _Scripts.Stage.UI.Board;
using _Scripts.Stage.UI.Board.Order;
using _Scripts.Stage.UI.Board.Score;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Contactable
{
    public class ServingTable : NetworkBehaviour, IContactable
    {
        [SF] private Team team;

        // Dependency
        private StageHub _stageHub;
        
        [Inject]
        private void Construct(StageHub stageHub)
        {
            _stageHub = stageHub;
        }

        #region Contactable 관련 메서드
        public bool TryContact(Carriable item, out string failMessage)
        {
            failMessage = null;
            
            if (item is Plate { IsWellPrepped : true } and { HasIngredient : true })
            {
                return true;
            }
            
            failMessage = "ServingRack엔 Plating이 있는 Plate만 제출할 수 있습니다.";
            return false;
        }

        public void RespondTo(Plate plate)
        {
            var orderPresenter = _stageHub.FetchOrderPresenter(team);
            
            if (!orderPresenter.CheckRecipe(plate.Plating)) return;
            
            plate.ClearHolder();
            
            var returnTable = _stageHub.FetchPlacable<PlateReturnTable>();
            returnTable.Place(plate);
        }
        #endregion
    }
}