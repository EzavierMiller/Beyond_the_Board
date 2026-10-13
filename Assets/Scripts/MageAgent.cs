using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple-reflex Mage behavior for the 8x8 Beyond the Board test board.
/// Attach this to the same GameObject as its BoardPiece component.
/// </summary>
[DisallowMultipleComponent]
public sealed class MageAgent : MonoBehaviour
{
    public enum TargetRole
    {
        Other = 0,
        Warrior = 1,
        Tank = 2,
        Ranger = 3,
        Healer = 4
    }

    [Serializable]
    public sealed class EnemyTarget
    {
        public BoardPiece piece;
        public TargetRole role = TargetRole.Other;
    }

    [Header("References")]
    [SerializeField] private BoardPiece mage;
    [SerializeField] private List<EnemyTarget> enemies = new List<EnemyTarget>();

    [Header("Mage rules")]
    [SerializeField, Min(1)] private int spellRange = 4;
    [SerializeField, Min(0)] private int dangerRange = 1;
    [SerializeField, Min(1)] private int spellDamage = 3;
    [SerializeField, Min(0.05f)] private float decisionInterval = 1f;
    [SerializeField] private bool autoAct = true;

    [Header("Board coordinates")]
    [SerializeField] private int minCoordinate = 1;
    [SerializeField] private int maxCoordinate = 8;

    private float nextActTime;

    private static readonly Vector2Int[] MoveDirections =
    {
        new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1),
        new Vector2Int(0, -1),                         new Vector2Int(0, 1),
        new Vector2Int(1, -1),  new Vector2Int(1, 0),  new Vector2Int(1, 1)
    };

    private void Awake()
    {
        if (mage == null)
        {
            mage = GetComponent<BoardPiece>();
        }
    }

    private void Start()
    {
        nextActTime = Time.time;
    }

    private void Update()
    {
        if (!autoAct || Time.time < nextActTime)
        {
            return;
        }

        nextActTime = Time.time + Mathf.Max(0.05f, decisionInterval);
        TakeTurn();
    }

    /// <summary>Runs one decision, also usable by a future turn manager or UnityEvent.</summary>
    public void TakeTurn()
    {
        if (mage == null)
        {
            Debug.LogError("MageAgent needs a BoardPiece on the same GameObject or in its Mage field.", this);
            return;
        }

        List<EnemyTarget> livingEnemies = GetLivingEnemies();
        if (livingEnemies.Count == 0)
        {
            return;
        }

        EnemyTarget target = SelectTarget(livingEnemies);
        if (target == null)
        {
            return;
        }

        bool threatTooClose = HasEnemyWithinDangerRange(livingEnemies);
        if (threatTooClose)
        {
            if (TryMove(target, livingEnemies, true))
            {
                return;
            }

            Debug.Log("Mage cannot retreat: every adjacent tile is blocked or off the board.", this);
        }

        if (IsInSpellRange(target.piece))
        {
            CastSpell(target.piece);
            return;
        }

        if (!TryMove(target, livingEnemies, false))
        {
            Debug.Log("Mage waits: no legal adjacent tile improves its position.", this);
        }
    }

    private List<EnemyTarget> GetLivingEnemies()
    {
        List<EnemyTarget> living = new List<EnemyTarget>();
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyTarget entry = enemies[i];
            if (entry != null && entry.piece != null && entry.piece.gameObject.activeInHierarchy && entry.piece.health > 0)
            {
                living.Add(entry);
            }
        }
        return living;
    }

    private EnemyTarget SelectTarget(List<EnemyTarget> candidates)
    {
        EnemyTarget best = null;
        for (int i = 0; i < candidates.Count; i++)
        {
            EnemyTarget candidate = candidates[i];
            if (best == null || HasHigherTargetPriority(candidate, best))
            {
                best = candidate;
            }
        }
        return best;
    }

    private bool HasHigherTargetPriority(EnemyTarget candidate, EnemyTarget current)
    {
        int candidatePriority = GetRolePriority(candidate.role);
        int currentPriority = GetRolePriority(current.role);
        if (candidatePriority != currentPriority)
        {
            return candidatePriority > currentPriority;
        }

        if (candidate.piece.health != current.piece.health)
        {
            return candidate.piece.health < current.piece.health;
        }

        return GridDistance(mage, candidate.piece) < GridDistance(mage, current.piece);
    }

    private int GetRolePriority(TargetRole role)
    {
        switch (role)
        {
            case TargetRole.Healer: return 4;
            case TargetRole.Ranger: return 3;
            case TargetRole.Warrior: return 2;
            case TargetRole.Tank: return 1;
            default: return 0;
        }
    }

    private bool HasEnemyWithinDangerRange(List<EnemyTarget> livingEnemies)
    {
        for (int i = 0; i < livingEnemies.Count; i++)
        {
            if (GridDistance(mage, livingEnemies[i].piece) <= dangerRange)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsInSpellRange(BoardPiece target)
    {
        // The current design has no terrain blockers, so every active enemy has line of sight.
        return HasLineOfSight(target) && GridDistance(mage, target) <= spellRange;
    }

    private bool HasLineOfSight(BoardPiece target)
    {
        // Replace this check with a board-ray/terrain query if blocking terrain is added later.
        return target != null && target.gameObject.activeInHierarchy;
    }

    private void CastSpell(BoardPiece target)
    {
        target.health = Mathf.Max(0, target.health - spellDamage);
        Debug.Log("Mage casts on " + target.name + "; target health is now " + target.health + ".", this);

        if (target.health == 0)
        {
            target.gameObject.SetActive(false);
            Debug.Log("Mage defeated " + target.name + ".", this);
        }
    }

    private bool TryMove(EnemyTarget preferredTarget, List<EnemyTarget> livingEnemies, bool retreat)
    {
        BoardPiece[] boardPieces = FindObjectsByType<BoardPiece>(FindObjectsSortMode.None);
        bool foundMove = false;
        Vector2Int bestPosition = new Vector2Int(mage.GetX(), mage.GetY());
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < MoveDirections.Length; i++)
        {
            int x = mage.GetX() + MoveDirections[i].x;
            int y = mage.GetY() + MoveDirections[i].y;
            if (x < minCoordinate || x > maxCoordinate || y < minCoordinate || y > maxCoordinate)
            {
                continue;
            }

            if (IsOccupied(x, y, boardPieces))
            {
                continue;
            }

            int targetDistance = GridDistance(x, y, preferredTarget.piece.GetX(), preferredTarget.piece.GetY());
            int nearestEnemyDistance = GetNearestEnemyDistance(x, y, livingEnemies);
            float score;

            if (retreat)
            {
                // Escape first; among equally safe tiles, prefer one closer to the chosen target.
                score = nearestEnemyDistance * 1000f - targetDistance * 10f;
            }
            else
            {
                // Reach spell range quickly, but avoid ending adjacent to a configured enemy.
                score = (targetDistance <= spellRange ? 500f : 0f) - targetDistance * 100f + nearestEnemyDistance;
                if (nearestEnemyDistance <= dangerRange)
                {
                    score -= 1000f;
                }
            }

            if (!foundMove || score > bestScore)
            {
                foundMove = true;
                bestScore = score;
                bestPosition = new Vector2Int(x, y);
            }
        }

        if (!foundMove)
        {
            return false;
        }

        mage.Move(bestPosition.x, bestPosition.y);
        Debug.Log("Mage " + (retreat ? "retreats" : "repositions") + " to (" + bestPosition.x + ", " + bestPosition.y + ").", this);
        return true;
    }

    private bool IsOccupied(int x, int y, BoardPiece[] boardPieces)
    {
        for (int i = 0; i < boardPieces.Length; i++)
        {
            BoardPiece piece = boardPieces[i];
            if (piece != null && piece != mage && piece.gameObject.activeInHierarchy && piece.health > 0 && piece.GetX() == x && piece.GetY() == y)
            {
                return true;
            }
        }
        return false;
    }

    private int GetNearestEnemyDistance(int x, int y, List<EnemyTarget> livingEnemies)
    {
        int nearest = int.MaxValue;
        for (int i = 0; i < livingEnemies.Count; i++)
        {
            BoardPiece enemy = livingEnemies[i].piece;
            nearest = Mathf.Min(nearest, GridDistance(x, y, enemy.GetX(), enemy.GetY()));
        }
        return nearest;
    }

    private int GridDistance(BoardPiece a, BoardPiece b)
    {
        return GridDistance(a.GetX(), a.GetY(), b.GetX(), b.GetY());
    }

    private int GridDistance(int x1, int y1, int x2, int y2)
    {
        // Chebyshev distance matches the board's eight-direction, one-tile movement.
        return Mathf.Max(Mathf.Abs(x1 - x2), Mathf.Abs(y1 - y2));
    }

    private void OnValidate()
    {
        minCoordinate = Mathf.Min(minCoordinate, maxCoordinate);
        maxCoordinate = Mathf.Max(minCoordinate, maxCoordinate);
        spellRange = Mathf.Max(1, spellRange);
        dangerRange = Mathf.Max(0, dangerRange);
        spellDamage = Mathf.Max(1, spellDamage);
        decisionInterval = Mathf.Max(0.05f, decisionInterval);
    }
}
