using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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

    // Update is called once per frame
    void Update()
    {

    }
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
}