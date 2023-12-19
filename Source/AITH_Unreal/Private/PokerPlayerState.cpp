// Fill out your copyright notice in the Description page of Project Settings.


#include "PokerPlayerState.h"

void APokerPlayerState::GetLifetimeReplicatedProps(TArray <FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(APokerPlayerState, BetAmount);
}