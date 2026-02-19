namespace _Scripts.Stage.Item
{
    public interface IPrepable
    {
        public bool IsReady { get; }
        public float Prepare(int multiplier);
        public void OnPrepFinished();
    }
}