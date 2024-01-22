// Fill out your copyright notice in the Description page of Project Settings.


#include "PokerPlayerState.h"

#include "PokerPlayerController.h"

void APokerPlayerState::GetLifetimeReplicatedProps(TArray <FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(APokerPlayerState, BetAmount);
	DOREPLIFETIME(APokerPlayerState, Balance);
}

void APokerPlayerState::Server_ConfirmHandOption_Implementation(FBetAction action, APokerPlayerController* caller)
{
	auto gi = Cast<APokerGameSession>(UGameplayStatics::GetGameMode(this)->GameSession.Get());
	auto state = caller->GetPlayerState<APokerPlayerState>();
	auto bal = state->Balance;
	bal.Amount5 = 9999;
	state->Balance = bal;
	UE_LOG(LogTemp, Display, TEXT("Raised"));
	GEngine->AddOnScreenDebugMessage(0, 10, FColor::Red,
		                                FString::Printf(TEXT("Raised {%d} {%f}"), action.Type, UPokerEngine::CalculateTotalAmount(action.Amount, FCoinValueTable())));
}