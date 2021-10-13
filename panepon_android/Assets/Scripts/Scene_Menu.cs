using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_Menu : MonoBehaviour
{
    GameObject canvas;
    GameObject canvas1;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Scene_Menu start");

        canvas = GameObject.Find("Canvas");
        canvas1 = GameObject.Find("Canvas1");

        if (canvas == null)
        {
            Debug.Log("canva is null");
        }

        if (canvas1 == null)
        {
            Debug.Log("canva1 is null");
        }
        canvas.SetActive(true);
        canvas1.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SceneCanvasButton()
    {
        canvas.SetActive(false);
        canvas1.SetActive(true);
    }

    // 状態2 -> 状態1
    public void SceneCanvas1Button()
    { 
        canvas.SetActive(true);
        canvas1.SetActive(false);
    }

    // リセット
    public void ResetCanvasButton()
    {
        canvas.SetActive(true);
        canvas1.SetActive(true);
    }
}
