using UnityEngine;

public class Healer_AI : BoardPiece
{
    public int healAmount = 2;
    public int maxHealth = 10;
    public BoardPiece[] allAllies;

    void Update()
    {
        if (control == PieceControl.AI)
        {
            reflexAction();
        }
    }

    private void reflexAction()
    {
        BoardPiece weakest = null;
        int lowest = int.MaxValue;

        foreach (BoardPiece piece in allAllies)
        {
            if (piece == null)
            {
                continue;
            }

            if (piece.control == this.control)
            {
                if (piece.health < lowest)
                {
                    lowest = piece.health;
                    weakest = piece;
                }
            }
        }

        if (weakest != null)
        {
            weakest.health += healAmount;
        }
    }
}
