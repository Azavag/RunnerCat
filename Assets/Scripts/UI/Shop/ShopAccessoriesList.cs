using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using TMPro;

public class ShopAccessoriesList : ShopList
{

    public AssetReference headerPrefab;
	[SerializeField] GameObject namePrefab;
    List<Character> m_CharacterList = new List<Character>();
    public override void Populate()
    {
        m_RefreshCallback = null;

        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        m_CharacterList.Clear();
        foreach (KeyValuePair<string, Character> pair in CharacterDatabase.dictionary)
        {
            Character c = pair.Value;

            if (c.accessories !=null && c.accessories.Length > 0)
                m_CharacterList.Add(c);
        }

        headerPrefab.InstantiateAsync().Completed += (op) =>
        {
            LoadedCharacter(op, 0);
        };
    }

    void LoadedCharacter(AsyncOperationHandle<GameObject> op, int currentIndex)
    {
        if (op.Result == null || !(op.Result is GameObject))
        {
            Debug.LogWarning(string.Format("Unable to load header {0}.", headerPrefab.RuntimeKey));
        }
        else
        {
            Character c = m_CharacterList[currentIndex];

            //GameObject header = op.Result;
			GameObject header = Instantiate(namePrefab);
            header.transform.SetParent(listRoot, false);
            ShopItemListItem itmHeader = header.GetComponent<ShopItemListItem>();
            

            if (Language.Instance.currentLanguage == "ru")
            {
                switch (c.characterName)
                {
                    case "Trash Cat":
                        itmHeader.nameText.text = "Кот";
                        break;
                    case "Rubbish Raccoon":
                        itmHeader.nameText.text = "Енот";
                        break;
                }
            }
			else itmHeader.nameText.text = c.characterName;

            prefabItem.InstantiateAsync().Completed += (innerOp) =>
            {
	            LoadedAccessory(innerOp, currentIndex, 0);
            };
        }
    }

    void LoadedAccessory(AsyncOperationHandle<GameObject> op, int characterIndex, int accessoryIndex)
    {
	    Character c = m_CharacterList[characterIndex];
	    if (op.Result == null || !(op.Result is GameObject))
	    {
		    Debug.LogWarning(string.Format("Unable to load shop accessory list {0}.", prefabItem.Asset.name));
	    }
	    else
	    {
		    CharacterAccessories accessory = c.accessories[accessoryIndex];

		    //GameObject newEntry = op.Result;
            GameObject newEntry = Instantiate(prefab);
            newEntry.transform.SetParent(listRoot, false);

		    ShopItemListItem itm = newEntry.GetComponent<ShopItemListItem>();

		    string compoundName = c.characterName + ":" + accessory.accessoryName;

		    

            if (Language.Instance.currentLanguage == "ru")
            {
				switch (accessory.accessoryName)
				{
                    case "Safety":
                        itm.nameText.text = "Каска";
						 break;
                    case "Party Hat":
                        itm.nameText.text = "Колпак";
						break;
					case "Smart":
                        itm.nameText.text = "Цилиндр";
						break;
                }
            }
			else itm.nameText.text = accessory.accessoryName;

            itm.pricetext.text = accessory.cost.ToString();
		    itm.icon.sprite = accessory.accessoryIcon;
		    itm.buyButton.image.sprite = itm.buyButtonSprite;

		    if (accessory.premiumCost > 0)
		    {
			    itm.premiumText.transform.parent.gameObject.SetActive(true);
			    itm.premiumText.text = accessory.premiumCost.ToString();
		    }
		    else
		    {
			    itm.premiumText.transform.parent.gameObject.SetActive(false);
		    }

		    itm.buyButton.onClick.AddListener(delegate()
		    {
			    Buy(compoundName, accessory.cost, accessory.premiumCost);
		    });

		    m_RefreshCallback += delegate() { RefreshButton(itm, accessory, compoundName); };
		    RefreshButton(itm, accessory, compoundName);
	    }

	    accessoryIndex++;

	    if (accessoryIndex == c.accessories.Length)
	    {//we finish the current character accessory, load the next character

		    characterIndex++;
		    if (characterIndex < m_CharacterList.Count)
		    {
			    headerPrefab.InstantiateAsync().Completed += (innerOp) =>
			    {
				    LoadedCharacter(innerOp, characterIndex);
			    };
		    }
	    }
	    else
	    {
		    prefabItem.InstantiateAsync().Completed += (innerOp) =>
		    {
			    LoadedAccessory(innerOp, characterIndex, accessoryIndex);
		    };
	    }
    }

	protected void RefreshButton(ShopItemListItem itm, CharacterAccessories accessory, string compoundName)
	{
		if (accessory.cost > Progress.instance.playerInfo.coins)
		{
			itm.buyButton.interactable = false;
			itm.pricetext.color = new Color(0.81f, 0.24f, 0.24f);
        }
		else
		{
			itm.pricetext.color = Color.black;
		}

		if (accessory.premiumCost > Progress.instance.playerInfo.premium)
		{
			itm.buyButton.interactable = false;
			itm.premiumText.color = new Color(0.81f, 0.24f, 0.24f);
        }
		else
		{
			itm.premiumText.color = Color.black;
		}

		if (Progress.instance.playerInfo.characterAccessories.Contains(compoundName))
		{
			itm.buyButton.interactable = false;
			itm.buyButton.image.sprite = itm.disabledButtonSprite;
			
            if (Language.Instance.currentLanguage == "ru")
            {
                itm.buyButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Купить";
            }
			else itm.buyButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Buy";
        }
	}



	public void Buy(string name, int cost, int premiumCost)
    {
        Progress.instance.playerInfo.coins -= cost;
		Progress.instance.playerInfo.premium -= premiumCost;
		Progress.instance.AddAccessory(name);
        Progress.instance.Save();


        Refresh();
    }
}
