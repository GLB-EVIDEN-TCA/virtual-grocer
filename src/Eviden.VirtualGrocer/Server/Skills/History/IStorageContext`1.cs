namespace Eviden.VirtualGrocer.Web.Server.Skills.History
{
    public interface IStorageContext<T>
    {
        Task<T?> Get(string key);

        Task<bool> Create(string key, T value);

        Task Set(string key, T value);

        Task<bool> Delete(string key);
    }
}
