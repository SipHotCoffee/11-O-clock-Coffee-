using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.FrontEnd
{
    

    public class DictionaryQueue<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, int> _dictionary = [];

        private readonly List<KeyValuePair<TKey, TValue>> _keyQueue = [];

        public int Count => _keyQueue.Count;

        public void Enqueue(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var foundIndex))
            {
                (_keyQueue[^1], _keyQueue[foundIndex]) = (_keyQueue[foundIndex], _keyQueue[^1]);
            }
            else
            {
                foundIndex = _dictionary.Count;
                _keyQueue.Add(new KeyValuePair<TKey, TValue>(key, value));
                _dictionary.Add(key, foundIndex);
            }
        }

        public bool TryDequeue([MaybeNullWhen(false)] out TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            if (_keyQueue.Count > 0)
            {
                var lastItem = _keyQueue[^1];
                
                key   = lastItem.Key;
                value = lastItem.Value;

                _dictionary.Remove(key);
				_keyQueue.RemoveAt(_keyQueue.Count - 1);
                return true;
            }

            key   = default;
            value = default;
            return false;
		}
    }
}
