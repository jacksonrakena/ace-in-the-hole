# Ace In The Hole

Ace In The Hole is a very early in-development multiplayer Unreal Engine game that allows players to play a variety of poker table games against other players over their local network, Epic Online Services, or (in the future) the Steamworks Relay.

Due to GitHub LFS size restrictions, the GitHub repository is frozen. Further development past November 2023 is done on [my Git server](https://scm.jacksonrakena.com/jackson/ace-in-the-hole).

## Features
- Full, 3D, online multiplayer
  - Local Network support
  - Epic Online Services P2P support for playing over the Internet using a join code
  - (Future?) Steamworks Relay P2P?
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
![Alt text](https://i.imgur.com/3eIiBqO.png)

## Blackjack (Dealer Draw 16, Stand 17, 2:1)
Not fully modelled yet.
![Alt text](https://i.imgur.com/svtxj8w.png)


#### Copyright
&copy; All rights reserved
