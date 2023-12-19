// Fill out your copyright notice in the Description page of Project Settings.


#include "PokerTableState.h"

#include "Net/UnrealNetwork.h"

void APokerTableState::BeginPlay()
{
	Super::BeginPlay();
	if (GetLocalRole() == ROLE_Authority)
	{
		Deck = NewObject<UCardDeck>();		
	}
}
void APokerTableState::OnRep_Deck(){}
void APokerTableState::GetLifetimeReplicatedProps(TArray <FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(APokerTableState, Deck);
}