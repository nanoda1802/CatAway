using _Scripts.Scene_Stage.Item;

namespace _Scripts.Scene_Stage.Table
{
    public interface IPlacable
    {
        public Carriable PlacedItem { get; }

        public void Place(Carriable item);
        public bool CanPlace(Carriable item, out string rejectMessage);
        public bool CanDisPlace(out string rejectMessage);
    }
}