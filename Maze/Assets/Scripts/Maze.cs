using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Maze : MonoBehaviour
{
    // 定数
    private const string FLOOR_NAME = "Floor";
    private const string HORIZONTAL_WALL_NAME = "HorizontalWall";
    private const string VERTICAL_WALL_NAME = "VerticalWall";

    //
    private struct Grid
    {
        public int x;
        public int y;

        public Grid(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    // 行列
    private int MAX_ROW;
    private int MAX_COLUMN;

    // 迷路の構成部分
    public GameObject _Floor;
    public GameObject _Wall_Horizontal;
    public GameObject _Wall_Vertical;

    //迷路生成利用変数
    //訪問状態保存配列
    int[,] VISITED;

    //セルの壁保存リスト
    List<KeyValuePair<string, Grid>> WALL_LIST;

    // コンストラクタ
    public Maze(int row, int column, GameObject floor, GameObject horizontalWall, GameObject verticalWall)
    {
        //初期化
        VISITED = new int[row, column];
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                VISITED[r, c] = 0;
            }
        }

        WALL_LIST = new List<KeyValuePair<string, Grid>>();

        //行数と列数保存
        MAX_ROW    = row;
        MAX_COLUMN = column;
        
        //構成部分保存
        _Floor           = floor;
        _Wall_Horizontal = horizontalWall;
        _Wall_Vertical   = verticalWall;

        //フロアのサイズによって壁のPosition計算
        float offset_X =  floor.transform.localScale.x;
        float offset_Y =  floor.transform.localScale.y;
        float offset_Z = -floor.transform.localScale.z;

        //フロアと壁生成
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                this.CreatePart(floor,          r, c, offset_X, offset_Y, offset_Z);
                this.CreatePart(horizontalWall, r, c, offset_X, offset_Y, offset_Z);
                this.CreatePart(verticalWall,   r, c, offset_X, offset_Y, offset_Z);

                //最後の一行横壁
                this.CreatePart(horizontalWall, row, c, offset_X, offset_Y, offset_Z);
            }

            //最後の一列縦壁
            this.CreatePart(verticalWall, r, column, offset_X, offset_Y, offset_Z);
        }
    }

    // 迷路生成
    public void Generate(int row, int column, Vector2Int startPos)
    {
        //乱数生成変数
        System.Random rnd = new System.Random();

        //配列index
        int x = startPos.x;
        int y = startPos.y;

        //迷路作成
        VISITED[x, y] = 1;
        //セルの壁をリストに追加
        AddWallList(x, y);
        //リスト内壁あるならループ
        while (WALL_LIST.Count != 0)
        {
            int next_x = x;
            int next_y = y;

            //乱数で壁を選ぶ
            int go_next = rnd.Next(0, WALL_LIST.Count - 1);
            KeyValuePair<string, Grid> wall = WALL_LIST[go_next];

            //横壁の場合、壁の左セルと右セルを判断
            if (wall.Key.Contains(HORIZONTAL_WALL_NAME))
            {
                x = wall.Value.x - 1;
                y = wall.Value.y;
                next_x = wall.Value.x;
                next_y = wall.Value.y;
            }
            //縦壁の場合、壁の上セルと下セルを判断
            else if (wall.Key.Contains(VERTICAL_WALL_NAME))
            {
                x = wall.Value.x;
                y = wall.Value.y - 1;
                next_x = wall.Value.x;
                next_y = wall.Value.y;
            }

            //訪問済みかを判断
            if (VISITED[x, y] == 0 && VISITED[next_x, next_y] == 1)
            {
                //訪問済みと設定
                VISITED[x, y] = 1;
                //セルの壁をリストに追加
                AddWallList(x, y);
                //この壁オブジェクトを削除
                DestroyWall(wall.Key);
            }
            else if (VISITED[x, y] == 1 && VISITED[next_x, next_y] == 0)
            {
                //訪問済みと設定
                VISITED[next_x, next_y] = 1;
                //セルの壁をリストに追加
                AddWallList(next_x, next_y);
                //この壁オブジェクトを削除
                DestroyWall(wall.Key);
            }

            //壁をリストから削除
            WALL_LIST.RemoveAt(go_next);
        }

    }

    // フロア/壁生成
    private void CreatePart(GameObject part, int row, int column, float offset_X, float offset_Y, float offset_Z)
    {
        //Position計算
        Vector3 position = new Vector3(column * offset_X + part.transform.localPosition.x,
                                       part.transform.localPosition.y,
                                       row * offset_Z + part.transform.localPosition.z);

        //Part生成
        GameObject newPart = Instantiate(part);
        newPart.transform.localPosition = position;
        //名前設定
        if(part == _Floor)
        {
            newPart.name = GetPartName(FLOOR_NAME, row, column);
        }
        else if(part == _Wall_Horizontal)
        {
            newPart.name = GetPartName(HORIZONTAL_WALL_NAME, row, column);
        }
        else if(part == _Wall_Vertical)
        {
            newPart.name = GetPartName(VERTICAL_WALL_NAME, row, column);
        }

        //固定で開始セルを[0,0]、終了セルを[MAXROW-1,MAXCOLUMN-1]と設定
        if(row == 0 && column == 0 && part == _Floor)
        {
            newPart.GetComponent<Renderer>().material.color = Color.green;
        }
        if(row == MAX_ROW - 1 && column == MAX_COLUMN - 1 && part == _Floor)
        {
            newPart.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    //Part名前生成
    private string GetPartName(string typeName, int row, int column)
    {
        string ret = "";
        ret = typeName + "_" + System.Convert.ToString(row) + "_" + System.Convert.ToString(column);
        return ret;
    }

    //壁リストに壁追加
    private void AddWallList(int x, int y)
    {
        void AddWall(string key, int add_x, int add_y) =>
            WALL_LIST.Add(new KeyValuePair<string, Grid>(key, new Grid(add_x, add_y)));

        //左の壁をリストに追加
        if (y != 0)
        {
            AddWall(GetPartName(VERTICAL_WALL_NAME, x, y), x, y);
        }

        //上の壁をリストに追加
        if (x != 0 )
        {
            AddWall(GetPartName(HORIZONTAL_WALL_NAME, x, y), x, y);
        }

        //右の壁をリストに追加
        if ((x != MAX_ROW) && ((y + 1) != MAX_COLUMN))
        {
            AddWall(GetPartName(VERTICAL_WALL_NAME, x, y + 1), x, y + 1);
        }

        //下の壁をリストに追加
        if (((x + 1) != MAX_ROW) && (y != MAX_COLUMN))
        {
            AddWall(GetPartName(HORIZONTAL_WALL_NAME, x + 1, y), x + 1, y);
        }
    }

    //壁を削除
    private void DestroyWall(string wall_name)
    {
        GameObject wall = GameObject.Find(wall_name);

        if (wall != null)
        {
            GameObject.Destroy(wall);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
