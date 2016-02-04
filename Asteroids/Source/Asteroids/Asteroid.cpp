// Fill out your copyright notice in the Description page of Project Settings.

#include "Asteroids.h"
#include "Asteroid.h"


// Sets default values
AAsteroid::AAsteroid()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;
	// Our root component will be a box that reacts to physics
	AsteroidBoxComponent = CreateDefaultSubobject<UBoxComponent>(TEXT("RootComponent"));
	RootComponent = AsteroidBoxComponent;

	AsteroidBoxComponent->InitBoxExtent(FVector(12.0f, 15.0f, 12.0f));
	AsteroidBoxComponent->SetCollisionProfileName(TEXT("BlockAllDynamic"));
	AsteroidBoxComponent->SetEnableGravity(false);
	AsteroidBoxComponent->SetNotifyRigidBodyCollision(true);
	OnActorHit.AddDynamic(this, &AAsteroid::onHit);
	static ConstructorHelpers::FObjectFinder<UMaterial> Material(TEXT("Material'/Game/asteroid_mat.asteroid_mat'"));
	if (Material.Object != NULL)
	{
		AsteroidMaterial = (UMaterial*)Material.Object;
	}
	UStaticMeshComponent* BoxVisual = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("VisualRepresentation"));
	BoxVisual->AttachTo(RootComponent);
	static ConstructorHelpers::FObjectFinder<UStaticMesh> BoxVisualAsset(TEXT("StaticMesh'/Game/prop_asteroid_01.prop_asteroid_01'"));
	if (BoxVisualAsset.Succeeded())
	{
		BoxVisual->SetStaticMesh(BoxVisualAsset.Object);
		BoxVisual->SetRelativeLocation(FVector(0.0f, 0.0f, 0.0f));
		BoxVisual->SetWorldScale3D(FVector(0.25f));
		BoxVisual->SetMaterial(0, (UMaterialInstanceDynamic*)AsteroidMaterial);
		BoxVisual->SetNotifyRigidBodyCollision(true);
	}

	static ConstructorHelpers::FObjectFinder<UBlueprint> FindExplosion(TEXT("Blueprint'/Game/Blueprints/ExplosionBP.ExplosionBP'"));
	if (FindExplosion.Object) {
		Explosion = (UClass*)FindExplosion.Object->GeneratedClass;
	}
	static ConstructorHelpers::FObjectFinder<USoundCue> explosionSound(TEXT("SoundCue'/Game/ExplosionCue.ExplosionCue'"));
	if (explosionSound.Object != NULL)
	{
		explosionSoundCue = (USoundCue*)explosionSound.Object;
	}
}

// Called when the game starts or when spawned
void AAsteroid::BeginPlay()
{
	Super::BeginPlay();
	
}

// Called every frame
void AAsteroid::Tick( float DeltaTime )
{
	Super::Tick( DeltaTime );

}

void AAsteroid::onHit(AActor* SelfActor, class AActor* OtherActor, FVector NormalImpulse, const FHitResult& Hit) {
	if (OtherActor && (OtherActor != this) && OtherActor->IsA(ABullet::StaticClass()))
	{
		Destroy();
		OtherActor->Destroy();
		UWorld* const World = GetWorld();
		if (World) {
			FActorSpawnParameters SpawnParams;
			SpawnParams.Owner = this;
			SpawnParams.Instigator = Instigator;
			World->SpawnActor<AActor>(Explosion, GetActorLocation(), GetActorRotation(), SpawnParams);
			UGameplayStatics::PlaySoundAtLocation(World, explosionSoundCue, GetActorLocation());
		}
	}
}

