# Julian contribution — implementation and verification

## Scope delivered
Julian's assigned functions are PieceMoraleAgent.ChooseAction(), PieceMoraleAgent.OnPieceDestroy(), and GameManager.TryManicMove(), with the supporting interfaces and scene wiring needed to exercise them. Sections 7 and 9 govern this implementation. The faction hierarchy, squad coordination, pressure metrics, and probabilistic hesitation/warning targets are not claimed complete.

## Git baseline
- Original pushed commit: 0da8825b58336c456dfbcfedd79c70c3e878cf83.
- Local merge of main into Mage_AI: f0c6677.
- The earlier remote check confirmed the original commit on GitHub main.
- Finishing changes are a separate focused local contribution. No force-push or overwrite of teammate history is required.
- Pre-merge backup and VCS-settings stash are retained. Unrelated packages, cloud project identity, documents, AI Toolkit files, and plan state must remain outside the contribution commit.

## Completed behavior
- Role decisions occur once when granted by the manager; the Mage's duplicate action branch is removed. Managed Mage timer actions cannot bypass dispatch.
- Capture processing retains one persistent board, clears occupancy, removes captured agents before notification, retains capture metadata after destruction, and guards against duplicate capture callbacks.
- Surviving player agents update morale locally with 40/50 hysteresis and configurable per-captured-type reaction overrides. Recovery clears refusal; refusing consumes the piece's action.
- Rogue agents choose their own action/destination. The manager validates manic bounds, adjacency, live membership, occupancy, and reservation conflicts; successful moves update board and one-based piece coordinates together.
- Rogue phase uses a snapshot, persistent piece-ID tie breaking, and one defection per phase with local manic fallback for further candidates. Side and controller change together.
- Healer, Tank, and Ranger integration includes the required live-target/null guards and turn validation. Managed healing consumes the action budget.
- Main scene has six referenced pieces and six morale components, unique serialized IDs, and valid starting coordinates. Mage starts at (1,1), Healer at (4,1), matching the previous clamping behavior without overlapping other units.
- Test scene has two referenced pieces and two morale components, a player-controlled Mage, a wired Mage order click, and an End Turn button. Existing board art, layout, scales, and colliders were preserved.

## Controls
- Main: WASD or arrow keys to move the player; E or the existing Advance/End Turn button to finish the round.
- E is separate from UI Submit to prevent a focused button and a keyboard action advancing two rounds.
- Test: click the Mage to request one role decision; click End Turn for enemy and rogue phases.

## Runtime evidence
### Main and mechanics: 18 assertions passed
- One manager; morale components on all six pieces.
- All six pieces register without overlap; IDs are unique; board queries retain the same instance.
- WASD produces one legal move; a second input cannot move that piece again in the round.
- End Turn input advances exactly one round and returns to Player.
- Controlled ally-capture event changes morale 45 to 37 and enters Rogue.
- Rogue decision sets refusal; the next command clears refusal and consumes the action.
- Enemy capture is recorded once, removes the victim, and recovers morale 37 to 52.
- Hysteresis retains Rogue at 44 and recovers at exactly 50.
- Reserved/missing manic destinations do not move a piece; a valid locally selected destination updates coordinates.
- Only the lowest-morale eligible piece defects; the next candidate uses manic fallback.
- Player commands are rejected for a defected unit.

The combined two-scene harness returned a 15-second deadline error after completing all 18 Main assertions while transitioning scenes. This was not accepted as a full-suite pass; the Test scene was then verified in its own run rather than increasing the deadline.

### Independent Test scene: 10 assertions passed, overall success=true
- One manager and two morale agents.
- Both pieces registered, without overlap and with unique IDs.
- Mage remains at its authored position before a command.
- Mage button points to IssuePlayerOrder; its existing graphic is nonzero and raycastable.
- One Mage click performs one move; a repeated order in the same round is rejected.
- End Turn callback is wired; enemy and rogue phases return to Player.
- The player's action budget resets on the next round.

### Earlier isolated regression evidence
13 isolated core checks also passed before scene wiring, including missing/off-board/non-adjacent/reserved manic destinations, occupied-tile contention, captures, refusal/recovery, and defection limits.

## Final checks and limits
- Each changed C# file was checked by the real Unity compiler. Final compilation status has no errors.
- Temporary verification scripts were removed; editor is out of Play Mode and Test.unity is restored.
- Runtime checks exercised actual input events and persistent button callbacks. Physical mouse hit testing, exhaustive keyboard/device combinations, full campaign balancing, and long-duration performance were not exhaustively tested.
- Existing Warrior obsolete object-search warnings are nonfatal and outside this targeted contribution.
- The simple agents/scenes still represent a small test board, not the full 16-piece faction/squad system. Do not describe the entire team design or every statistical target as 100% verified.
