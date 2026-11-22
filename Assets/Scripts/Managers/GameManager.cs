using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public List<BoardPiece> pieces = new List<BoardPiece>();
    public static GameManager instance;
    public BoardPiece player;

    public enum TurnPhase { Player, Enemy, Rogue }
    public TurnPhase Phase { get; private set; } = TurnPhase.Player;
    public int Turn { get; private set; } = 1;
    public BoardPiece ActingPiece { get; private set; }
    public bool IsDispatching => ActingPiece != null;

    private Board board;
    private bool actionCommitted;
    private readonly HashSet<BoardPiece> acted = new HashSet<BoardPiece>();
    private readonly HashSet<string> rolesActed = new HashSet<string>();
    private readonly List<BoardPos> phaseReservations = new List<BoardPos>();
    private readonly Dictionary<BoardPiece, PieceMoraleAgent> agents = new Dictionary<BoardPiece, PieceMoraleAgent>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Only one GameManager may control a loaded board.", this);
            enabled = false;
            return;
        }
        instance = this;
    }

    private void Start() => InitializeBoard();
    private void OnDestroy() { if (instance == this) instance = null; }

    public void InitializeBoard()
    {
        if (board != null) return;
        board = new Board(8, 8);
        if (player != null && !pieces.Contains(player)) pieces.Add(player);
        for (int i = pieces.Count - 1; i >= 0; i--)
        {
            BoardPiece piece = pieces[i];
            if (piece == null || !piece.gameObject.activeInHierarchy || piece.health <= 0)
            {
                pieces.RemoveAt(i);
                continue;
            }
            if (pieces.IndexOf(piece) != i) { pieces.RemoveAt(i); continue; }
            if (!board.Register(piece))
            {
                Debug.LogError($"Cannot register {piece.name}: invalid side, coordinates, or occupied tile.", piece);
                continue;
            }
            agents[piece] = piece.GetComponent<PieceMoraleAgent>();
        }
    }

    public Board GetBoard() { InitializeBoard(); return board; }
    public void PrintBoard() => Debug.Log(GetBoard().ToString());
    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().path);

    // Retains the existing Main scene button callback, now completing the player turn.
    public void AdvanceAll() => EndPlayerTurn();
    public void IssuePlayerOrder() { if (player != null) IssueOrder(player); }

    public bool CanAct(BoardPiece piece)
    {
        if (piece == null || piece.health <= 0 || !piece.gameObject.activeInHierarchy || !pieces.Contains(piece)) return false;
        if (!GetBoard().TryGetPosition(piece, out _)) return false;
        if (acted.Contains(piece) || rolesActed.Contains(RoleKey(piece))) return false;
        return Phase == TurnPhase.Player ? piece.control == Team.Player :
            Phase == TurnPhase.Enemy && piece.control == Team.AI;
    }

    private static string RoleKey(BoardPiece piece)
    {
        return piece.control + ":" + (piece.GetComponent<MageAgent>() != null ? "Mage" : piece.GetType().Name);
    }

    public bool IssueOrder(BoardPiece piece)
    {
        if (!CanAct(piece)) return false;
        acted.Add(piece);
        rolesActed.Add(RoleKey(piece));
        agents.TryGetValue(piece, out PieceMoraleAgent agent);
        if (agent != null && agent.ConsumeRefusal()) return false;
        if (piece.control == Team.Player && agent != null && agent.IsRogue)
        {
            Debug.Log($"turn={Turn} piece={piece.name} response=refuse morale={agent.Morale}", piece);
            return false;
        }
        ActingPiece = piece;
        actionCommitted = false;
        try
        {
            if (agent != null) agent.ChooseAction();
            else { piece.Logic(); piece.Advance(); }
        }
        finally
        {
            if (piece != null && board.TryGetPosition(piece, out BoardPos pos))
            {
                piece.xLoc = pos.X + 1;
                piece.yLoc = pos.Y + 1;
            }
            ActingPiece = null;
        }
        Debug.Log($"turn={Turn} piece={piece.name} response=obey", this);
        return true;
    }

    public bool IssueMove(BoardPiece piece, int x, int y)
    {
        if (!CanAct(piece)) return false;
        acted.Add(piece);
        rolesActed.Add(RoleKey(piece));
        agents.TryGetValue(piece, out PieceMoraleAgent agent);
        if (agent != null && (agent.ConsumeRefusal() || agent.IsRogue)) return false;
        ActingPiece = piece;
        actionCommitted = false;
        try { return TryMove(piece, x, y); }
        finally { ActingPiece = null; }
    }

    public bool TryMove(BoardPiece piece, int x, int y)
    {
        if (ActingPiece != piece || actionCommitted || piece == null || piece.health <= 0) return false;
        bool moved = GetBoard().TryMove(piece, new BoardPos(x - 1, y - 1, Team.None));
        if (moved) actionCommitted = true;
        return moved;
    }

    public bool TryDamage(BoardPiece actor, BoardPiece target, int damage)
    {
        if (ActingPiece != actor || actionCommitted || actor == null || target == null || damage <= 0 ||
            actor.control == target.control || !board.TryGetPosition(target, out _)) return false;
        actionCommitted = true;
        target.health = Mathf.Max(0, target.health - damage);
        if (target.health == 0) CapturePiece(target, actor.control);
        return true;
    }

    public bool TryHeal(BoardPiece actor, BoardPiece target, int amount, int maxHealth)
    {
        if (ActingPiece != actor || actionCommitted || actor == null || target == null || amount <= 0 || target.health <= 0 ||
            actor.control != target.control || !board.TryGetPosition(target, out _)) return false;
        actionCommitted = true;
        target.health = Mathf.Min(Mathf.Max(target.health, maxHealth), target.health + amount);
        return true;
    }

    public void CapturePiece(BoardPiece captured, Team captor)
    {
        if (captured == null || captor == Team.None || !GetBoard().TryGetPosition(captured, out _)) return;
        CaptureEvent capture = new CaptureEvent(Turn, captured, captor);
        board.AddCapture(captured, captor);
        board.CaptureHistory.Add(capture);
        board.Remove(captured);
        captured.health = 0;
        agents.TryGetValue(captured, out PieceMoraleAgent agent);
        pieces.Remove(captured);
        agents.Remove(captured);
        if (agent != null) agent.OnPieceDestroy(capture);
        else PieceMoraleAgent.PublishCapture(capture);
        Debug.Log($"turn={Turn} id={capture.PieceId} type={capture.PieceType} cause=capture side={capture.CapturedTeam} captor={captor}", this);
        captured.gameObject.SetActive(false);
        Destroy(captured.gameObject);
    }

    public void UnregisterAgent(BoardPiece piece)
    {
        if (piece == null) return;
        board?.Remove(piece);
        pieces.Remove(piece);
        agents.Remove(piece);
    }

    public void EndPlayerTurn()
    {
        if (Phase != TurnPhase.Player || IsDispatching) return;
        InitializeBoard();
        Phase = TurnPhase.Enemy;
        try
        {
            foreach (BoardPiece piece in new List<BoardPiece>(pieces))
                if (piece != null && piece.control == Team.AI) IssueOrder(piece);
            RunRoguePhase();
        }
        finally
        {
            ActingPiece = null;
            Phase = TurnPhase.Player;
            Turn++;
            acted.Clear();
            rolesActed.Clear();
        }
    }

    public void RunRoguePhase()
    {
        if (Phase != TurnPhase.Enemy || IsDispatching) return;
        Phase = TurnPhase.Rogue;
        phaseReservations.Clear();
        var rogues = new List<BoardPiece>();
        foreach (BoardPiece piece in pieces)
            if (piece != null && piece.control == Team.Player && agents.TryGetValue(piece, out PieceMoraleAgent agent) && agent != null && agent.IsRogue)
                rogues.Add(piece);
        rogues.Sort((a, b) =>
        {
            int compare = agents[a].Morale.CompareTo(agents[b].Morale);
            return compare != 0 ? compare : string.CompareOrdinal(a.PieceId, b.PieceId);
        });
        int defections = 0;
        foreach (BoardPiece piece in rogues)
        {
            if (piece == null || piece.health <= 0 || !pieces.Contains(piece) || !agents.TryGetValue(piece, out PieceMoraleAgent agent) || agent == null || !agent.IsRogue) continue;
            BoardPos[] reserved = phaseReservations.ToArray();
            RequestAction request = agent.DecideRogueAction(board, reserved);
            if (request.Type == RequestActionType.Defect)
            {
                if (defections == 0)
                {
                    board.SetTeam(piece, Team.AI);
                    defections++;
                    Debug.Log($"turn={Turn} piece={piece.name} cause=defection", piece);
                    continue;
                }
                request = agent.FallbackManic(board, reserved);
            }
            if (request.Type == RequestActionType.Manic) TryManicMove(piece, request, reserved);
            else if (request.Type == RequestActionType.Refuse) agent.SetRefusal();
        }
    }

    public void TryManicMove(BoardPiece piece, RequestAction request, BoardPos[] reservedTiles)
    {
        if (Phase != TurnPhase.Rogue || piece == null || piece.health <= 0 || !piece.gameObject.activeInHierarchy ||
            !pieces.Contains(piece) || piece.control != Team.Player || request.Type != RequestActionType.Manic ||
            !agents.TryGetValue(piece, out PieceMoraleAgent agent) || agent == null || !agent.IsRogue)
        {
            Debug.Log($"turn={Turn} cause=manic_rejected reason=invalid_actor_phase_or_request", this);
            return;
        }
        BoardPos destination = request.Destination;
        if (!board.IsLegal(destination) || IsReserved(destination, reservedTiles) || IsReserved(destination, phaseReservations.ToArray()))
        {
            Debug.Log($"turn={Turn} piece={piece.PieceId} cause=manic_rejected reason=unavailable_destination", piece);
            return;
        }
        if (!board.TryMove(piece, destination))
        {
            Debug.Log($"turn={Turn} piece={piece.PieceId} cause=manic_rejected reason=non_adjacent_or_unregistered", piece);
            return;
        }
        phaseReservations.Add(destination);
        Debug.Log($"turn={Turn} piece={piece.name} cause=manic_move tile=({destination.X},{destination.Y})", piece);
    }

    private static bool IsReserved(BoardPos destination, BoardPos[] reservedTiles)
    {
        if (reservedTiles == null) return false;
        foreach (BoardPos tile in reservedTiles)
            if (tile.X == destination.X && tile.Y == destination.Y) return true;
        return false;
    }
}
