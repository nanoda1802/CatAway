namespace _Scripts.Stage.Item
{
    public interface IPrepable
    {
        public bool IsWellPrepped { get; }
        public float Prepare(int multiplier);
        public void OnPrepCompleted();
    }
}