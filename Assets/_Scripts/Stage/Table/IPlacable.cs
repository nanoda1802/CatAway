using _Scripts.Stage.Item;
using _Scripts.Stage.Player;
using _Scripts.Stage.Player.Behaviour;

namespace _Scripts.Stage.Table
{
    public interface IPlacable
    {
        public Carriable PlacedItem { get; }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        public bool TryPlace(Carriable carriable);
        public bool TryDisplace(CarrierBehaviour carrier, out Carriable carriable);
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

        public void Place(Carriable item);
        public bool CanPlace(Carriable item, out string rejectMessage);
        public bool CanDisPlace(out string rejectMessage);
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    }
}