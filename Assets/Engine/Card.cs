using System;
using Unity.Netcode;
using UnityEngine;
namespace AceInTheHole.Engine
{
    public struct Card : INetworkSerializable, IEquatable<Card>, IComparable<Card>
    {
        public Suit Suit;
        public Number Number;

        public override string ToString()
        {
            return this.Suit.GetId() + this.Number.GetId();
        }

        public Texture2D Resolve2D()
        {
            return Resources.Load<Texture2D>("Cards2D/" + this.Number.GetId() + this.Suit.GetId());
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Suit);
                reader.ReadValueSafe(out Number);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Suit);
                writer.WriteValueSafe(Number);
            }
        }

        public bool Equals(Card other)
        {
            return other.Suit == this.Suit && other.Number == this.Number;
        }
        public int CompareTo(Card other)
        {
            return Number.CompareTo(other.Number);
        }
    }
}