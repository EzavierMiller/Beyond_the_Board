# Julian's Mage Agent

## Design

- Owner: Julian Williams
- Role: Mage, a ranged magical-combat piece.
- Agent category: deterministic simple-reflex agent with priority-based target selection.
- Goal: keep away from adjacent enemies, attack from spell range, and favor high-threat targets.
- Performance checks: move to a legal empty tile when out of range; retreat when an enemy enters the danger radius; cast only within range; choose Healer before Ranger, then the weakest and nearest target within a role; never move outside the board or onto an occupied tile.
- Inputs: configured enemy BoardPieces and roles, their health and coordinates, the Mage's coordinates, and board occupancy.
- Outputs: one-tile movement, a spell that reduces enemy health, or no action.
- Environment: 8x8 grid with coordinates 1 through 8. Movement and range use Chebyshev distance (eight neighboring directions). The current test board has no terrain blockers, so active targets have line of sight.

## Decision rule

1. If no configured enemy is alive, wait.
2. If any configured enemy is within the danger radius, move to the best empty adjacent tile that increases distance from enemies.
3. Otherwise select the highest-priority role: Healer, Ranger, Warrior, Tank, then Other. Break ties by lower current health, then shorter distance.
4. If the chosen target is in spell range, deal spell damage; deactivate it when health reaches zero.
5. If it is out of range, move one empty tile toward it while avoiding danger. If no legal move improves the position, wait.

## Test it in Unity

Open `Assets/Test.unity` on the `Mage_AI` branch and press Play. The scene's Mage acts automatically every second. The console reports movement and casts. Default values are spell range 4, danger radius 1, and spell damage 3; change them on MageAgent in the Inspector.

To test target priority or retreat, add another BoardPiece to the scene, include it in MageAgent's Enemies list, and assign its role. Put an enemy within one tile of the Mage to see retreat behavior. Call the public `TakeTurn` method from a turn manager or UnityEvent to disable automatic timing and integrate turn-by-turn play.

When reusing the component, attach `MageAgent` to the same GameObject as the Mage's `BoardPiece`, list each enemy in the Inspector, set its role, and set the Mage BoardPiece control to `None` if it should not also respond to WASD. The BoardPiece base class owns health and grid coordinates; the agent uses those existing fields.
