using _Scripts.Stage.Player;
using _Scripts.Stage.Player.Behaviour;

namespace _Scripts.Stage.Table
{
    public interface IInteractable
    {
        public bool TryInteractStart(InteractionBehaviour interactor, out int animParamHash);
        public bool TryInteractStop(InteractionBehaviour interactor, out int animParamHash);
    }
}