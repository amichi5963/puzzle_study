using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct FallDate
{
    public readonly int X { get; }
    public readonly int Y { get; }
    public readonly int Dest { get; }//堕ちる先
    public FallDate(int x, int y, int dest)
    {
        X = x; 
        Y = y; 
        Dest = dest; 
    }
}

public class BoardController : MonoBehaviour
{
    public const int FALL_FRAME_PAR_CELL = 5;// 単位セルあたりの落下フレーム数
    public const int BOARD_WIDTH = 6;
    public const int BOARD_HEIGHT = 14;

    [SerializeField] GameObject prefabPuyo = default!;

    int[,] _board = new int[BOARD_HEIGHT,BOARD_WIDTH];
    GameObject[,] _Puyos = new GameObject[BOARD_HEIGHT,BOARD_WIDTH];

    //堕ちる際の一時的変数
    List<FallDate> _falls = new();
    int _fallFrames = 0;

    private void ClearAll()
    {
        for(int y = 0; y < BOARD_HEIGHT; y++)
        {
            for(int x = 0; x < BOARD_WIDTH; x++)
            {
                _board[y,x] = 0;

                if (_Puyos[y,x] !=null) Destroy(_Puyos[y,x]);
                _Puyos[y, x] = null;
            }
        }
    }

    void Start()
    {
        ClearAll();
    }

    public static bool IsValidated(Vector2Int pos)
    {
        return 0 <= pos.y && pos.y < BOARD_HEIGHT
            && 0 <= pos.x && pos.x < BOARD_WIDTH;
    }

    public bool CanSettle(Vector2Int pos)
    {
        if(!IsValidated(pos)) return false;

        return 0 == _board[pos.y,pos.x];
    }

    public bool Settle(Vector2Int pos, int val)
    {
        if(!CanSettle(pos)) return false;

        _board[pos.y,pos.x] = val;

        Debug.Assert(_Puyos[pos.y,pos.x] == null);
        Vector3 worle_position = transform.position + new Vector3(pos.x, pos.y, 0.0f);
        _Puyos[pos.y,pos.x] = Instantiate(prefabPuyo, worle_position, Quaternion.identity, transform);
        _Puyos[pos.y, pos.x].GetComponent<PuyoController>().SetPuyoType((PuyoType)val);

        return true;
    }

    //下が空間となっていて堕ちるぷよを検索する
    public bool CheckFall()
    {
        _falls.Clear();
        _fallFrames = 0;

        //堕ちる先の高さの記録用
        int[] dsts = new int[BOARD_WIDTH];
        for(int x = 0; x < BOARD_WIDTH; x++) dsts[x] = 0;

        int max_check_line = BOARD_HEIGHT - 1;//実はぷよぷよ通では最上段は落ちてこない
        for(int y=0; y<max_check_line; y++)//下から上に検索
        {
            for(int x=0; x<BOARD_WIDTH; x++)
            {
                if (_board[y , x] == 0) continue;

                int dst = dsts[x];
                dsts[x] = y + 1;//上のぷよが　落ちてくるなら　自分の上

                if (y == 0) continue;//一番下なら落ちない

                if(_board[y - 1, x] != 0) continue; //下があれば対象外

                _falls.Add(new FallDate(x,y,dst));

                //データを変更しておく
                _board[dst, x] = _board[y, x];
                _board[y, x] = 0;
                _Puyos[dst, x] = _Puyos[y, x];  
                _Puyos[y, x] = null;

                dsts[x] = dst + 1;//次の物は落ちたさらに上に乗る
            }
        }

        return _falls.Count != 0;
    }

    public bool Fall()
    {
        _fallFrames++;

        float dy = _fallFrames / (float)FALL_FRAME_PAR_CELL;
        int di = (int)dy;

        for(int i=_falls.Count-1; i>=0; i--)//ループ中で削除しても安全なように後から検索
        {
            FallDate f = _falls[i];

            Vector3 pos = _Puyos[f.Dest, f.X].transform.localPosition;
            pos.y = f.Y - dy;

            if (f.Y <= f.Dest + di)
            {
                pos.y = f.Dest;
                _falls.RemoveAt(i);
            }
            _Puyos[f.Dest,f.X].transform.localPosition = pos;//表示一の更新
        }

        return _falls.Count != 0;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}