// Fill out your copyright notice in the Description page of Project Settings.

#include "Asteroids.h"
#include "Ship.h"


// Sets default values
AShip::AShip()
{
	// Set this actor to call Tick() every frame. You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;
	// Set this pawn to be controlled by the lowest-numbered player
	AutoPossessPlayer = EAutoReceiveInput::Player0;
	// Our root component will be a sphere that reacts to physics

	ShipSphereComponent = CreateDefaultSubobject<USphereComponent>(TEXT("RootComponent"));
	RootComponent = ShipSphereComponent;
	ShipSphereComponent->InitSphereRadius(10.0f);
	ShipSphereComponent->SetCollisionProfileName(TEXT("Pawn"));
	ShipSphereComponent->SetSimulatePhysics(true);
	ShipSphereComponent->SetEnableGravity(false);
	ShipSphereComponent->SetLinearDamping(0.3);
	ShipSphereComponent->SetAngularDamping(1);
	ShipSphereComponent->SetLockedAxis(EDOFMode::XYPlane);
	// Load the ship material
	static ConstructorHelpers::FObjectFinder<UMaterial> Material0(TEXT("Material'/Game/ship_material.ship_material'"));
	if (Material0.Object != NULL)
	{
		ShipMaterial = (UMaterial*)Material0.Object;
		UE_LOG(LogTemp, Warning, TEXT("Found material."));

	}
	else {
		UE_LOG(LogTemp, Error, TEXT("Couldn't find material."));
	}
	// Create and position a mesh component so we can see where our ship is
	UStaticMeshComponent* ShipVisual = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("VisualRepresentation"));
	ShipVisual->AttachTo(RootComponent);
	static ConstructorHelpers::FObjectFinder<UStaticMesh> ShipVisualAsset(TEXT("/Game/vehicle_playerShip.vehicle_playerShip"));
	if (ShipVisualAsset.Succeeded())
	{
		ShipVisual->SetStaticMesh(ShipVisualAsset.Object);
		ShipVisual->SetRelativeLocation(FVector(0.0f, 0.0f, 0.0f));
		ShipVisual->SetRelativeRotation(FRotator(0.0f, -90.0f, 0.0f));
		ShipVisual->SetWorldScale3D(FVector(0.2f));
		ShipVisual->SetMaterial(0, (UMaterialInstanceDynamic*)ShipMaterial);
	}

	static ConstructorHelpers::FObjectFinder<UBlueprint> Bullet(TEXT("Blueprint'/Game/Blueprints/BulletBP.BulletBP'"));
	if (Bullet.Object) {
		ProjectileClass = (UClass*)Bullet.Object->GeneratedClass;
	}
}

// Called when the game starts or when spawned
void AShip::BeginPlay()
{
	Super::BeginPlay();
	
}

// Called every frame
void AShip::Tick( float DeltaTime )
{
	Super::Tick( DeltaTime );

}

// Called to bind functionality to input
void AShip::SetupPlayerInputComponent(class UInputComponent* InputComponent)
{
	Super::SetupPlayerInputComponent(InputComponent);
	InputComponent->BindAxis("MoveForward", this, &AShip::Move_Forward);
	InputComponent->BindAxis("Turn", this, &AShip::Move_Turn);
	InputComponent->BindAction("Shoot", EInputEvent::IE_Pressed, this, &AShip::Shoot);
}

void AShip::Move_Forward(float AxisValue)
{
	if (AxisValue > 0)
	{
		ShipSphereComponent->AddForce(GetActorForwardVector() * AxisValue * 600);
	}
}

void AShip::Move_Turn(float AxisValue)
{
	FRotator NewRotation = GetActorRotation();
	NewRotation.Yaw += AxisValue * 4;
	SetActorRotation(NewRotation);
}

void AShip::Shoot() {
	UWorld* const World = GetWorld();
	if (World) {
		FActorSpawnParameters SpawnParams;
		SpawnParams.Owner = this;
		SpawnParams.Instigator = Instigator;
		World->SpawnActor<ABullet>(ProjectileClass, GetActorLocation() + GetActorForwardVector() * 30, GetActorRotation(), SpawnParams);
	}
}