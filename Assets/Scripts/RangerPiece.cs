using UnityEngine;

public class RangerPiece : BoardPiece
{
    public int range = 3;
    public float distance = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    public override void Logic()
    {
        distance = Vector3.Distance(transform.position, targetPiece.transform.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(true)
        {

        }
    }
    public override void Advance()
    {
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
                        if (xLoc > 1 && xLoc < 4)
                        {
                            tempX = Mathf.Clamp(++xLoc, 1, 8);
                            break;
                        }
                        if (xLoc < 8 && xLoc >= 4)
                        {
                            tempX = Mathf.Clamp(--xLoc, 1, 8);
                            break;
                        }
                        if (xLoc == 8 )
                        {
                            tempX = Mathf.Clamp(--xLoc, 1, 8);
                            break;
                        }
                        if (xLoc == 1)
                        {
                            tempX = Mathf.Clamp(++xLoc, 1, 8);
                            Debug.Log("Lemme squeeze on by!");
                            break;
                        }

                    }

                    if (targetPiece.GetX() > xLoc)
                    {
                        tempX = Mathf.Clamp(--xLoc, 1, 8);
                        break;
                    }

                    if (targetPiece.GetX() < xLoc)
                    {
                        tempX = Mathf.Clamp(++xLoc, 1, 8);
                        break;
                    }

                    if (targetPiece.GetY() > yLoc)
                    {
                        tempY = Mathf.Clamp(--yLoc, 1, 8);
                        break;
                    }

                    if (targetPiece.GetY() < yLoc)
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
