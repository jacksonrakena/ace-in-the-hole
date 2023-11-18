using Unity.Netcode.Components;
namespace AceInTheHole.Player
{
    public class ClientNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
