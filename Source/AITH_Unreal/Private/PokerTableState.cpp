// Fill out your copyright notice in the Description page of Project Settings.


#include "PokerTableState.h"

#include "Net/UnrealNetwork.h"

void APokerTableState::BeginPlay()
{
	Super::BeginPlay();
	if (GetLocalRole() == ROLE_Authority)
	{
		FUCard Card;
		Card.Number = static_cast<ENumber>(FMath::RandRange(0, 12));
		Card.Suit = static_cast<ESuit>(FMath::RandRange(0, 3));
		Cards.Emplace(Card);

		FUCard Card2;
		Card2.Number = static_cast<ENumber>(FMath::RandRange(0, 12));
		Card2.Suit = static_cast<ESuit>(FMath::RandRange(0, 3));
		Cards.Emplace(Card2);
	}
}
void APokerTableState::OnRep_Deck(){}
void APokerTableState::GetLifetimeReplicatedProps(TArray <FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(APokerTableState, Cards);
}
