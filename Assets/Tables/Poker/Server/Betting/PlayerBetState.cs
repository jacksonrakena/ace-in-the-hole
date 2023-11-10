using Unity.Netcode;
namespace AceInTheHole.Tables.Poker.Server.Betting
{
    public struct PlayerBetState : INetworkSerializable
    {
        public int Amount;
        public bool InRound;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Amount);
                reader.ReadValueSafe(out InRound);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Amount);
                writer.WriteValueSafe(InRound);
            }
        }

        public override string ToString()
        {
            return $"{(InRound ? "Folded" : $"In for ${Amount}")}";
        }
        
        public static void DuplicateNullable(in PlayerBetState? value, ref PlayerBetState? duplicatedValue)
        {
            duplicatedValue = value;
        }
        
        public static void ReadNullable(FastBufferReader reader, out PlayerBetState? value) 
        {
            reader.ReadValueSafe(out bool hasValue);
            if (hasValue)
            {
                reader.ReadValueSafe(out PlayerBetState v);
                value = v;
            }
            else value = null;
        }
        
        public static void WriteNullable(FastBufferWriter writer, in PlayerBetState? value) 
        {
            if (value.HasValue)
            {
                writer.WriteValueSafe(true);
                writer.WriteValueSafe(value.Value);
            }
            else
            {
                writer.WriteValueSafe(false);
            }
        }
    }
}