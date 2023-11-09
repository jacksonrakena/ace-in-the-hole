using System;
using System.Collections.Generic;
using System.Linq;
using AceInTheHole.Tables.Poker.Client;
using AceInTheHole.Tables.Poker.Server.Betting;
using Unity.Netcode;
namespace AceInTheHole.Tables.Poker.Server
{
    public struct PotState : INetworkSerializable, IClone<PotState>
    {
        public Dictionary<ulong, PlayerBetState> PlayerBetStates;
        public int CurrentRequiredBet;
        public int Pot;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out ulong[] keys);
                reader.ReadValueSafe(out PlayerBetState[] values);
                PlayerBetStates = keys.Zip(values, KeyValuePair.Create).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                reader.ReadValueSafe(out CurrentRequiredBet);
                reader.ReadValueSafe(out Pot);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(PlayerBetStates?.Keys.ToArray() ?? Array.Empty<ulong>());
                writer.WriteValueSafe(PlayerBetStates?.Values.ToArray() ?? Array.Empty<PlayerBetState>());
                writer.WriteValueSafe(CurrentRequiredBet);
                writer.WriteValueSafe(Pot);
            }
        }
        public PotState Clone()
        {
            return new PotState { CurrentRequiredBet = CurrentRequiredBet, Pot = Pot, PlayerBetStates = new Dictionary<ulong, PlayerBetState>(PlayerBetStates) };
        }

        public PlayerBetState? GetCurrentBetStateFor(PokerPlayerState cont)
        {
            if (this.PlayerBetStates == null) return null;
            if (!this.PlayerBetStates.ContainsKey(cont.OwnerClientId)) return null;
            return this.PlayerBetStates[cont.OwnerClientId];
        }
        
        public int GetCurrentBetAmountFor(PokerPlayerState cont)
        {
            var pbs = GetCurrentBetStateFor(cont);
            return pbs?.Amount ?? 0;
        }

        public int GetRequiredRaiseFor(PokerPlayerState cont)
        {
            return this.CurrentRequiredBet - this.GetCurrentBetAmountFor(cont);
        }

        public bool HasFolded(PokerPlayerState cont)
        {
            return !GetCurrentBetStateFor(cont)?.InRound ?? false;
        }
    }
}