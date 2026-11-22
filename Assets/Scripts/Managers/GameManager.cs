using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Audio.ProcessorInstance;
public class GameManager : MonoBehaviour
{
    public List<BoardPiece> pieces;
    public static GameManager instance;
    public BoardPiece player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
    }

    //Debug
    public void AdvanceAll()
    {
        foreach (BoardPiece piece in pieces)
        {
            piece.Advance();
        }
    }
    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public Board GetBoard()
    {
        Board board = new Board(8,8);
        foreach (BoardPiece piece in pieces) { board.AddSpace(piece.GetPos()); }
        return board;
    }

    public void PrintBoard()
    {
        Debug.Log(GetBoard().ToString());
    }
    // Moves a piece to a certain request.
    void TryManicMove(BoardPiece agent, RequestAction request, BoardPos[] reservedTiles)
    {
        throw new NotImplementedException();
    }

    // Board Communication; removes pieces from the board if they are captured (is in the captured LIST or ARRAY).
    void ApplyAndCheckCaptures()
    {
        throw new NotImplementedException();
    }

    // Removes the current piece from the team.
    void UnregisterAgent(BoardPiece agent)
    {
        throw new NotImplementedException();
    }

    // Board Communication between the pieces. Manage phases and turns.
    void Update()
    {
        throw new NotImplementedException();
    }

    // Makes sure this upcoming move is valid. (Board.IsLegal())
    bool ValidateMove(BoardPos cmd)
    {
        throw new NotImplementedException();
    }
}