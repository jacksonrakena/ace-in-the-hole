namespace AceInTheHole.Tables.Poker.Server
{
    public enum RoundStage
    {
        /*
         * Waiting for players to connect/play in
         */
        Setup,
        
        /*
         * Blinds have bet. Other players can view their cards before deciding to bet, or fold.
         */
        PlayIn,
        
        /**
         * First three cards
         */
        Flop,
        
        /**
         * Fourth card
         */
        Turn,
        
        /**
         * Fifth card
         */
        River,
        
        /*
         * End of hand
         */
        End
    }
}