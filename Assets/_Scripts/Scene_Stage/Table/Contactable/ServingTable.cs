using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Enums;
using _Scripts.Scene_Stage.Item;
using _Scripts.Scene_Stage.Item.Plate;
using _Scripts.Scene_Stage.Table.Placable;
using _Scripts.Scene_Stage.UI.Widget.Toast;
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
        private StageMode _curMode;
        
        [Inject]
        private void Construct(
            StageHub stageHub,
            StageData stageData)
        {
            _stageHub = stageHub;
            _curMode = stageData.Mode;
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
            var contactor = RpcTarget.Single(contactorId, RpcTargetUse.Temp);
            
            var orderPresenter = _stageHub.FetchOrderPresenter(team);

            bool hasMatchOrder = orderPresenter.CheckRecipe(plate.Plating, contactorId, out int point);
            
            ActivateToastWidgetRpc(point,contactor);
            
            if (!hasMatchOrder) return;
            
            plate.ClearHolder(_curMode == StageMode.Comp);
            
            var returnTable = _stageHub.FetchPlateReturnTable(team);
            returnTable.Place(plate);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void ActivateToastWidgetRpc(int point, RpcParams rpcParams)
        {
            var toastProvider = _stageHub.FetchProvider<ToastProvider>();
            var widget = toastProvider.GetWidget(transform.position);

            if (point < 0)
            {
                widget.SetText("No Match Order!", false);
            }
            else
            {
                widget.SetText($"+{point}", true);
            }
        }
        #endregion
    }
}