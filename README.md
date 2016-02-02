# cis568
CIS 568 - Game Design Practicum

Sacha Best

### QWOP Skateboarding

QWOP Skateboarding is a physically semi-accurate skateboarding simulator built in Unity. The games use player's jumps and rotations to score points over a timed interval. Some features include

* Fully controllable ground based movement using physically accurate forces
* Aerial rotations to score 180s and 360s
* Ability to ride "switch"
* "Pop" feature to exert upward momentum prior to jumps
* Music from Tony Hawk's Pro Skater 2: Guerilla Radio - Rage Against the Machine (copyrights reserved to the artist)
* Sound effects variable to movement (speed controls volume)
* VTOL from upward ramps

#### Controls

Ground Controls:

* w: pedal
* a: lean left
* d: lean right
* spacebar: pop

Aerial Controls:

* i: lean forward
* k: lean backward
* j: spin counterclockwise
* k: spin clockwise

Experimental:

* shift+w: gravity upwards
* shift+d: gravity downwards
* shift+a: gravity left
* shift+d: gravity right

#### Implementation Details

Scene Layout: there are two major scenes (Main.unity and Menu.unity).

* Menu.unity: Just the basic start menu with instructions and one button
* Main.unity: The game and endgame states

Functional Organization:

* Player: parent GameObject for the User's avatar. This object contains many components:
	* Capsule Collider + Rigidbody: Used to simulate physics and movement while on the skateboard
	* Animator: used as a finite state machine to control animation states
	* Fixed Joint: connects the body to the skateboard
	* Avatar: see script documentation below
* EthanBody, EthanGlasses, EthanSkeleton: Unity player character body parts and ragdoll data
* Skateboard: controller for Skateboard-centric physics with many components:
	* Mesh Renderer: draws the shape
	* Box Collider + Rigidbody: Used to simulate physics on the skateboard
	* Audio Source: plays audio for landing and takeoff
	* SkatebaordPhysicsManager: see script documentation below
	* Trucks: parent object that holds 4 wheels, each with its own SphereCollider for ground contact
* Main Camera: the only camera used
* PermanentData: information that persists between deaths/falls:
	* HUD: the in-game heads up display with score and time
	* GameOverHUD: the end-game HUD
	* DeadUI: message shown upon falls
* dead: a trigger attached to a ground plane that says "GAME OVER" as a joke - but causes death on contact

Programmatic Organization:

* Avatar.cs: coordinates and manages all aspects of the player including vital state (alive/dead), gravity, etc.
* SkateboardPhysicsManager.cs: handles all movement based physics of the skateboard and translates that data to the player character
* GameManager.cs: handles game state, music, and transitions between scenes
* SmoothFollow.cs: a heavily modified camera follow script that ignores certain movement based on player state
* PermanentData.cs: lets data persist through scene changes without duplication

#### Challenges, Accomplishments, and Pitfalls

Procedure:

In general, making a physically accurate skateboarding game was extremely difficult. Attempting to handle the fully dynamical system all at once and attempting to match a physically accurate model as such proved to be impossible. So, in order of attempt, here is how I approached the process. 

1. WheelColliders and Rigidbodies: Unity provides wheel colliders normally used to control cars that can be used to simulate skateboard wheels. The problem here was the friction, drift, and skid of the wheels. There was no apparently feasible way to eliminate the "fishtail" effect seen in cars without making a fully accurate skateboard rig with a spring damper system to model trucks. This wasn't feasible

2. CharacterController: Unity provides a class for controller third person characters, however it is not remotely physically accurate. Thus this controller didn't fit the mold appropriately.

3. CapsuleColliders and Rigidbodies: Instead of using WHeelCollider, I decided to use CapsuleColliders to model wheels and continued with the standard rigidbody mechanics. 

Pitfalls and Solutions:

* Maintaining orientation perpendicular to normals: the skateboarder needs to stay normal to the surface he is riding on - this becomes difficult as the rigidbody displacement of the character is not reflected in the character's transform nor any parent transform. Solution: work entirely in rigidbody physics "land" - disregarding the actual object transforms. 

* Maintaining balance: Placing a skateboarder on the board leads to major balance problems when turning. Since there is no spring damper system on the trucks of the skateboard, there is no way to redistribute the mass of the rider at an angle as you would on a real physical skateboard. The solution was to model the character as a near massless object bound to the skateboard. 

* Landings: Landing the skateboard flat is physically impossible with the constraints mentioned above. There are simply too many exterior forces and user error at play. Quite simply put, it is very hard as a user to land perfectly flat. So, I needed to come up with an appropriate tolerance on landings. To accomplish this, there was another boundary to overcome. If we simply "snap" the board into place on landing into the appropriate position, the skateboard-player joint breaks due to excessive force. So, the final solution was to linearly interpolate between a start and destination orientation for transform.forward. 

* Breaking: Landings can be rough. So, I needed a way to gauge when to break the landing. This was a case of trial and error with mapping the force and torque limits on the joint. 

* Ragdoll: We cannot have both the ragdoll and animation systems working at once. So, the game needs to be aware of when to use one system over the other. Thus, the ragdoll system is disabled until one of two conditions are met: either the player collider hits the ground, or the joint between person and skateboard breaks. 

* Turning on the ground: Since the rigidbody displacement of the skateboard doesn't match the object's transform parameters, it is very tricky to properly steer the skateboard. The solution was to break the player transform.rotation into a quaternion and apply certain aspects of the quaternion to the player's velocity (more specifically angular velocity). 

* Riding switch: On a normal skateboard, the rider can turn 180 degrees to ride "switch" or opposite foot forward. This was difficult to bring into the game due to the discrepancy between the transform and rigidbody displacement. The solution was to toggle between the forward and backward vectors of the player transform dependent on the displacement of the forward velocity from the true forward vector. 

#### Unaddressed Questions

These are problems and bugs with the game I didn't get to fixing in the given amount of time. I'd love to go back and address them at some point in the future - each is a complex problem that will involve a creative solution. 

* VTOL orientation: vertical takeoff and landing off of a quarterpipe is a tricky problem - we need to adapt the player's rotation keys to turn the player about a different axis than if the player was horizontal. 

* Better landings: the landings are rough right now. I'd love to dig into the math that is involved with interpolating between the orientation vector, velocity vector, and the ground plane normal.
