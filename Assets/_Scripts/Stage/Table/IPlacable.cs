using _Scripts.Stage.Item;

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