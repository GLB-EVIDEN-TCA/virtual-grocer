using System.Collections.Concurrent;

namespace Eviden.VirtualGrocer.Web.Server.Skills.History
{
    public class MemoryStorageContext<T> : IStorageContext<T>
    {
        private readonly ConcurrentDictionary<string, T> _storage = new();
        public Task<bool> Create(string key, T value) =>
            Task.FromResult(_storage.TryAdd(key, value));

        public Task<bool> Delete(string key) =>
            Task.FromResult(_storage.TryRemove(key, out _));

        public Task<T?> Get(string key)
        {
            _storage.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task Set(string key, T value)
        {
            _storage.AddOrUpdate(key, value, (k, v) => value);
            return Task.CompletedTask;
        }
    }
}
