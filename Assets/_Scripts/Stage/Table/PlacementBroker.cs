using _Scripts.Stage._Data;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Cookware;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;

namespace _Scripts.Stage.Table
{
    public class PlacementBroker
    {
        public BrokerResult AcceptCase(CarrierBehaviour carrier, IPlacable table)
        {
            switch (carrier.CarriedItem, table.PlacedItem)
            {
                case (null,Ingredient)
                or (null,Plate)
                or (null,Cookware):
                    return HandlePickCase(carrier, table);
                
                case (Ingredient,null)
                or (Plate,null)
                or (Cookware,null):
                    return HandlePlaceCase(carrier.CarriedItem, table);
                
                case (Ingredient, Plate)
                or (Ingredient, Cookware):
                    return HandleHoldCase(carrier.CarriedItem, table.PlacedItem);
                
                case (Plate, Ingredient)
                or (Cookware, Ingredient):
                    return HandleHoldCase(table.PlacedItem, carrier.CarriedItem);
                
                case (Plate, Cookware):
                    return HandleHolderToHolderCase(carrier.CarriedItem, table.PlacedItem);
                
                case (Cookware, Plate):
                    return HandleHolderToHolderCase(table.PlacedItem,carrier.CarriedItem);
                    
                default:
                    return new BrokerResult(false, "Broker가 처리할 수 없는 상황입니다.");
            }
        }

        public BrokerResult AcceptCase(Ingredient ingredient, IPlacable table)
        {
            switch (table.PlacedItem)
            {
                case null:
                    return HandlePlaceCase(ingredient, table);
                
                case Ingredient:
                    return new BrokerResult(false, "이미 Place된 Ingredient가 있습니다.");
                
                case Plate or Cookware:
                    return HandleHoldCase(ingredient, table.PlacedItem);
                
                default:
                    return new BrokerResult(false, "Broker가 처리할 수 없는 상황입니다.");
            }
        }

        private BrokerResult HandlePickCase(CarrierBehaviour carrier, IPlacable table)
        {
            if (!table.CanDisPlace(out var reason)) return new BrokerResult(false, reason);
            
            carrier.Pick(table.PlacedItem);
            return new BrokerResult(true, null);
        }

        private BrokerResult HandlePlaceCase(Carriable carriedItem, IPlacable table)
        {
            if (!table.CanPlace(carriedItem, out var reason)) return new BrokerResult(false, reason);
            
            table.Place(carriedItem);
            return new BrokerResult(true, null);
        }

        private BrokerResult HandleHoldCase(Carriable potentialIngredient, Carriable potentialHolder)
        {
            if (potentialIngredient is not Ingredient ingredient)
                return new BrokerResult(false, "Item을 Ingredient로 형변환하는 데 실패했습니다.");
            if (potentialHolder is not IIngredientHolder holder)
                return new BrokerResult(false, "Item을 Plate로 형변환하는 데 실패했습니다.");
            
            if (!holder.CanHold(ingredient, out var reason)) 
                return new BrokerResult(false, reason);
                    
            holder.Hold(ingredient);
            return new BrokerResult(true, null);
        }

        private BrokerResult HandleHolderToHolderCase(Carriable potentialPlate, Carriable potentialCookware)
        {
            if (potentialPlate is not Plate plate)
                return new BrokerResult(false, "Item을 Plate로 형변환하는 데 실패했습니다.");
            if (potentialCookware is not Cookware cookware)
                return new BrokerResult(false, "Item을 Cookware로 형변환하는 데 실패했습니다.");

            if (!cookware.HasIngredient)
                return new BrokerResult(false, "Cookware가 hold 중인 Ingredient가 없습니다.");
            if (!plate.CanHold(cookware.HeldIngredient, out var reason))
                return new BrokerResult(false, reason);
            
            plate.Hold(cookware.HeldIngredient);
            return new BrokerResult(true, null);
        }
    }
}