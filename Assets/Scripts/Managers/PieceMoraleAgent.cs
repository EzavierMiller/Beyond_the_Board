using System;
using UnityEngine;

public class PieceMoraleAgent : MonoBehaviour
{

    // Morale Handling
    void Update()
    {
        throw new NotImplementedException();
    }

    // Returns a random request to move in the nearby vicinity while manic
    RequestAction FallbackManic(Board board, BoardPos[] reservedTiles)
    {
        throw new NotImplementedException();
    }

    // Returns a request based on current morale value.
    RequestAction DecideRogueAction(Board board, BoardPos[] reservedTiles)
    {
        throw new NotImplementedException();
    }

    // Single Agent AI Here (Piece.Move())
    void ChooseAction()
    {
        throw new NotImplementedException();
    }

    // Add this to have an effect when a piece gets destroyed/captured, replacing destroying the piece from our single agent versions.
    void OnPieceDestroy()
    {
        throw new NotImplementedException();
    }

}
public enum RequestAction
{
    None,
    Refuse 
    //or more
}