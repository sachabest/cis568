// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "GameFramework/Actor.h"
#include "Bullet.generated.h"

UCLASS()
class ASTEROIDS_API ABullet : public AActor
{
	GENERATED_BODY()
	
public:
	// Sets default values for this actor's properties
	ABullet(const FObjectInitializer& ObjectInitializer);
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	// Called every frame
	virtual void Tick(float DeltaSeconds) override;
	// Sphere collision component
	USphereComponent* CollisionComp;
	// Projectile movement component
	UProjectileMovementComponent* ProjectileMovement;
	// inits velocity of the projectile in the shoot direction
	void InitVelocity(const FVector& ShootDirection);
};
