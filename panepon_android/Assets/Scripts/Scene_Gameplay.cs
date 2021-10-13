using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パネルの構造体
/// </summary>
struct PanelData
{
    public bool panel_delete_flag;      // false:デフォルト  true:削除予定
    //public int panel_color;             // 0:無効 1:青色 2:緑色 3:赤色 
    public GameObject panel_info;       //Transform 情報など
    public bool search_flag;            //サーチパネルフラグ     //そのパネルがサーチする対象かどうか
    public bool up_flag;                //上方向のサーチ有無
    public bool down_flag;              //下方向のサーチ有無
    public bool right_flag;             //右方向のサーチ有無
    public bool left_flag;              //左方向のサーチ有無
    public int latest_x;                //パネル座標             //サーチ時こちらを見る
    public int latest_y;                //パネル座標
}

/// <summary>
/// サーチ方向(上下左右) //RecursionSearch用
/// </summary>
enum SEARCH_DIRECTION
{
    UP,
    DOWN,
    RIGHT,
    LEFT
}

/// <summary>
/// サーチ方向(上下か左右) 　//BothDirectionSerach用
/// </summary>
enum BOTH_DIRECTION
{
    UP_DOWN,
    RIGHT_LEFT
}


public class Scene_Gameplay : MonoBehaviour
{
    const float default_x = -2.5f;  //左の位置
    const float default_y = -5.5f;  //下の位置
    const float margin_y = 0.1f;    //パネル座標更新時の誤差マージン用

    int grayPanelArrayTop;  //一番上の無効パネルの配列番号
    
    private const int GRAY_PANEL_WIDTH = 6;  //無効パネルの横幅
    private const int GRAY_PANEL_HEIGHT = 3; //無効パネルの縦幅

    private List<PanelData> bluePanelList = new List<PanelData>();                              //パネル構造体リスト(青色)
    private List<PanelData> redPanelList = new List<PanelData>();                               //パネル構造体リスト(赤色)
    private List<PanelData> greenPanelList = new List<PanelData>();                             //パネル構造体リスト(緑色)
    private PanelData[,] grayPanelArray = new PanelData[GRAY_PANEL_HEIGHT, GRAY_PANEL_WIDTH];   //パネル構造体 配列(灰色)


    [SerializeField] bool Blue_Flag;                //サーチ対象フラグ(青色)
    [SerializeField] bool Red_Flag;                 //サーチ対象フラグ(赤色)
    [SerializeField] bool Green_Flag;               //サーチ対象フラグ(緑色)

    [SerializeField] float mymagunitude;            //重力加速度
    
    PanelData mypanel;                        //ブルーパネル生成用

    GameObject obj;                             //Panelインスタンス生成用

    int frameCnt;                               //フレームカウント
    int execUpPanel;                        //何フレーム毎に実施する回数

    //デバック用
    int flamecount = 0;
    float mytime = 0f;
    float oldmytime = 0f;

    SpriteRenderer MainSpriteRenderer;
    // publicで宣言し、inspectorで設定可能にする
    public Sprite sprite_gray;
    public Sprite sprite_blue;
    public Sprite sprite_red;           //後で設定する(8/8)
    public Sprite sprite_green;

    public Ray ray;
    public RaycastHit2D hit;

    // マウス位置座標 
 	private Vector3 mousePosition; 
 	// スクリーン座標をワールド座標に変換したマウス位置座標 
 	private Vector3 screenToWorldPointMousePosition;


    //左クリックした初期位置(タッチした初動位置)
    public float InitTouchPos_x;

    //ドラッグ(フリップ)した距離
    public float TouchDistance_x; //-の時、右に移動していて、+ の時左に移動しているため注意

    //パネル交換する際のオブジェクト
    private GameObject swapPanelInfo;
    //パネル交換する際のオブジェクトのx座標
    private float swapPanelInfo_trans_x;

    //パネルフレーム範囲の右端のx座標
    private float PanelFrameRight_x = 3.5f;
    //パネルフレーム範囲の左端のx座標
    private float PanelFrameLeft_x = - 3.5f;

    // Start is called before the first frame update
    void Start()
    {

        //コンストラクタ
        obj = (GameObject)Resources.Load("Panel");
        sprite_gray = Resources.Load<Sprite>("Images/Panel/Gray");
        sprite_blue = Resources.Load<Sprite>("Images/Panel/Blue");


        grayPanelArrayTop = 0;

        frameCnt = 0;
        execUpPanel = 5;

        //２．グレーパネル配置

        //縦
        for (int y = 0; y < GRAY_PANEL_HEIGHT; y++)
        {
            //横
            for(int x = 0; x < GRAY_PANEL_WIDTH; x++)
            {
                grayPanelArray[y, x].panel_delete_flag = false;
                //grayPanelArray[y, x].panel_color = 0;
                grayPanelArray[y, x].search_flag = false;
                grayPanelArray[y, x].up_flag = false;
                grayPanelArray[y, x].down_flag = false;
                grayPanelArray[y, x].right_flag = false;
                grayPanelArray[y, x].left_flag = false;
                grayPanelArray[y, x].latest_x = x;
                grayPanelArray[y, x].latest_y = (int)(default_y - y - 1);
                

                // プレハブを元にオブジェクトを生成する
                grayPanelArray[y, x].panel_info = (GameObject)Instantiate(obj,
                                                    new Vector3(default_x + x, default_y - y - 1, 0.0f),
                                                    Quaternion.identity);

                //パネルに色を付ける
                grayPanelArray[y,x].panel_info.gameObject.GetComponent<SpriteRenderer>().sprite = sprite_gray;
                grayPanelArray[y, x].panel_info.GetComponent<Panel_Test>().setPanelColor(0);

                if (grayPanelArray[y, x].panel_info.Equals(null))
                {
                    Debug.Log("Error:grayPanelArray.panel_info = null y = " + y + " x = " + x);
                }
            }
        }

        //ブルーパネル作成 (暫定)
        for (int y = 0; y < 1; y++)
        {
            mypanel = new PanelData();

            //色有りを作成する
            //構造体初期設定
            mypanel.panel_delete_flag = false;
            //mypanel.panel_color = 3;
            mypanel.search_flag = false;
            mypanel.up_flag = false;
            mypanel.down_flag = false;
            mypanel.right_flag = false;
            mypanel.left_flag = false;
            mypanel.latest_x = 2;
            mypanel.latest_y = (int)(-2.5f + y);

            // プレハブを元にオブジェクトを生成する
            mypanel.panel_info = (GameObject)Instantiate(obj,
                                                new Vector3(2 + default_x, -2.5f + y, 0.0f),
                                                Quaternion.identity);

            //パネルに色を付ける
            mypanel.panel_info.gameObject.GetComponent<SpriteRenderer>().sprite = sprite_blue;
            mypanel.panel_info.GetComponent<Panel_Test>().setPanelColor(1);

            if (!mypanel.panel_info.Equals(null))
            {
                //パネルの初期設定を行う
                //１．RigitBody2D初期設定 
                mypanel.panel_info.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX |
                                                                    RigidbodyConstraints2D.FreezeRotation;
                bluePanelList.Add(mypanel);
            }
        }
        //レッドーパネル作成 (暫定)
        for (int y = 0; y < 0; y++)
        {
            mypanel = new PanelData();

            //色有りを作成する
            //構造体初期設定
            mypanel.panel_delete_flag = false;
            //mypanel.panel_color = 4;
            mypanel.search_flag = false;
            mypanel.up_flag = false;
            mypanel.down_flag = false;
            mypanel.right_flag = false;
            mypanel.left_flag = false;
            mypanel.latest_x = 2;
            mypanel.latest_y = (int)(y);

            // プレハブを元にオブジェクトを生成する
            mypanel.panel_info = (GameObject)Instantiate(obj,
                                                new Vector3(2 + default_x,y, 0.0f),
                                                Quaternion.identity);

            //パネルに色を付ける (8/8)
           

            if (!mypanel.panel_info.Equals(null))
            {
                //パネルの初期設定を行う
                //１．RigitBody2D初期設定 
                mypanel.panel_info.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX |
                                                                    RigidbodyConstraints2D.FreezeRotation;
               redPanelList.Add(mypanel);
            }
        }

        int count = bluePanelList.Count;
        for (int i = 0; i < count; i++)
        {
            //Debug.Log("panel_color[" + i + "]=" + bluePanelList[i].panel_color);

        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //フレーム確認処理
        flamecount++;
        mytime += Time.deltaTime;

        if (flamecount % 50 == 0)
        {
            Debug.Log("50フレーム : " + (mytime - oldmytime));//フレーム毎の秒数

            oldmytime = mytime;
        }


        frameCnt++;

        if (frameCnt >= 50)
        {
            frameCnt = 0;
        }

        //フリップ処理
        
        // 左クリック時(画面タッチ時)
        if (Input.GetMouseButton(0))
        {
            mousePosition = Input.mousePosition;

            //Debug.Log("Click:mousePosition = " + mousePosition);
            
            // マウス位置座標をスクリーン座標からワールド座標に変換する 
            screenToWorldPointMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            
            //Debug.Log("Click:screenToWorldPointMousePosition = " + screenToWorldPointMousePosition);

            //新規左クリック(タッチ)処理
            if (InitTouchPos_x == 0)
            {
                //クリック(タッチ)初期位置を保持する
                InitTouchPos_x = screenToWorldPointMousePosition.x;

                //Ray取得
                ray = Camera.main.ScreenPointToRay(screenToWorldPointMousePosition);    //マウス(タップ)位置を取得する

                //指定座標のオブジェクト情報を取得
                hit = Physics2D.Raycast(screenToWorldPointMousePosition, ray.direction, 20.0f);//これが取得出来ていない

                //hit.collider.gameobjectがNullだった時、NullReferenseで処理が止まってしまう為、
                //hit.colliderでgameobjectがNull出ないことを保証するものとする。
                //また、格納した hit.collider.gameobjectは、Nullチェックすること
                if (hit.collider != null)
                {
                    swapPanelInfo = hit.collider.gameObject;       //パネル情報を取得
                    swapPanelInfo_trans_x = swapPanelInfo.transform.position.x; //パネルのx座標を取得
                    Debug.Log("パネル情報を取得 + swapPanelInfo_trans_x=" +  swapPanelInfo_trans_x);
                }
            }

            //継続左クリック(タッチ)処理
            //ドラッグ(フリック)距離を計算する
            TouchDistance_x = InitTouchPos_x - screenToWorldPointMousePosition.x;
            
            ////Ray取得
            //ray = Camera.main.ScreenPointToRay(screenToWorldPointMousePosition);    //マウス(タップ)位置を取得する

            ////指定座標のオブジェクト情報を取得
            //hit = Physics2D.Raycast(screenToWorldPointMousePosition, ray.direction, 20.0f);//これが取得出来ていない

            //Debug.Log("Click:タッチ中：TouchDistance_x = " + TouchDistance_x);
        }
        else
        {
            //クリック(タッチ)を離した時、変数を初期化する

            //Ray初期化
            hit = Physics2D.Raycast(new Vector2(-20f, -20f), ray.direction, 0.0f);

            //クリック(タッチ)初期化
            InitTouchPos_x = 0f;

            //ドラッグ(フリック)距離初期化
            TouchDistance_x = 0f;

            swapPanelInfo_trans_x = 0f;

            //Debug.Log("Click:タッチしていない ");
        }

        //パネルを交換する処理
        //☆ パネル斜め交換バグの対策は未実装

        //ドラッグ(フリック)距離が-1以下、1以上の時、フリップしたかどうかのフラグをtrueにする
        if ((TouchDistance_x <= -1f) || (1f < TouchDistance_x ))
        {
            GameObject temp = null;
            float moveX = 0f;

            //交換対象のパネル情報を取得
            ray = Camera.main.ScreenPointToRay(new Vector3(screenToWorldPointMousePosition.x, screenToWorldPointMousePosition.y, screenToWorldPointMousePosition.z));

            hit = Physics2D.Raycast(new Vector3(screenToWorldPointMousePosition.x, screenToWorldPointMousePosition.y, screenToWorldPointMousePosition.z), ray.direction, 20.0f);

            //例外処理
            //パネル中心位置 + 移動距離の絶対値どちらかが、パネルフレーム範囲外の時、例外側
            //
            if ( (swapPanelInfo_trans_x + TouchDistance_x > PanelFrameLeft_x ) 
                && (swapPanelInfo_trans_x - TouchDistance_x > PanelFrameLeft_x) )
            {
                //正常側
                Debug.Log("正常側");
                //パネルが存在しない時のみ変数に保持する
                if (hit.collider != null)
                {
                    temp = hit.collider.gameObject;
                    Debug.Log("交換対象のパネル情報を取得");
                }

                if (TouchDistance_x < 0f)
                {
                    //右方向にドラッグ(フリップ)した際
                    moveX = 1f;
                }
                else
                {
                    //左方向にドラッグ(フリップ)した際
                    moveX = -1f;
                }

                //パネルの位置情報を交換する
                if (swapPanelInfo != null)
                {
                    //中心位置を移動
                    swapPanelInfo.transform.position += new Vector3(moveX, 0, 0);
                    //フリップフラグをtrueにする
                    swapPanelInfo.GetComponent<Panel_Test>().setFlipFlag(true);
                }
                if (temp != null)
                {
                    temp.transform.position += new Vector3(-moveX, 0, 0);
                    temp.GetComponent<Panel_Test>().setFlipFlag(true);
                }
             
            }
            else
            {
                //例外側
                Debug.Log("例外側");
            }


  
            //クリック(タッチ)初期化
            InitTouchPos_x = 0f;

            //ドラッグ(フリック)距離初期化
            TouchDistance_x = 0f;

            swapPanelInfo_trans_x = 0f;
        }

        //１．せり上げ処理
        if (frameCnt % execUpPanel == 0)
        { 
            for (int y = 0; y < GRAY_PANEL_HEIGHT; y++)
            {
                for (int x = 0; x < GRAY_PANEL_WIDTH; x++)
                {
                        grayPanelArray[y, x].panel_info.transform.position += new Vector3(0, 0.03f, 0);     //1パネル33回分のせり上がりだが、一旦仮設定とする。
                }
            }
        }
        //２　サーチフラグの有効化

        //２．１　グレーパネルの有効化
        if (default_y <= grayPanelArray[grayPanelArrayTop, 0].panel_info.transform.position.y)
        { 
            for (int x = 0; x < GRAY_PANEL_WIDTH; x++)
            {
                //grayPanelArray[grayPanelArrayTop, x].panel_color = 1;   //まず無効->有効
                grayPanelArray[grayPanelArrayTop, x].search_flag = true;
                grayPanelArray[grayPanelArrayTop, x].up_flag = true;
                grayPanelArray[grayPanelArrayTop, x].down_flag = true;
                grayPanelArray[grayPanelArrayTop, x].right_flag = true;
                grayPanelArray[grayPanelArrayTop, x].left_flag = true;
                grayPanelArray[grayPanelArrayTop, x].panel_info.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX |
                                                                    RigidbodyConstraints2D.FreezeRotation;

                //パネルの色を変更する
                grayPanelArray[grayPanelArrayTop, x].panel_info.gameObject.GetComponent<SpriteRenderer>().sprite = sprite_blue;
                grayPanelArray[grayPanelArrayTop, x].panel_info.gameObject.GetComponent<Panel_Test>().setPanelColor(1);

                Debug.Log("BluePanel作成");

                //リスト内のパネルの色変更
                bluePanelList.Add(grayPanelArray[grayPanelArrayTop, x]);        //ブルーリストに追加


                //無効パネル再生成
                grayPanelArray[grayPanelArrayTop, x].panel_delete_flag = false;
                //grayPanelArray[grayPanelArrayTop, x].panel_color = 0;
                grayPanelArray[grayPanelArrayTop, x].search_flag = false;
                grayPanelArray[grayPanelArrayTop, x].up_flag = true;
                grayPanelArray[grayPanelArrayTop, x].down_flag = true;
                grayPanelArray[grayPanelArrayTop, x].right_flag = true;
                grayPanelArray[grayPanelArrayTop, x].left_flag = true;
                grayPanelArray[grayPanelArrayTop, x].latest_x = x;
                grayPanelArray[grayPanelArrayTop, x].latest_y = (int)(default_y - GRAY_PANEL_HEIGHT);

                // プレハブを元にオブジェクトを生成する
                grayPanelArray[grayPanelArrayTop, x].panel_info = (GameObject)Instantiate(obj,
                                                    new Vector3(default_x + x, default_y - GRAY_PANEL_HEIGHT, 0.0f),
                                                    Quaternion.identity);

                //パネルに色を付ける
                grayPanelArray[grayPanelArrayTop, x].panel_info.gameObject.GetComponent<SpriteRenderer>().sprite = sprite_gray;
                grayPanelArray[grayPanelArrayTop, x].panel_info.gameObject.GetComponent<Panel_Test>().setPanelColor(0);
                //Debug.Log("生成された　grayPanelArrayTop y座標確認" + grayPanelArray[grayPanelArrayTop,x].panel_info.transform.position.y);
                if (grayPanelArray[grayPanelArrayTop, x].panel_info.Equals(null))
                {
                    Debug.Log("(再生成) Error:grayPanelArray.panel_info = null y = " + grayPanelArrayTop + " x = " + x);
                }
            }
            
            grayPanelArrayTop++;
            if (grayPanelArrayTop >= GRAY_PANEL_HEIGHT)
            {
                grayPanelArrayTop = 0;
            }
        }


        //２．２　落下・交換パネルの有効化
        PanelData[] tempBluePanelArray = new PanelData[bluePanelList.Count];
        PanelData[] tempRedPanelArray = new PanelData[redPanelList.Count];
        PanelData[] tempGreenPanelArray = new PanelData[greenPanelList.Count];

        //２．２．１　青パネル
        for (int i = 0; i < bluePanelList.Count; i++)
        {
            //一時変数に格納する
            tempBluePanelArray[i] = bluePanelList[i];
            
            //下方向に落下している場合、サーチフラグをtrueにする
            if (tempBluePanelArray[i].latest_y > (int)(tempBluePanelArray[i].panel_info.transform.position.y - default_y))
            {
                //Debug.Log("パネル落下");
                //暫定：パネルが宙に浮いているかどうか->重力加速度で対応
                //2020/07/05 追記  本番では、下方向にパネルがあるかどうかで実装予定

                //if (temparray[i].panel_info.GetComponent<Rigidbody2D>().velocity.magnitude <= 0)
                //{
                    //mymagunitude = temparray[i].panel_info.GetComponent<Rigidbody2D>().velocity.magnitude;
                    //Debug.Log("mymagunitude = " + mymagunitude);
                    //Debug.Log("落下パネル有効化");
                tempBluePanelArray[i].search_flag = true;
                tempBluePanelArray[i].up_flag = true;
                tempBluePanelArray[i].down_flag = true;
                tempBluePanelArray[i].right_flag = true;
                tempBluePanelArray[i].left_flag = true;
               // }
            }

            if(tempBluePanelArray[i].search_flag)
            {
                Blue_Flag = true;
            }

            //パネルlatest_y更新 
            //Default_yを使わず、グレーパネルの中心座標を使う
            tempBluePanelArray[i].latest_y = (int)(tempBluePanelArray[i].panel_info.transform.position.y - grayPanelArray[grayPanelArrayTop, 0].panel_info.transform.position.y + margin_y);
            //Debug.Log("青色:"+ i + "番 実態：" + tempBluePanelArray[i].panel_info.transform.position.y + "整数:" + tempBluePanelArray[i].latest_y + " X=" + tempBluePanelArray[i].latest_x);
        }
        //２．２．１　赤パネル
        for (int i = 0; i < redPanelList.Count; i++)
        {
            //一時変数に格納する
            tempRedPanelArray[i] = redPanelList[i];

            //下方向に落下している場合、サーチフラグをtrueにする
            if (tempRedPanelArray[i].latest_y > (int)(tempRedPanelArray[i].panel_info.transform.position.y - default_y))
            {
                Debug.Log("パネル落下");
                //暫定：パネルが宙に浮いているかどうか->重力加速度で対応
                //2020/07/05 追記  本番では、下方向にパネルがあるかどうかで実装予定

                //if (temparray[i].panel_info.GetComponent<Rigidbody2D>().velocity.magnitude <= 0)
                //{
                //mymagunitude = temparray[i].panel_info.GetComponent<Rigidbody2D>().velocity.magnitude;
                //Debug.Log("mymagunitude = " + mymagunitude);
                Debug.Log("落下パネル有効化");
                tempRedPanelArray[i].search_flag = true;
                tempRedPanelArray[i].up_flag = true;
                tempRedPanelArray[i].down_flag = true;
                tempRedPanelArray[i].right_flag = true;
                tempRedPanelArray[i].left_flag = true;
                // }
            }

            if (tempRedPanelArray[i].search_flag)
            {
                Red_Flag = true;
            }

            //パネルlatest_y更新 
            //Default_yを使わず、グレーパネルの中心座標を使う
            tempRedPanelArray[i].latest_y = (int)(tempRedPanelArray[i].panel_info.transform.position.y - grayPanelArray[grayPanelArrayTop, 0].panel_info.transform.position.y + margin_y);
            //Debug.Log("青色:"+ i + "番 実態：" + tempBluePanelArray[i].panel_info.transform.position.y + "整数:" + tempBluePanelArray[i].latest_y + " X=" + tempBluePanelArray[i].latest_x);
        }
        //２．２．１　緑パネル
        for (int i = 0; i < greenPanelList.Count; i++)
        {
            //一時変数に格納する
            tempGreenPanelArray[i] = greenPanelList[i];

            //下方向に落下している場合、サーチフラグをtrueにする
            if (tempGreenPanelArray[i].latest_y > (int)(tempGreenPanelArray[i].panel_info.transform.position.y - default_y))
            {
                Debug.Log("パネル落下");
                //暫定：パネルが宙に浮いているかどうか->重力加速度で対応
                //2020/07/05 追記  本番では、下方向にパネルがあるかどうかで実装予定

                //if (temparray[i].panel_info.GetComponent<Rigidbody2D>().velocity.magnitude <= 0)
                //{
                //mymagunitude = temparray[i].panel_info.GetComponent<Rigidbody2D>().velocity.magnitude;
                //Debug.Log("mymagunitude = " + mymagunitude);
                Debug.Log("落下パネル有効化");
                tempGreenPanelArray[i].search_flag = true;
                tempGreenPanelArray[i].up_flag = true;
                tempGreenPanelArray[i].down_flag = true;
                tempGreenPanelArray[i].right_flag = true;
                tempGreenPanelArray[i].left_flag = true;
                // }
            }

            if (tempGreenPanelArray[i].search_flag)
            {
                Green_Flag = true;
            }

            //パネルlatest_y更新 
            //Default_yを使わず、グレーパネルの中心座標を使う
            tempGreenPanelArray[i].latest_y = (int)(tempGreenPanelArray[i].panel_info.transform.position.y - grayPanelArray[grayPanelArrayTop, 0].panel_info.transform.position.y + margin_y);
            //Debug.Log("青色:"+ i + "番 実態：" + tempBluePanelArray[i].panel_info.transform.position.y + "整数:" + tempBluePanelArray[i].latest_y + " X=" + tempBluePanelArray[i].latest_x);
        }
        //Debug.Log("赤色:7番 実態：" + redPanelList[6].panel_info.transform.position.y + "整数:" + redPanelList[6].latest_y + " X=" + redPanelList[6].latest_x);

        //３．　サーチ
        if (Blue_Flag == true)
        {
            Search(tempBluePanelArray, bluePanelList.Count);
        }
            //赤色がサーチ対象
        if (Red_Flag == true)
        {
            //ここに、サーチ処理を入れる
            Search(tempRedPanelArray, redPanelList.Count);
        }
        //緑色がサーチ対象
        if (Green_Flag == true )
        {
            //ここに、サーチ処理を入れる
            Search(tempGreenPanelArray, greenPanelList.Count);
        }

        //格納していた変数をパネルリストに戻す
        for (int i = 0; i < bluePanelList.Count; i++)
        {
            bluePanelList[i] = tempBluePanelArray[i];
        }
        //格納していた変数をパネルリストに戻す
        for (int i = 0; i < redPanelList.Count; i++)
        {
            redPanelList[i] = tempRedPanelArray[i];
        }
        //格納していた変数をパネルリストに戻す
        for (int i = 0; i < greenPanelList.Count; i++)
        {
            greenPanelList[i] = tempGreenPanelArray[i];
        }

        //削除処理

        for (int i = bluePanelList.Count - 1; i >= 0; i--)
        {
            
            if(bluePanelList[i].panel_delete_flag == true)
            {
                Debug.Log("パネル削除　座標X=" + (bluePanelList[i].latest_x) + "  座標Y=" + (bluePanelList[i].latest_y));
                Destroy(bluePanelList[i].panel_info.gameObject);
                bluePanelList.RemoveAt(i);
            }
        }
        for (int i = redPanelList.Count - 1; i >= 0; i--)
        {

            if (redPanelList[i].panel_delete_flag == true)
            {
                Debug.Log("パネル削除　座標X=" + (redPanelList[i].latest_x) + "  座標Y=" + (redPanelList[i].latest_y));
                Destroy(redPanelList[i].panel_info.gameObject);
                redPanelList.RemoveAt(i);
            }
        }
        for (int i = greenPanelList.Count - 1; i >= 0; i--)
        {

            if (greenPanelList[i].panel_delete_flag == true)
            {
                Debug.Log("パネル削除　座標X=" + (greenPanelList[i].latest_x) + "  座標Y=" + (greenPanelList[i].latest_y));
                Destroy(greenPanelList[i].panel_info.gameObject);
                greenPanelList.RemoveAt(i);
            }
        }


    }

    void Search(PanelData[] temparray,int count)
    {
        //サーチ開始(1回目)
        for (int i = 0; i < count; i++)
        {
            //サーチパネル対象の時
            if (temparray[i].search_flag == true)
            {
                BothDirectionSerach(temparray, i, BOTH_DIRECTION.UP_DOWN);
                BothDirectionSerach(temparray, i, BOTH_DIRECTION.RIGHT_LEFT);
            }
            temparray[i].search_flag = false;
        }
    }

    //上下と左右のサーチ
    void BothDirectionSerach(PanelData[] temparray, int i, BOTH_DIRECTION direction)
    {
        //1回目のサーチ対象となるパネル座標
        int myPanel_x = temparray[i].latest_x;
        int myPanel_y = temparray[i].latest_y;

        bool ReverseFlag = false;
        int ReverseNum = 0;

        bool dir_1_flag = false;
        bool dir_2_flag = false;

        SEARCH_DIRECTION dir_1 = SEARCH_DIRECTION.UP;
        SEARCH_DIRECTION dir_2 = SEARCH_DIRECTION.DOWN;

        switch (direction)
        {
        case BOTH_DIRECTION.UP_DOWN:
            dir_1 = SEARCH_DIRECTION.UP;
            dir_2 = SEARCH_DIRECTION.DOWN;
            dir_1_flag = temparray[i].up_flag;
            dir_2_flag = temparray[i].down_flag;
            //Debug.Log("縦方向探索");
            break;
        case BOTH_DIRECTION.RIGHT_LEFT:
            dir_1 = SEARCH_DIRECTION.RIGHT;
            dir_2 = SEARCH_DIRECTION.LEFT;
            //Debug.Log("横方向探索");
            dir_1_flag = temparray[i].right_flag;
            dir_2_flag = temparray[i].left_flag;
            break;
        default:
            Debug.Log("サーチ中に例外発生 配列番号：" + i + "  myPanel_x：" + myPanel_x + "  myPanel_y:" + myPanel_y);
            break;
        }

        if (dir_1_flag == true)
        {
            PanelSearch(temparray, i, ref ReverseFlag, ref ReverseNum, dir_1);
        }
        if (dir_2_flag == true)
        {
            //Debug.Log("横方向探索");
            PanelSearch(temparray, i, ref ReverseFlag, ref ReverseNum, dir_2);
        }
    }

    //１方向のサーチ
    void PanelSearch(PanelData[] temparray, int i, ref bool ReverseFlag, ref int ReverseNum, SEARCH_DIRECTION direction)
    {
        //1個目のパネル座標を取得
        int myPanel_x_next = temparray[i].latest_x;
        int myPanel_y_next = temparray[i].latest_y;

        switch(direction)
        {
            //2回目のサーチ対象となるパネル座標
            //1回目のパネルのフラグを落とす
            case SEARCH_DIRECTION.UP:
                myPanel_y_next++;
                temparray[i].up_flag = false;
                temparray[i].down_flag = false;
                break;
            case SEARCH_DIRECTION.DOWN:
                myPanel_y_next--;
                temparray[i].down_flag = false;
                temparray[i].up_flag = false;
                break;
            case SEARCH_DIRECTION.RIGHT:
                myPanel_x_next++;
                temparray[i].right_flag = false;
                temparray[i].left_flag = false;
                break;
            case SEARCH_DIRECTION.LEFT:
                myPanel_x_next--;
                temparray[i].left_flag = false;
                temparray[i].right_flag = false;
                break;
            default:
                Debug.Log("PanelSearch");
                break;
        }


        //サーチ開始(2回目)
        for (int j = 0; j < bluePanelList.Count; j++)
        {
            //自身のパネルの探査方法のパネルが青
            if ( (temparray[j].latest_x == myPanel_x_next) &&  (temparray[j].latest_y == myPanel_y_next) )
            {
                if (ReverseFlag == true)
                {
                    //1個目の反対方向のパネルの削除フラグを立てる
                    temparray[ReverseNum].panel_delete_flag = true;

                    //1個目とサーチ対象のパネル削除フラグを立てる
                    temparray[i].panel_delete_flag = true;
                    temparray[j].panel_delete_flag = true;

                    if (myPanel_y_next == 1)
                    { 
                        Debug.Log("削除On[Rev0] X:" + myPanel_x_next + "  Y:" + myPanel_y_next);
                    }
                }
                else
                {
                    ReverseFlag = true;
                    ReverseNum = j;
                    if (myPanel_y_next == 1)
                    {
                        Debug.Log("削除On[Rev1] X:" + myPanel_x_next + "  Y:" + myPanel_y_next);
                    }
                }

                //3回目のサーチは再帰関数を使用する

                //３列以上同色が続いた場合
                if (RecursionSearch(temparray, j, direction) == true)
                {
                    //1つ目、2つ目のパネルも削除対象にする
                    temparray[i].panel_delete_flag = true;
                    temparray[j].panel_delete_flag = true;
                    
                    Debug.Log("削除On[PanelSearch] next_X:" + myPanel_x_next + "next_Y:" + myPanel_y_next);
                }
            }
        }
    }

    //3回目以降の再帰サーチ
    bool RecursionSearch(PanelData[] temparray, int num, SEARCH_DIRECTION direction)
    {
        //中心パネル座標
        int myPanel_x_next = temparray[num].latest_x;
        int myPanel_y_next = temparray[num].latest_y;

        switch (direction)
        {
            //次のサーチ対象となるパネル座標
            //中心パネルのフラグを落とす
            case SEARCH_DIRECTION.UP:
                myPanel_y_next++;
                temparray[num].up_flag = false;
                temparray[num].down_flag = false;
                break;
            case SEARCH_DIRECTION.DOWN:
                myPanel_y_next--;
                temparray[num].down_flag = false;
                temparray[num].up_flag = false;
                break;
            case SEARCH_DIRECTION.RIGHT:
                myPanel_x_next++;
                temparray[num].right_flag = false;
                temparray[num].left_flag = false;
                break;
            case SEARCH_DIRECTION.LEFT:
                myPanel_x_next--;
                temparray[num].left_flag = false;
                temparray[num].right_flag = false;
                break;
            default:
                Debug.Log("サーチ中に例外発生 配列番号：" + num + " 中心パネル：" + myPanel_x_next + ":" + myPanel_y_next + " サーチ方向:" + direction);
                break;
        }
        
        bool Return_Flag = false;

        for (int i = 0; i < bluePanelList.Count; i++)
        {
            //パネル配列にサーチ対象のパネル座標が含まれていた場合
            if ((temparray[i].latest_x == myPanel_x_next) && ( temparray[i].latest_y == myPanel_y_next))
            {
                Return_Flag = true;
                temparray[i].panel_delete_flag = true;
                if (myPanel_y_next == 1)
                {
                    //Debug.Log("削除On[RecursionSearch] X:" + myPanel_x_next + "  Y:" + myPanel_y_next);
                }
                Debug.Log("サーチ開始 配列番号：" + i + "  myPanel_x_next：" + myPanel_x_next + "  myPanel_y_next:" + myPanel_y_next);
                RecursionSearch(temparray, i, direction);
            }
        }
        return Return_Flag;
    }

    //ドラッグ(フリック)距離取得
    public float GetTouchDistance_x()
    {
        return TouchDistance_x;
    }
}

