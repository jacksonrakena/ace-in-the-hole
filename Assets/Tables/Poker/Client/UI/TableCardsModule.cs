using System.Collections.Generic;
using AceInTheHole.Tables.Poker.Server;
using UnityEngine;
using UnityEngine.UIElements;
namespace AceInTheHole.Tables.Poker.Client.UI
{
    public class TableCardsModule : IUserInterfaceModule
    {
        public PokerPlayerState PokerPlayerState { get; set; }
        public PokerTableState PokerTableState { get; set; }
        
        VisualElement _dealer;
        
        public void Connect(UIDocument document)
        {
            _dealer = document.rootVisualElement.Q<VisualElement>("Dealer").Q<VisualElement>("cardcontainer");
            
        }
        public void Revalidate()
        {
            int v = 0;
            foreach (var child in _dealer.Children())
            {
                child.style.backgroundImage = PokerTableState.VisibleTableCards.Count > v ? PokerTableState.VisibleTableCards[v].Resolve2D() : null;
                v++;
            }
            var dealerCardObject = GameObject.Find("Dealer Cards");
            if (dealerCardObject != null)
            {
                var cardRenderers = GameObject.Find("Dealer Cards").GetComponentsInChildren<MeshRenderer>();
                for (var i = 0; i < cardRenderers.Length; i++)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    if (PokerTableState.VisibleTableCards.Count > i)
                    {
                        mat.mainTexture = PokerTableState.VisibleTableCards[i].Resolve2D();
                        cardRenderers[i].SetMaterials(new List<Material> { mat });
                    }
                    else
                    {
                        cardRenderers[i].SetMaterials(new List<Material>());
                    }
                }   
            }
        }
    }
}