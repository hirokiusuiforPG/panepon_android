using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var touchCount = Input.touchCount;
        for (var i = 0; i < touchCount; i++)
        {
            var touch = Input.GetTouch(i);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // 画面に指が触れた時に行いたい処理をここに書く
                    break;
                case TouchPhase.Moved:
                    // 画面上で指が動いたときに行いたい処理をここに書く
                    break;
                case TouchPhase.Stationary:
                    // 指が画面に触れているが動いてはいない時に行いたい処理をここに書く
                    break;
                case TouchPhase.Ended:
                    // 画面から指が離れた時に行いたい処理をここに書く

                    //メニュー画面に遷移する
                    
                    break;
                case TouchPhase.Canceled:
                    // システムがタッチの追跡をキャンセルした時に行いたい処理をここに書く
                    break;
                default:
                    //throw new ArgumentOutOfRangeException();
                    break;
            }
        }
    }
}
