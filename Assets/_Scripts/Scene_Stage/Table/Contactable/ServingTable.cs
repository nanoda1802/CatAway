using _Scripts.Scene_Stage.Enums;
using _Scripts.Scene_Stage.Item;
using _Scripts.Scene_Stage.Item.Plate;
using _Scripts.Scene_Stage.Table.Placable;
using Unity.Netcode;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Table.Contactable
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

        public void RespondTo(Plate plate, ulong contactorId)
        {
            var orderPresenter = _stageHub.FetchOrderPresenter(team);
            
            if (!orderPresenter.CheckRecipe(plate.Plating, contactorId)) return;
            
            plate.ClearHolder();
            
            var returnTable = _stageHub.FetchPlacable<PlateReturnTable>();
            returnTable.Place(plate);
        }
        #endregion
    }
}