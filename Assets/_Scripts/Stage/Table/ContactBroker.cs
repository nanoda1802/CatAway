using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Cookware;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;

namespace _Scripts.Stage.Table
{
    public class ContactBroker
    {
        public BrokerResult AcceptCase(CarrierBehaviour carrier, IContactable table)
        {
            if (!table.TryContact(carrier.CarriedItem, out string reason))
                return new BrokerResult(false, reason);
            
            switch (carrier.CarriedItem)
            {
                case null:
                    table.RespondTo(carrier); break;
                
                case Ingredient ingredient:
                    table.RespondTo(ingredient); break;
                
                case Plate plate:
                    table.RespondTo(plate); break;
                
                case Cookware cookware:
                    table.RespondTo(cookware); break;
                
                default:
                    return new BrokerResult(false, "Broker가 처리할 수 없는 상황입니다.");
            }
            
            return new BrokerResult(true, null);
        }
        
        public BrokerResult AcceptCase(Ingredient ingredient, IContactable table)
        {
            if (!table.TryContact(ingredient, out string reason))
                return new BrokerResult(false, reason);

            table.RespondTo(ingredient);
            
            return new BrokerResult(true, null);
        }
    }
}