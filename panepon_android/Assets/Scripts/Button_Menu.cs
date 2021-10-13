using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button_Menu : Scene_Menu
{
    public void TitleButton()
    {
        ResetCanvasButton();
        //スタート画面に遷移
        SceneManager.LoadScene("title");
    }
    public void ConfigButton()
    {
        ResetCanvasButton();
        //スタート画面に遷移
        SceneManager.LoadScene("config");
    }
    public void GameplayButton()
    {
        ResetCanvasButton();
        //スタート画面に遷移
        SceneManager.LoadScene("gameplay");
    }
    // 状態1 -> 状態2
    public void CanvasButton()
    {
        SceneCanvasButton();
    }

    // 状態2 -> 状態1
    public void Canvas1Button()
    {
        SceneCanvas1Button();
    }
}
