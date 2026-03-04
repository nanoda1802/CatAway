namespace _Scripts.Stage
{
    public interface IProviderInfo
    {
        public string ObjNamePrefix { get; }
        public int DefaultCount { get; }
        public int MaxCount { get; }
    }
}