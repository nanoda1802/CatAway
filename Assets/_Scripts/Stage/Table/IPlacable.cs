using _Scripts.Stage.Item;
using _Scripts.Stage.Player;
using _Scripts.Stage.Player.Behaviour;

namespace _Scripts.Stage.Table
{
    public interface IPlacable
    {
        public Carriable PlacedItem { get; }
        public bool TryPlace(Carriable carriable);
        public bool TryDisplace(CarrierBehaviour carrier, out Carriable carriable);
    }
}