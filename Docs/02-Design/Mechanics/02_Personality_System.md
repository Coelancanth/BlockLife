# Core Mechanic: Personality System (MBTI-like)

This document details the design for a dynamic personality system where a player's traits are developed through their actions, rather than chosen at the start.

---

## 1. Core Philosophy

-   **Personality is Developed, Not Chosen**: Players do not select a personality type. Their in-game actions and choices continuously shape their personality along four distinct axes.
-   **Behavior Shapes Identity Shapes Abilities**: A feedback loop is created where player behavior shapes their personality, which in turn unlocks unique abilities that reinforce or enable new behaviors.

## 2. The Four Axes

The system tracks personality on four sliding scales, inspired by the MBTI framework.

### a. Energy Source (Introversion vs. Extroversion)

-   **Shift to Introversion (I)**: Merging "Hobby," "Reading," "Solo" blocks. Spending turns without social interaction.
-   **Shift to Extroversion (E)**: Merging "Social," "Friends," "Party" blocks.
-   **Gameplay Effect**: Determines how the player recharges their "Mindset" or "Energy" resource—either through solitude or social activity.

### b. Information Perception (Sensing vs. Intuition)

-   **Shift to Sensing (S)**: Focusing on concrete, immediate actions like merging "Money" or "Job" blocks. Preferring simple, guaranteed merges over complex ones.
-   **Shift to Intuition (N)**: Planning multi-step, complex merges. Merging abstract blocks like "Ideas," "Theories," or "Art."
-   **Gameplay Effect**: S-types might get bonuses on resource generation per turn, while N-types might get larger rewards for complex combo merges.

### c. Decision Making (Thinking vs. Feeling)

-   **Shift to Thinking (T)**: Making choices that maximize resources, even at a social or emotional cost. Prioritizing "Career" over "Family."
-   **Shift to Feeling (F)**: Making choices that prioritize relationships and emotional harmony, even if less optimal from a resource perspective.
-   **Gameplay Effect**: T-types may have faster "Attribute" growth. F-types may have a higher "Mindset" cap and better resilience to stress.

### d. Lifestyle Approach (Judging vs. Perceiving)

-   **Shift to Judging (J)**: Keeping the grid organized. Planning moves far in advance. Using "storage" or "planning" skills.
-   **Shift to Perceiving (P)**: Making spontaneous moves. Reacting to opportunities as they arise. Having a chaotic or disorganized grid.
-   **Gameplay Effect**: J-types might receive bonuses for having many empty, organized spaces on their grid. P-types might receive rewards for adapting to sudden, unexpected events.

## 3. Implementation Loop

1.  **Action Tracking**: Every significant player action (merging a certain block type, making a key decision) sends a small nudge along one of the four axes.
2.  **Stage-End Assessment**: At the end of each major life stage (Childhood, Adolescence, Young Adulthood), the system assesses the player's current position on the four axes and generates a "Personality Report" (e.g., "In your youth, you were a curious and adaptable ENFP").
3.  **Trait-Based Unlocks**: This report is not just flavor text. Based on the resulting personality type, the player unlocks a unique passive ability or active skill for the next life stage.
    -   *Example (INTJ)*: Unlock "Foresight," an ability to see the next 3-5 blocks that will spawn.
    -   *Example (ESFP)*: Unlock "Improvise," an ability to turn a negative "Stress" block into a positive "Party" block.
4.  **Personality Solidification**: As the character ages, the axes become harder to move, representing the solidification of personality over a lifetime. A "Midlife Crisis" event might provide a rare, costly opportunity to dramatically shift one of the axes.
