using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.FrontEnd
{
    public class DictionaryQueue<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary = [];

        private readonly Queue<KeyValuePair<TKey, TValue>> _queue = [];

        public int Count => _queue.Count;

        public bool Enqueue(TKey key, TValue value)
        {
            if (_dictionary.TryAdd(key, value))
            {
                _queue.Enqueue(new KeyValuePair<TKey, TValue>(key, value));
                return true;
            }
            return false;
        }

        public bool TryDequeue([MaybeNullWhen(false)] out TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            var result = _queue.TryDequeue(out var pair) && _dictionary.Remove(pair.Key);
            key   = pair.Key;
            value = pair.Value;
            return result;
		}
    }
}
