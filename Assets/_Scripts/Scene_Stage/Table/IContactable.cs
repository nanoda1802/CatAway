using _Scripts.Scene_Stage.Item;
using _Scripts.Scene_Stage.Item.Cookware;
using _Scripts.Scene_Stage.Item.Ingredient;
using _Scripts.Scene_Stage.Item.Plate;
using _Scripts.Scene_Stage.Player.Behaviour;

namespace _Scripts.Scene_Stage.Table
{
    public interface IContactable
    {
        public bool TryContact(Carriable item, out string failMessage);

        public void RespondTo(CarrierBehaviour carrier) { }
        public void RespondTo(Ingredient ingredient) { }
        public void RespondTo(Plate plate, ulong carrierId) { }
        public void RespondTo(Cookware cookware) { }
        
    }
}