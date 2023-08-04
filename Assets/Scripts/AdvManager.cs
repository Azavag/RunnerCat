using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AdvManager : MonoBehaviour
{
    [SerializeField] GameState gameState;
    bool isAdvAllowed = false;
    float advTimer;
    float advBreak = 61f;

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
        if (!isAdvAllowed && advTimer <= 0)
        {
            isAdvAllowed = true;
            Debug.Log("Можно Реклама");
            return;
            
        }
    }

    public void ShowAdv()
    {
        if (isAdvAllowed)
        {
            isAdvAllowed = false;
            advTimer = 10000;
            Debug.Log("Реклама");
#if !UNITY_EDITOR
            ShowIntersitialAdvExtern();
#else 
            StartTimer();
#endif
        }
    }

    public void StartTimer()
    {
        advTimer = advBreak;
    }

    public void ShowRewardAdv()
    {
#if !UNITY_EDITOR
        ShowRewardedAdvExtern();
#endif
    }

    public void GiveSecondWind()
    {
        gameState.SecondWind();
    }

}
