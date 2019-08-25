namespace Framework
{
    public interface IKeyValueStorage
    {
        void SetValue(string key, string value);
        string GetValue(string key);
        bool ContainsKey(string key);
        bool ClearValue(string key);
    }
}
