using _Scripts.Stage.Player.Behaviour;

namespace _Scripts.Stage.Table
{
    public interface IInteractable
    {
        public bool IsInteracting { get; }
        public bool TryInteraction(InteractionBehaviour interactor, out int animParamHash);
        public void CancelInteraction(InteractionBehaviour interactor);
        public void FinishInteraction();
    }
}