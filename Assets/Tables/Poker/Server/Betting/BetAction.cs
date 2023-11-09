using Unity.Netcode;
namespace AceInTheHole.Tables.Poker.Server.Betting
{
    public struct BetAction : INetworkSerializable
    {
        public BetActionType Type;
        public int Amount;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Type);
                reader.ReadValueSafe(out Amount);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Type);
                writer.WriteValueSafe(Amount);
            }
        }
    }
}