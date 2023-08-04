using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ShopUI : MonoBehaviour
{
    public ConsumableDatabase consumableDatabase;

    public ShopItemList itemList;
    public ShopCharacterList characterList;
    public ShopAccessoriesList accessoriesList;
    public ShopThemeList themeList;

    [Header("UI")]
    public Text coinCounter;
    public Text premiumCounter;
    public Button cheatButton;

    protected ShopList m_OpenList;

    protected const int k_CheatCoins = 1000000;
    protected const int k_CheatPremium = 1000;


	void Start ()
    {
        Progress.Create();

        consumableDatabase.Load();
        //Комент
        //CoroutineHandler.StartStaticCoroutine(CharacterDatabase.LoadDatabase());
        //CoroutineHandler.StartStaticCoroutine(ThemeDatabase.LoadDatabase());


        m_OpenList = itemList;
        itemList.Open();
	}
	
	void Update ()
    {
        coinCounter.text = Progress.instance.playerInfo.coins.ToString();
        premiumCounter.text = Progress.instance.playerInfo.premium.ToString();
    }

    public void OpenItemList()
    {
        m_OpenList.Close();
        itemList.Open();
        m_OpenList = itemList;
    }

    public void OpenCharacterList()
    {
        m_OpenList.Close();
        characterList.Open();
        m_OpenList = characterList;
    }

    public void OpenThemeList()
    {
        m_OpenList.Close();
        themeList.Open();
        m_OpenList = themeList;
    }

    public void OpenAccessoriesList()
    {
        m_OpenList.Close();
        accessoriesList.Open();
        m_OpenList = accessoriesList;
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

	public void CloseScene()
	{
        SceneManager.UnloadSceneAsync("shop");
	    LoadoutState loadoutState = GameManager.instance.topState as LoadoutState;
	    if(loadoutState != null)
        {
            loadoutState.Refresh();
        }
	}

	public void CheatCoin()
	{

        Progress.instance.playerInfo.coins += k_CheatCoins;
		Progress.instance.playerInfo.premium += k_CheatPremium;
		Progress.instance.Save();
	}

}
