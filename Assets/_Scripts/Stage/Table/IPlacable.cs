using _Scripts.Stage.Item;
using _Scripts.Stage.Player;

namespace _Scripts.Stage.Table
{
    public interface IPlacable
    {
        public bool TryPlace(Carriable carriable);
        public bool TryDisplace(CarrierBehaviour carrier);
    }
}