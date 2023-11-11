# Ace In The Hole

Ace In The Hole is a very early in-development multiplayer Unity game that allows players to play a variety of poker table games against other players over their local network, Unity Relay, or (in the future) the Steamworks Relay.

## Features
- Full, 3D, online multiplayer
  - Local Network support
  - Unity Relay support for playing over the Internet using a join code
  - (Future?) Steamworks Relay/Epic Online Services P2P?
  - Dedicated server partially working
- Poker (Texas Hold 'Em) implemented
  - Raising, checking, folding
  - Little and big blind rotation
  - Pot splitting/all ins not implemented yet
- Blackjack partially implemented
- A lot of generic code written to allow for easy implementation of other tables
  - [OrdinalTableBase](Assets/Tables/Base/OrdinalTableBase.cs) and [OrdinalTablePlayerStateBase](Assets/Tables/Base/OrdinalTablePlayerStateBase.cs) allow for quick development of new tables which have ordered seating

# Games
## Poker (Texas Hold 'Em Style)
Fully working, with the exception of pot splitting
![Alt text](readme/0.png)

## Blackjack (Dealer Draw 16, Stand 17, 2:1)
Not fully modelled yet.
![Alt text](readme/1.png)


#### Copyright
&copy; All rights reserved