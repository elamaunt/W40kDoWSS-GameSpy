namespace Framework
{
    public interface IDataBundle
    {
        void SetString(string key, string value);
        string GetString(string key, string defaultValue = null);
    }
}