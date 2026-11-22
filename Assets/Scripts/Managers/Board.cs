using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

    // 2D index directly like a 2D array
    public Team this[int x, int y]
    {
        get => localView[x, y];
        set => localView[x, y] = value;
    }

    // 2. Returns a standalone 2D array copy
    public Team[,] ToArray() => (Team[,])localView.Clone();

    // 3. Return the Board where CellState[,] is expected
    public static implicit operator Team[,](Board board) => board.ToArray();

    // Team Management
    void SetTeam(BoardPiece agent, Team team)
    {
        BoardPos pos = agent.GetPos();
        localView[pos.X, pos.Y] = team;
    }

    // Adds the piece to a list of captured pieces, a list per faction.
    void AddCapture(BoardPiece agent, Team captors)
    {
        if (!capturedEachTeam.ContainsKey(captors))
        {
            capturedEachTeam[captors] = new List<BoardPiece>();
        }

        if (capturedEachTeam[captors].Contains(agent))
        {
            capturedEachTeam[captors].Add(agent);
        }
    }

    // Returns if a piece can exist in this spot on the board.
    public bool IsLegal(BoardPos cmd)
    {
        return cmd.X >= 0 && cmd.X < Width && cmd.Y >= 0 && cmd.Y < Height && localView[cmd.X, cmd.Y] == Team.None;
    }

    // Adds a space to the board.
    public void AddSpace(BoardPos cmd)
    {
        localView[cmd.X, cmd.Y] = cmd.currSpace;
    }

    public BoardPos[] GetReservedTiles()
    {
        var occupied = new List<BoardPos>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Team space = localView[x, y];
                if (space != Team.None) // None = 0
                {
                    occupied.Add(new BoardPos(x, y, space));
                }
            }
        }

        return occupied.ToArray();
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

        // Iterate Y in the outer loop so rows print horizontally top-to-bottom
        for (int y = 0; y < Height; y++)
        {
            sb.Append($"{y} "); // Optional: Row index

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

    // Implicit tuple conversions: Coord2D pos = (3, 4);
    public static implicit operator BoardPos((int x, int y, Team z) tuple) => new(tuple.x, tuple.y, tuple.z);
    public static implicit operator (int x, int y, Team z)(BoardPos coord) => (coord.X, coord.Y, coord.currSpace);
    // distance helpers
    public int ManhattanDistanceTo(BoardPos other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    public double EuclideanDistanceTo(BoardPos other)
    {
        int dx = X - other.X;
        int dy = Y - other.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}