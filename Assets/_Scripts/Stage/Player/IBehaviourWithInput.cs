using _Scripts.Stage._Messages;

namespace _Scripts.Stage.Player
{
    public interface IBehaviourWithInput
    {
        public void SubscribeInputEvents(StartStageMessage msg);
        public void UnsubscribeInputEvents(EndStageMessage msg);
    }
}