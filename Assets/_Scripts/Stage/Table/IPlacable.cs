using _Scripts.Stage.Item;
using _Scripts.Stage.Player;
using _Scripts.Stage.Player.Behaviour;

namespace _Scripts.Stage.Table
{
    public interface IPlacable
    {
        public Carriable PlacedItem { get; }

        public void Place(Carriable item);
        public bool CanPlace(Carriable item, out string rejectMessage);
        public bool CanDisPlace(out string rejectMessage);
    }
}