using _Scripts.Messages.Stage;

namespace _Scripts.Scene_Stage.Player
{
    public interface IBehaviourWithInput
    {
        public void SubscribeInputEvents(StartStageMessage msg);
        public void UnsubscribeInputEvents(EndStageMessage msg);
    }
}