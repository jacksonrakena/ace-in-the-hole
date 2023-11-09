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
    }
}