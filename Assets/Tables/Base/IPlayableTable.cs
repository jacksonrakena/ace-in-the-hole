using Unity.Netcode;
namespace AceInTheHole.Tables.Base
{
    public interface IPlayableTable
    {
        public void JoinClientToTable(ulong clientId);
        public PlayableTableInfo TableInfo { get; }

        public NetworkVariable<int> PlayerCount { get; }
    }

    public struct PlayableTableInfo
    {
        public string Name;
        public string Description;
        public int MinimumPlayers;
        public int MaximumPlayers;
    }
}