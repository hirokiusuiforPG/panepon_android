using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button_Title : MonoBehaviour
{
    public void MenuButton()
    {
        //フィールド画面に遷移
        SceneManager.LoadScene("menu");
    }
}
