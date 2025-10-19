## Mission Demolition – Enhancements Summary

This project expands upon the Mission Demolition tutorial from Jeremy Gibson Bond, Introduction to Game Design, Prototyping, and Development.
The base gameplay remains faithful to the tutorial—players launch projectiles with a slingshot to knock down castles—but now includes additional features that enhance replayability, usability, and scoring feedback.

## Meaningful Enhancements
#### Score Tracking System

Added a complete scoring framework that tracks both per-level shots and a cumulative total shot count across all levels.

Introduced a “Lowest Score” system (lower is better) that persists between play sessions using PlayerPrefs.

Displays the player’s record both on the HUD during gameplay and on the Game Over panel after the final level.

Encourages replay and mastery—players can attempt to complete the campaign using fewer shots.

### Level Restart Functionality

Added a Restart Level button allowing players to immediately retry a castle without restarting the full game.

Resets projectiles, castles, and per-level shot count while preserving the overall run’s statistics.

Supports the game’s “one-try” philosophy: while each level is designed to be beatable in a single attempt, this gives players the flexibility to practice and refine their aim before committing to a full run.

## Why These Changes Matter

These improvements make the game more engaging and replayable without altering the core mechanics.
The score tracker provides long-term motivation by rewarding precision and efficiency, while the level restarter improves accessibility and experimentation for new players. Together, they create a smoother, more satisfying gameplay loop that feels modern yet consistent with the original design.
