using Unity.VisualScripting;
using UnityEngine;

public class MagePiece : BoardPiece
{
    public int range = 3;
    public float distance = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Logic()
    {

    }
    public override void Advance()
    {
        gameObject.GetComponent<MageAgent>().TakeTurn();

        if (distance <= range * 75 && distance > 75)
        {
            Debug.Log("Shoot!");
            Attack(targetPiece);
        }
        else if (distance <= 75)
        {
            int tempX = xLoc;
            int tempY = yLoc;
            bool moveFound = false;
            while (!moveFound)
            {
                if (targetPiece != null)
                {
                    if ((targetPiece.GetX() == xLoc && (yLoc == 1 || yLoc == 8)))
                    {
                        if (xLoc > 1 && xLoc < 4 && CanMove(xLoc + 1, yLoc))
                        {
                            tempX = Mathf.Clamp(++xLoc, 1, 8);
                            break;
                        }
                        if (xLoc < 8 && xLoc >= 4 && CanMove(xLoc - 1, yLoc))
                        {
                            tempX = Mathf.Clamp(--xLoc, 1, 8);
                            break;
                        }
                        if (xLoc == 8 && CanMove(xLoc - 1, yLoc))
                        {
                            tempX = Mathf.Clamp(--xLoc, 1, 8);
                            break;
                        }
                        if (xLoc == 1 && CanMove(xLoc + 1, yLoc))
                        {
                            tempX = Mathf.Clamp(++xLoc, 1, 8);
                            Debug.Log("Lemme squeeze on by!");
                            break;
                        }

                    }

                    if (targetPiece.GetX() > xLoc && CanMove(xLoc - 1, yLoc))
                    {
                        tempX = Mathf.Clamp(--xLoc, 1, 8);
                        break;
                    }

                    if (targetPiece.GetX() < xLoc && CanMove(xLoc + 1, yLoc))
                    {
                        tempX = Mathf.Clamp(++xLoc, 1, 8);
                        break;
                    }

                    if (targetPiece.GetY() > yLoc && CanMove(xLoc, yLoc - 1))
                    {
                        tempY = Mathf.Clamp(--yLoc, 1, 8);
                        break;
                    }

                    if (targetPiece.GetY() < yLoc && CanMove(xLoc, yLoc + 1))
                    {
                        tempY = Mathf.Clamp(++yLoc, 1, 8);
                        break;
                    }




                }
                moveFound = true;
            }
            moveFound = true;
            Move(tempX, tempY);
        }

    }
}
