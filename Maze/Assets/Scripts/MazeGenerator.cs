using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    // 迷路
    private Maze maze;
    // 迷路の行と列
    public int mazeRow = 5;
    public int mazeColumn = 5;

    // 迷路の構成部分
    public GameObject mazeFloor;
    public GameObject mazeWall_Horizontal;
    public GameObject mazeWall_Vertical;

    // Start is called before the first frame update
    void Start()
    {
        //乱数生成変数
        System.Random rnd = new System.Random();
        //スタートセル
        Vector2Int startPos = new Vector2Int(rnd.Next(0, mazeRow - 1), rnd.Next(0, mazeColumn - 1));
        //迷路生成
        maze = new Maze(mazeRow, mazeColumn, mazeFloor, mazeWall_Horizontal, mazeWall_Vertical);
        maze.Generate(mazeRow, mazeColumn, startPos);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
