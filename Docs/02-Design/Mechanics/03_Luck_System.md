# Core Mechanic: Luck System

This document details the design for the Luck System, which introduces unpredictability and fate into the game, forcing players to adapt to changing fortunes.

---

## 1. Macro-Level: The Fortune Cycle

-   **Concept**: The player's general "luck" is not static but ebbs and flows like a tide.
-   **Mechanics**:
    -   A UI element (e.g., a "Fortune Wheel" or "Weather Vane") displays the current luck state.
    -   The state changes periodically (e.g., every 20-30 turns) between several levels:
        -   **Great Fortune**: Increased chance of positive block spawns ("Opportunity," "Inspiration"). Small chance for merge results to be "blessed" (level up an extra time).
        -   **Good Fortune**: Minor positive bonuses.
        -   **Neutral**: Standard gameplay probabilities.
        -   **Misfortune**: Increased chance of negative block spawns ("Illness," "Accident," "Bills"). Small chance for actions to "fumble" (e.g., a block moves to the wrong tile).
-   **Player Interaction**: While the cycle is largely out of the player's control, certain actions or special "Charm" blocks can slightly increase the duration of positive cycles or shorten negative ones.

## 2. Micro-Level: Serendipity and Events

-   **Concept**: Luck manifests as specific, tangible events on the grid.
-   **Mechanics**:
    -   **"Lucky Star" Block**: A rare, temporary block that appears on the grid. While present, it provides a powerful positive aura to adjacent tiles, increasing merge rewards or reducing costs in that area.
    -   **"Ominous Cloud" Block**: The negative counterpart to the Lucky Star. It creates a "cursed" area where actions are more likely to fail or have negative side effects.
    -   **Critical Success/Failure**: Every merge action has a very small, fixed probability (e.g., 1%) of resulting in a "critical success" (jackpot reward) or a "critical failure" (blocks are destroyed, leaving a negative "Regret" block). This represents pure, unmodifiable chance.

## 3. Player Agency: Interacting with Luck

-   **Concept**: Players should feel they can influence their luck, even if it's just a perception.
-   **Mechanics**:
    -   **"Ritual" Actions**: Performing specific, non-obvious sequences of actions (e.g., merging a "Hobby," then "Family," then "Work" block at the start of a "week") can provide a small, temporary boost to the player's luck factor. These are hidden mechanics for players to discover.
    -   **"Lucky Charm" Blocks**: Players can acquire special blocks (e.g., "Four-Leaf Clover," "Family Photo") that occupy a grid space but provide a permanent, passive bonus to the luck calculation, making positive outcomes slightly more likely.
