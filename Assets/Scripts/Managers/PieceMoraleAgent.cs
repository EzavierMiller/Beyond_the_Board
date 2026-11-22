using System;
using UnityEngine;

/// <summary>
/// Morale and simple action delegation for a piece. Attach this to the same
/// GameObject as its BoardPiece component.
/// </summary>
[DisallowMultipleComponent]
public class PieceMoraleAgent : MonoBehaviour
{
    [Header("Morale")]
    [SerializeField, Range(0, 100)] private int morale = 100;
    [SerializeField, Range(0, 100)] private int rogueThreshold = 40;
    [SerializeField, Range(0, 100)] private int recoverThreshold = 50;
    [SerializeField, Range(0, 100)] private int defectThreshold = 10;
    [SerializeField, Range(0, 100)] private int manicThreshold = 25;
    [SerializeField, Min(0)] private int moraleLostWhenAllyIsCaptured = 10;
    [SerializeField, Min(0)] private int moraleGainedWhenEnemyIsCaptured = 5;

    private BoardPiece piece;
    private bool refuseNextCommand;

    public int Morale => morale;
    public bool IsRogue => morale < rogueThreshold;
    public bool RefuseNextCommand => refuseNextCommand;

    private void Awake()
    {
        piece = GetComponent<BoardPiece>();
        if (piece == null)
        {
            Debug.LogError("PieceMoraleAgent requires a BoardPiece on the same GameObject.", this);
        }
    }

    // Morale changes are broadcast when a piece is captured/destroyed.
    private void OnEnable()
    {
        PieceDestroyed += HandlePieceDestroyed;
    }

    private void OnDisable()
    {
        PieceDestroyed -= HandlePieceDestroyed;
    }

    private static event Action<Team> PieceDestroyed;

    /// <summary>
    /// Returns a random legal adjacent tile for a manic move, or Wait if none is free.
    /// BoardPos coordinates are zero-based; BoardPiece.Move uses one-based coordinates.
    /// </summary>
    public RequestAction FallbackManic(Board board, BoardPos[] reservedTiles)
    {
        if (piece == null || board == null)
        {
            return new RequestAction(RequestActionType.Wait);
        }

        var options = new System.Collections.Generic.List<BoardPos>();
        int currentX = piece.GetX() - 1;
        int currentY = piece.GetY() - 1;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                var destination = new BoardPos(currentX + dx, currentY + dy, Team.None);
                if (board.IsLegal(destination) && !IsReserved(destination, reservedTiles))
                {
                    options.Add(destination);
                }
            }
        }

        if (options.Count == 0)
        {
            return new RequestAction(RequestActionType.Wait);
        }

        BoardPos selected = options[UnityEngine.Random.Range(0, options.Count)];
        return new RequestAction(RequestActionType.Manic, selected);
    }

    /// <summary>Chooses a rogue action from the current morale thresholds.</summary>
    public RequestAction DecideRogueAction(Board board, BoardPos[] reservedTiles)
    {
        if (morale >= rogueThreshold)
        {
            return new RequestAction(RequestActionType.None);
        }

        if (morale < defectThreshold)
        {
            return new RequestAction(RequestActionType.Defect);
        }

        if (morale < manicThreshold)
        {
            return FallbackManic(board, reservedTiles);
        }

        return new RequestAction(RequestActionType.Refuse);
    }

    /// <summary>
    /// Runs the piece's existing single-piece AI once. Call this only when the
    /// turn manager has chosen this piece to act.
    /// </summary>
    public void ChooseAction()
    {
        if (piece == null || !piece.gameObject.activeInHierarchy || piece.health <= 0)
        {
            return;
        }

        if (IsRogue)
        {
            Debug.Log($"Piece {piece.name} did not follow its normal command because its morale is {morale}.", this);
            return;
        }

        if (refuseNextCommand)
        {
            refuseNextCommand = false;
            Debug.Log($"Piece {piece.name} refused its command.", this);
            return;
        }

        piece.Advance();
    }

    /// <summary>
    /// Called before this piece is removed after a capture. Other active agents
    /// adjust morale according to whether the captured piece was an ally.
    /// </summary>
    public void OnPieceDestroy()
    {
        if (piece == null)
        {
            return;
        }

        Team capturedTeam = piece.control;
        Debug.Log($"Piece {piece.name} was captured (team {capturedTeam}).", this);
        PieceDestroyed?.Invoke(capturedTeam);
    }

    private void HandlePieceDestroyed(Team capturedTeam)
    {
        if (piece == null || !piece.gameObject.activeInHierarchy || piece.health <= 0)
        {
            return;
        }

        int previousMorale = morale;
        morale += capturedTeam == piece.control
            ? -moraleLostWhenAllyIsCaptured
            : moraleGainedWhenEnemyIsCaptured;
        morale = Mathf.Clamp(morale, 0, 100);

        if (morale >= recoverThreshold)
        {
            refuseNextCommand = false;
        }

        Debug.Log($"Morale updated for {piece.name}: {previousMorale} -> {morale} (captured {capturedTeam}).", this);
    }

    private static bool IsReserved(BoardPos destination, BoardPos[] reservedTiles)
    {
        if (reservedTiles == null)
        {
            return false;
        }

        for (int i = 0; i < reservedTiles.Length; i++)
        {
            if (reservedTiles[i].X == destination.X && reservedTiles[i].Y == destination.Y)
            {
                return true;
            }
        }

        return false;
    }
}

public enum RequestActionType
{
    None,
    Refuse,
    Manic,
    Defect,
    Wait
}

/// <summary>A rogue decision, optionally carrying a zero-based board destination.</summary>
public struct RequestAction
{
    public RequestActionType Type;
    public BoardPos Destination;

    public RequestAction(RequestActionType type)
    {
        Type = type;
        Destination = new BoardPos(-1, -1, Team.None);
    }

    public RequestAction(RequestActionType type, BoardPos destination)
    {
        Type = type;
        Destination = destination;
    }
}
