using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_Test : MonoBehaviour
{
    //パネルの色
    [SerializeField] public int  panel_color; // 0:無効 1:青色 2:緑色 3:赤色

    //フリップしたかどうかのフラグ
    [SerializeField] public bool isFlip;

    // Start is called before the first frame update
    void Start()
    {
        panel_color = 0;
        isFlip = false;
    }
    
    public void OnTriggerEnter2D(Collider2D collision)
    {
       // Debug.Log("テストEnter");
       //重力加速度
        //blueflag++;
        //落ち切ったフラグをTrueにする。
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
       // Debug.Log("テストExit");
    }

    //フリップしたかどうかのフラグをtrueにする
    public void setFlipFlag(bool flg)
    {
        Debug.Log("setFlipFlag"+ flg);
        isFlip = flg;
    }

    //パネルの色を変更する
    public void setPanelColor(int color)
    {
        panel_color = color;
    }
}
