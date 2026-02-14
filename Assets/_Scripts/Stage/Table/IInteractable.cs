using _Scripts.Stage.Player;

namespace _Scripts.Stage.Table
{
    public interface IInteractable
    {
        public int AnimParamHash { get; set; }
        public bool TryInteractStart(InteractionBehaviour interactor);
        public bool TryInteractStop(InteractionBehaviour interactor);
    }
}