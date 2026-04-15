namespace _Scripts.Stage._Data.Level
{
    public interface IProviderInfo
    {
        public string ObjNamePrefix { get; }
        public int DefaultCount { get; }
        public int MaxCount { get; }
    }
}