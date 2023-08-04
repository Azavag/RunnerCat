using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StartButton : MonoBehaviour
{
    public void StartGame()
    {
        if (Progress.instance.playerInfo.ftueLevel == 0)
        {
            Progress.instance.playerInfo.ftueLevel = 1;
            Progress.instance.Save();

        }

        SceneManager.LoadScene("main");
    }
}
