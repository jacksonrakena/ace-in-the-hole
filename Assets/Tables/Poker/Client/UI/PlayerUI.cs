using System;
using System.Collections.Generic;
using AceInTheHole.Tables.Poker.Server;
using UnityEngine;
using UnityEngine.UIElements;
namespace AceInTheHole.Tables.Poker.Client.UI
{
    public class PlayerUI : MonoBehaviour
    {
        UIDocument _cardsUI;

        public bool RevalidateOnFrame => false;

        readonly List<IUserInterfaceModule> _uiModules = new List<IUserInterfaceModule>
        {
            new HostModule(),
            new MoneyModule(),
            new PlayerCardsModule(),
            new TableCardsModule(),
            new PlayInModule(),
            new ActionModule()
        };
        
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _cardsUI = GetComponentInParent<UIDocument>();
        }
        
        public void Configure(PokerPlayerState pokerPlayerState, PokerTableState pokerTableState)
        {
            foreach (var module in _uiModules)
            {
                module.PokerPlayerState = pokerPlayerState;
                module.PokerTableState = pokerTableState;
                module.Connect(_cardsUI);
            }
            if (!RevalidateOnFrame)
            {
                pokerPlayerState.balance.OnValueChanged += (old, @new) => Revalidate();
                pokerTableState.potState.OnValueChanged += (old, @new) => Revalidate();
                pokerPlayerState.Cards.OnListChanged += (_) => Revalidate();
                pokerTableState.VisibleTableCards.OnListChanged += _ => Revalidate();
                pokerTableState.currentPlayerSeatId.OnValueChanged += (_, _) => Revalidate();
                pokerTableState.stage.OnValueChanged += (_, _) => Revalidate();
                pokerTableState._nPlayerCount.OnValueChanged += (_, _) => Revalidate();
            
                Revalidate();
            }
        }

        public void Update()
        {
            if (RevalidateOnFrame)
            {
                Revalidate();
            }
        }
        public bool destroyed = false;

        public void OnDestroy()
        {
            destroyed = true;
        }
        public void Revalidate()
        {
            if (destroyed) return;
            foreach (var module in _uiModules)
            {
                module.Revalidate();
            }
        }
    }
}
