// Fill out your copyright notice in the Description page of Project Settings.


#include "PokerPlayerState.h"

void APokerPlayerState::GetLifetimeReplicatedProps(TArray <FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(APokerPlayerState, BetAmount);
	DOREPLIFETIME(APokerPlayerState, Balance);
}

void APokerPlayerState::Server_ConfirmHandOption_Implementation(FBetAction action)
{
	auto gi = Cast<APokerGameSession>(UGameplayStatics::GetGameMode(this)->GameSession.Get());
	GEngine->AddOnScreenDebugMessage(0, 10, FColor::Red,
		                                FString::Printf(TEXT("Raised {%d} {%f}"), action.Type, UPokerEngine::CalculateTotalAmount(action.Amount, FCoinValueTable())));
}