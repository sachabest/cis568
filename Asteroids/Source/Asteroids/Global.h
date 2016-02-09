// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "GameFramework/Actor.h"
#include "Asteroid.h"
#include "Global.generated.h"

UCLASS()
class ASTEROIDS_API AGlobal : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	AGlobal();

	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	
	// Called every frame
	virtual void Tick( float DeltaSeconds ) override;

	FTimerHandle TimerHandle;

	TSubclassOf<class AAsteroid> AsteroidClass;
	void SpawnAsteroids();
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Global")
	int32 Score;

	UFUNCTION()
	void incrementScore();
	
};
