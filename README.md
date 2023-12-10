# Multiplayer FPS Project
## A titanfall/black ops 3-4 inspired project.

An FPS Project with a high-speed movement controller.

This project is intented to be a testing ground at the least, or a full-fledged game ideally, that showcases my talent and dedication.
Feel free to download the project to import into Unity, or to download a build when one exists to try it out.

## Current plans/roadmap
### Networking/Gaming Services
* Random matchmaking - SBMM is not something I want to try to implement, as it can often deteriorate the experience for newer or casual gamers.
* Cosmetics - Every gamer loves cosmetics, right? Even better is _free_ cosmetics. I don't think that style should be paywalled, and should instead be an indicator of your dedication.
### Game Mechanics
* Fast, responsive movement - This is already in the works. The movement needs tweaking in order to give the right feel, but its mostly there. Movement options include sprinting, sliding, wallrunning/wallriding, and aerial movement.
  * All movement settings are tweakable, and will likely be able to be tweaked for any custom game implementation in the future.
* Hitscan and projectile weapons - Just like in Titanfall, I want there to be some projectile weapons and some hitscan/raycast weapons. This allows more skillful play for those who want the challenge, as well as some weapons (such as grenade launchers) simply making sense this way. A hitscan rocket launcher? no thanks..
* Single-player Campaign mode - All the games I've enjoyed the most have a campaign or story mode; one that doesn't force an internet connection to play it, either. I've always wanted to take a crack at writing and creating a campaign mode with unique challenges and enemies, and epic boss fights (because who doesn't love a boss fight?)
* Dynamic weapon balancing - In multiplayer games, the biggest question always seems to be "What's the meta?" - A term that I hate. I don't think there should be a clear "best gun to use" that grants you some crazy advantage over others that isn't derived purely from skill. My aim with weapon balancing is to keep tweaking the game, should it be released publicly, to provide the best and most level experience for all players.

## Current Features
* Rigidbody movement controller - the controller uses _almost exclusively_ forces to move the player, occasionally adding a velocity in certain situations, or removing the y-axis velocity of the players.
