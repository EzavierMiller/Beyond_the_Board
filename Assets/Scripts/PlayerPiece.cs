using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerPiece : BoardPiece
{
    public override void Logic()
    {
        //player controls for testing
        if (control == PieceControl.Player)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                xLoc--;
            }
            if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                xLoc++;
            }
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                yLoc--;
            }
            if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                yLoc++;
            }
        }
        // Update is called once per frame

    }

}