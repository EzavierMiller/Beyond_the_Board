using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public List<BoardPiece> pieces = new List<BoardPiece>();
    public static GameManager instance;
    public BoardPiece player;

    private void Start()
    {
        instance = this;
    }

    // Debug helper
    public void AdvanceAll()
    {
        foreach (BoardPiece piece in pieces)
        {
            if (piece != null)
            {
                piece.Advance();
            }
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public Board GetBoard()
    {
        Board board = new Board(8, 8);
        foreach (BoardPiece piece in pieces)
        {
            if (piece != null && piece.gameObject.activeInHierarchy)
            {
                board.AddSpace(piece.GetPos());
            }
        }
        return board;
    }

    public void PrintBoard()
    {
        Debug.Log(GetBoard().ToString());
    }

    /// <summary>
    /// Applies a manic move request if its destination is still free and is
    /// adjacent to the requesting piece. BoardPos is zero-based; Move is one-based.
    /// </summary>
    public void TryManicMove(BoardPiece agent, RequestAction request, BoardPos[] reservedTiles)
    {
        if (agent == null || !agent.gameObject.activeInHierarchy || request.Type != RequestActionType.Manic)
        {
            return;
        }

        BoardPos destination = request.Destination;
        Board board = GetBoard();
        if (!board.IsLegal(destination) || IsReserved(destination, reservedTiles))
        {
            Debug.Log($"Rejected manic move for {agent.name}: destination ({destination.X}, {destination.Y}) is unavailable.", agent);
            return;
        }

        int currentX = agent.GetX() - 1;
        int currentY = agent.GetY() - 1;
        if (Mathf.Abs(destination.X - currentX) > 1 || Mathf.Abs(destination.Y - currentY) > 1 ||
            (destination.X == currentX && destination.Y == currentY))
        {
            Debug.Log($"Rejected manic move for {agent.name}: destination is not adjacent.", agent);
            return;
        }

        agent.Move(destination.X + 1, destination.Y + 1);
        Debug.Log($"{agent.name} made a manic move to ({destination.X}, {destination.Y}).", agent);
    }

    // Removes captured pieces after the Board has recorded them.
    private void ApplyAndCheckCaptures()
    {
        Board board = GetBoard();
        foreach (List<BoardPiece> capturedList in board.capturedEachTeam.Values)
        {
            for (int i = pieces.Count - 1; i >= 0; i--)
            {
                BoardPiece capturedPiece = pieces[i];
                if (capturedPiece != null && capturedList.Contains(capturedPiece))
                {
                    PieceMoraleAgent moraleAgent = capturedPiece.GetComponent<PieceMoraleAgent>();
                    if (moraleAgent != null)
                    {
                        moraleAgent.OnPieceDestroy();
                    }

                    pieces.RemoveAt(i);
                    Destroy(capturedPiece.gameObject);
                }
            }
        }
    }

    public void UnregisterAgent(BoardPiece agent)
    {
        if (agent != null)
        {
            pieces.Remove(agent);
        }
    }

    // Turn/phase orchestration will call the action methods when implemented.
    private void Update()
    {
    }

    private bool ValidateMove(BoardPos cmd)
    {
        return GetBoard().IsLegal(cmd);
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
