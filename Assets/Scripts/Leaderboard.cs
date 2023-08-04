using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using SimpleJSON;
using System;
using UnityEngine.SocialPlatforms.Impl;
using System.Threading;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] GameObject[] otherPlayersEntries;
    [SerializeField] GameObject playerEntry, allEntries, alertAuth, leaderboardObject;

    Transform scoreTextObj, nameTextObj;

    public event Action<string> LeaderBoardReady;

    [DllImport("__Internal")]
    private static extern void ShowLeaderBoard();

    [DllImport("__Internal")]
    private static extern void Auth();


    [DllImport("__Internal")]
    private static extern void CheckAuth();


    private void Awake()
    {
        gameObject.transform.parent = null;
    }
    void Start()
    {
        
        UpdateLeaderBoard();
        LeaderBoardReady += GetEntries;

    }
    

    public void UpdateLeaderBoard()
    {
#if !UNITY_EDITOR
            ShowLeaderBoard();                     
#endif
    }


    public void BoardEntriesReady(string _data)
    {
        LeaderBoardReady?.Invoke(_data);
    }
    //По кнопке
    public void MakeAuth()
    {
#if !UNITY_EDITOR
        Auth();
#endif
    }
    //Вызывается в jslib
    public void OpenAuthAlert()
    {
        allEntries.SetActive(false);
        alertAuth.SetActive(true);
    }

    public void OpenEntries()
    {       
        alertAuth.SetActive(false);
        allEntries.SetActive(true);
    }
    //В jslib
    public void CloseAuthWindow()
    {
        leaderboardObject.SetActive(false);
    }

    public void GetEntries(string jsonEntries)
    {
#if !UNITY_EDITOR
        CheckAuth();
#endif
        var json = JSON.Parse(jsonEntries);
        var userRank = json["userRank"].ToString();
        //Если userScore = 0, То выводить -
        if (userRank == "0")
            userRank = "-";
        var count = (int)json["entries"].Count;


        for (int i = 0; i < count; i++) 
        {
            var score = json["entries"][i]["score"].ToString();
            var name = json["entries"][i]["player"]["publicName"];

            string strName = name.ToString();
            strName = strName.Trim(new char[] {'\"', '\'' });
            
            for (int index = 0; index < strName.Length; index++)
            {
                if (strName[index] == ' ')
                {
                    strName = strName.Substring(0, index + 2) + ".";
                    
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(strName))
                strName = "Неизвестный пользователь";

            nameTextObj = otherPlayersEntries[i].transform.Find("EntryBackground/Name");
            scoreTextObj = otherPlayersEntries[i].transform.Find("EntryBackground/Score");

            nameTextObj.GetComponent<Text>().text = strName;
            scoreTextObj.GetComponent<Text>().text = score;
        }

        playerEntry.transform.Find("EntryBackground/Name").GetComponent<Text>().text = "ВЫ";
        playerEntry.transform.Find("EntryBackground/Score").
            GetComponent<Text>().text = Progress.instance.playerInfo.highScore.ToString();
        playerEntry.transform.Find("EntryBackground/PlaceBack/PlaceText").GetComponent<Text>().text = userRank;

    }

}
