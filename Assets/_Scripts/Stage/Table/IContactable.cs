using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Cookware;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;

namespace _Scripts.Stage.Table
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