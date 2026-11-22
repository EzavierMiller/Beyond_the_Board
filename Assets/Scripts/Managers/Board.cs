using System;
using System.Collections.Generic;
using System.Text;

public class Board
{
    private readonly Team[,] localView;

    public readonly Dictionary<Team, List<BoardPiece>> capturedEachTeam = new Dictionary<Team, List<BoardPiece>>();

    public int Width => localView.GetLength(0);
    public int Height => localView.GetLength(1);

    public Board(int width, int height)
    {
        localView = new Team[width, height];
    }

    public Team this[int x, int y]
    {
        get => localView[x, y];
        set => localView[x, y] = value;
    }

    public Team[,] ToArray() => (Team[,])localView.Clone();
    public static implicit operator Team[,](Board board) => board.ToArray();

    /// <summary>Updates the side recorded at the piece's current board position.</summary>
    public void SetTeam(BoardPiece agent, Team team)
    {
        if (agent == null)
        {
            return;
        }

        BoardPos pos = agent.GetPos();
        if (IsInBounds(pos.X, pos.Y))
        {
            localView[pos.X, pos.Y] = team;
        }
    }

    /// <summary>Records a captured piece under the side that captured it.</summary>
    public void AddCapture(BoardPiece agent, Team captors)
    {
        if (agent == null)
        {
            return;
        }

        if (!capturedEachTeam.TryGetValue(captors, out List<BoardPiece> captured))
        {
            captured = new List<BoardPiece>();
            capturedEachTeam[captors] = captured;
        }

        if (!captured.Contains(agent))
        {
            captured.Add(agent);
        }
    }

    /// <summary>True when the zero-based destination is on the board and unoccupied.</summary>
    public bool IsLegal(BoardPos cmd)
    {
        return IsInBounds(cmd.X, cmd.Y) && localView[cmd.X, cmd.Y] == Team.None;
    }

    public void AddSpace(BoardPos cmd)
    {
        if (IsInBounds(cmd.X, cmd.Y))
        {
            localView[cmd.X, cmd.Y] = cmd.currSpace;
        }
    }

    public BoardPos[] GetReservedTiles()
    {
        var occupied = new List<BoardPos>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Team space = localView[x, y];
                if (space != Team.None)
                {
                    occupied.Add(new BoardPos(x, y, space));
                }
            }
        }

        return occupied.ToArray();
    }

    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("  ");
        for (int x = 0; x < Width; x++)
        {
            sb.Append($"{x} ");
        }
        sb.AppendLine();

        for (int y = 0; y < Height; y++)
        {
            sb.Append($"{y} ");
            for (int x = 0; x < Width; x++)
            {
                char symbol = localView[x, y] switch
                {
                    Team.None => '.',
                    Team.Player => 'P',
                    Team.AI => 'A',
                    Team.Rogue => 'R',
                    _ => '?'
                };
                sb.Append($"{symbol} ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

public enum Team
{
    None,
    Player,
    AI,
    Rogue
}

public struct BoardPos
{
    public int X;
    public int Y;
    public Team currSpace;

    public BoardPos(int x, int y, Team space)
    {
        X = x;
        Y = y;
        currSpace = space;
    }

    public static implicit operator BoardPos((int x, int y, Team z) tuple) => new BoardPos(tuple.x, tuple.y, tuple.z);
    public static implicit operator (int x, int y, Team z)(BoardPos coord) => (coord.X, coord.Y, coord.currSpace);

    public int ManhattanDistanceTo(BoardPos other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    public double EuclideanDistanceTo(BoardPos other)
    {
        int dx = X - other.X;
        int dy = Y - other.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
