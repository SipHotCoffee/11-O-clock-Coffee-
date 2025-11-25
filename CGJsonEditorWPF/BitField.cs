using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace CG.Test.Editor
{
    public struct BitField<TInt>(TInt value) : IEquatable<BitField<TInt>>, IReadOnlyList<bool> where TInt : unmanaged, IBinaryInteger<TInt>, IUnsignedNumber<TInt>, IMinMaxValue<TInt>
    {
        private TInt _value = value;

        public static BitField<TInt> AllBits { get; } = new(TInt.AllBitsSet);
        public static BitField<TInt> NoBits { get; } = new(TInt.Zero);

        public unsafe readonly int Count => sizeof(TInt) * 8;

        public bool this[int index]
        {
            readonly get
            {
                var mask = TInt.One << index;
                return (_value & mask) != TInt.Zero;
            }
            set
            {
                var mask = TInt.One << index;
                if (value)
                {
                    _value |= mask;
                }
                else
                {
                    _value &= ~mask;
                }
            }
        }
        
        public readonly IEnumerator<bool> GetEnumerator()
        {
            for (var mask = TInt.One; mask < TInt.MaxValue; mask <<= 1)
            {
                yield return (_value & mask) != TInt.Zero;
            }
        }

        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static BitField<TInt> operator &(BitField<TInt> left, BitField<TInt> right) => new(left._value & right._value);
        public static BitField<TInt> operator |(BitField<TInt> left, BitField<TInt> right) => new(left._value | right._value);
        public static BitField<TInt> operator ^(BitField<TInt> left, BitField<TInt> right) => new(left._value ^ right._value);

        public static BitField<TInt> operator ~(BitField<TInt> value) => new(~value._value);

        public readonly bool Equals(BitField<TInt> other) => _value == other._value;

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is BitField<TInt> other && Equals(other);

        public override readonly int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(BitField<TInt> left, BitField<TInt> right) => left.Equals(right);
        public static bool operator !=(BitField<TInt> left, BitField<TInt> right) => !left.Equals(right);

    }
}
