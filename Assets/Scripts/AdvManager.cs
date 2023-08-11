using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AdvManager : MonoBehaviour
{
    [SerializeField] GameState gameState;
    float advTimer;
    float advBreak = 60f;

    [DllImport("__Internal")]
    private static extern void ShowIntersitialAdvExtern();
    [DllImport("__Internal")]
    private static extern void ShowRewardedAdvExtern();

    private void Start()
    {
        advTimer = advBreak;
    }
    private void Update()
    {
        advTimer -= Time.deltaTime;

    }

    public void ShowAdv()
    {
        if (advTimer <= 0)
        {
#if !UNITY_EDITOR
            ShowIntersitialAdvExtern();
#else
            StartTimer();
#endif
        }
    }
    //Âûחûגאועסÿ ג ShowIntersitialAdvExtern.OnClose()
    public void StartTimer()
    {
        advTimer = advBreak;
    }

    public void ShowRewardAdv()
    {

#if !UNITY_EDITOR
        ShowRewardedAdvExtern();
#else
        GiveSecondWind();
#endif
    }

    public void GiveSecondWind()
    {
        gameState.SecondWind();
    }

}
