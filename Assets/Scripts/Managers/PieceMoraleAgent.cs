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
    [Serializable]
    private sealed class CaptureWeight
    {
        public string capturedType;
        [Min(0)] public int allyLost;
        [Min(0)] public int enemyTaken;
    }
    [SerializeField] private CaptureWeight[] captureWeights = Array.Empty<CaptureWeight>();

    private BoardPiece piece;
    private MageAgent mageAgent;
    private bool refuseNextCommand;
    private bool isRogue;
    private bool captureNotified;

    public int Morale => morale;
    public bool IsRogue => isRogue;
    public bool RefuseNextCommand => refuseNextCommand;

    private void Awake()
    {
        piece = GetComponent<BoardPiece>();
        mageAgent = GetComponent<MageAgent>();
        isRogue = morale < rogueThreshold;
        if (piece == null)
        {
            Debug.LogError("PieceMoraleAgent requires a BoardPiece on the same GameObject.", this);
        }
    }

    public void SetRefusal() => refuseNextCommand = true;
    public bool ConsumeRefusal()
    {
        if (!refuseNextCommand) return false;
        refuseNextCommand = false;
        Debug.Log($"piece={piece.name} response=refuse morale={morale}", this);
        return true;
    }

    private void OnEnable()
    {
        PieceDestroyed += HandlePieceDestroyed;
    }

    private void OnDisable()
    {
        PieceDestroyed -= HandlePieceDestroyed;
    }

    private static event Action<CaptureEvent> PieceDestroyed;

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
        if (!IsRogue)
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
        if (piece == null || !piece.gameObject.activeInHierarchy || piece.health <= 0) return;
        if (piece.Manager != null && piece.Manager.ActingPiece != piece) return;
        if (ConsumeRefusal()) return;
        // Rogue behavior is requested separately; normal decisions are not run per frame.
        if (piece.control == Team.Player && IsRogue) return;
        if (mageAgent != null) mageAgent.TakeTurn();
        else
        {
            piece.Logic();
            piece.Advance();
        }
    }

    /// <summary>
    /// Called before this piece is removed after a capture. Other active agents
    /// adjust morale according to whether the captured piece was an ally.
    /// </summary>
    public void OnPieceDestroy()
    {
        if (piece != null && piece.Manager != null && piece.Manager.ActingPiece != null && piece.Manager.ActingPiece != piece)
        {
            piece.Manager.CapturePiece(piece, piece.Manager.ActingPiece.control);
            return;
        }
        Debug.LogWarning("A capture requires a GameManager action and known capturing side.", this);
    }

    public void OnPieceDestroy(CaptureEvent capture)
    {
        if (captureNotified) return;
        captureNotified = true;
        PieceDestroyed -= HandlePieceDestroyed;
        PublishCapture(capture);
    }

    public static void PublishCapture(CaptureEvent capture)
    {
        PieceDestroyed?.Invoke(capture);
    }

    private void HandlePieceDestroyed(CaptureEvent capture)
    {
        if (piece == null || !piece.gameObject.activeInHierarchy || piece.health <= 0 ||
            piece.control != Team.Player || capture.PieceId == piece.PieceId) return;

        int previousMorale = morale;
        int allyLost = moraleLostWhenAllyIsCaptured;
        int enemyTaken = moraleGainedWhenEnemyIsCaptured;
        foreach (CaptureWeight weight in captureWeights)
            if (weight != null && weight.capturedType == capture.PieceType)
            {
                allyLost = weight.allyLost;
                enemyTaken = weight.enemyTaken;
                break;
            }
        morale += capture.CapturedTeam == piece.control ? -allyLost : enemyTaken;
        morale = Mathf.Clamp(morale, 0, 100);
        if (!isRogue && morale < rogueThreshold) isRogue = true;
        else if (isRogue && morale >= recoverThreshold) isRogue = false;
        if (!isRogue) refuseNextCommand = false;
        Debug.Log($"turn={capture.Turn} piece={piece.name} id={piece.GetEntityId()} cause=capture type={capture.PieceType} side={capture.CapturedTeam} captor={capture.CapturingTeam} morale={previousMorale}->{morale} rogue={isRogue}", this);
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
