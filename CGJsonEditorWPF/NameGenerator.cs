namespace CG.Test.Editor
{
    public class NameGenerator
    {
        private readonly Dictionary<string, int> _names;

        private readonly string _defaultName;
        private readonly string _copySuffix;

        public NameGenerator(IEnumerable<string> elements, string defaultName, string copySuffix)
        {
            _names = [];

            _defaultName = defaultName;
            _copySuffix  = copySuffix;

            Reset(elements);
        }

        public void Reset(IEnumerable<string> elements)
        {
            _names.Clear();

            foreach (var element in elements)
            {
                var startsWithDefault = element.StartsWith(_defaultName);
                var copySuffixIndex = element.LastIndexOf(_copySuffix);

                if (startsWithDefault && copySuffixIndex == -1)
                {
                    var suffixStartIndex = _defaultName.Length;
                    var number = 1;
                    if (suffixStartIndex == element.Length || int.TryParse(element.AsSpan(suffixStartIndex), out number))
                    {
                        _names.TryAdd(_defaultName, number);
                        _names[_defaultName] = Math.Max(_names[_defaultName] + 1, number + 1);
                    }
                }
                else
                {
                    if (copySuffixIndex != -1)
                    {
                        var suffixStartIndex = copySuffixIndex + _copySuffix.Length;
                        var number = 1;
                        var nameWithoutNumber = element[..suffixStartIndex];
                        if (suffixStartIndex == element.Length)
                        {
                            _names.TryAdd(nameWithoutNumber, number);
                        }
                        else if (int.TryParse(element.AsSpan(suffixStartIndex), out number))
                        {
                            if (_names.TryGetValue(nameWithoutNumber, out var count))
                            {
                                _names[nameWithoutNumber] = Math.Max(count + 1, number + 1);
                            }
                        }
                    }
                }
            }
        }

        public string GenerateNew()
        {
            if (!_names.TryGetValue(_defaultName, out var nextId))
            {
                nextId = 1;
            }
            _names[_defaultName] = nextId + 1;
            return $"{_defaultName}{nextId}";
        }

        public string GenerateCopy(string sourceName)
        {
            var result = sourceName + _copySuffix;

            if (_names.TryGetValue(result, out var nextId))
            {
                _names[result] = nextId + 1;
                result += nextId;
            }
            else
            {
                _names.Add(result, 1);
            }

            return result;
        }
    }
}
