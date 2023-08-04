using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using TMPro;

public class ShopCharacterList : ShopList
{
    public override void Populate()
    {
        m_RefreshCallback = null;
        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        foreach(KeyValuePair<string, Character> pair in CharacterDatabase.dictionary)
        {
            Character c = pair.Value;
            if (c != null)
            {
                prefabItem.InstantiateAsync().Completed += (op) =>
                {
                    if (op.Result == null || !(op.Result is GameObject))
                    {
                        Debug.LogWarning(string.Format("Unable to load character shop list {0}.", prefabItem.Asset.name));
                        return;
                    }
                    //GameObject newEntry = op.Result;
                    GameObject newEntry = Instantiate(prefab);
                    newEntry.transform.SetParent(listRoot, false);

                    ShopItemListItem itm = newEntry.GetComponent<ShopItemListItem>();

                    itm.icon.sprite = c.icon;
                    
                    if(Language.Instance.currentLanguage == "ru")
                    {
                        switch (c.characterName)
                        {
                            case "Trash Cat":
                                itm.nameText.text = "Кот";
                                break;
                            case "Rubbish Raccoon":
                                itm.nameText.text = "Енот";
                                break;
                        }
                    } else itm.nameText.text = c.characterName;


                    itm.pricetext.text = c.cost.ToString();

                    itm.buyButton.image.sprite = itm.buyButtonSprite;

                    if (c.premiumCost > 0)
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(true);
                        itm.premiumText.text = c.premiumCost.ToString();
                    }
                    else
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(false);
                    }

                    itm.buyButton.onClick.AddListener(delegate() { Buy(c); });

                    m_RefreshCallback += delegate() { RefreshButton(itm, c); };
                    RefreshButton(itm, c);
                };
            }
        }
    }

	protected void RefreshButton(ShopItemListItem itm, Character c)
	{
		if (c.cost > Progress.instance.playerInfo.coins)
		{
			itm.buyButton.interactable = false;
			itm.pricetext.color = new Color(0.81f, 0.24f, 0.24f);
        }
		else
		{
			itm.pricetext.color = Color.black;
		}

		if (c.premiumCost > Progress.instance.playerInfo.premium)
		{
			itm.buyButton.interactable = false;
			itm.premiumText.color = new Color(0.81f, 0.24f, 0.24f);
        }
		else
		{
			itm.premiumText.color = Color.black;
		}

        if (Progress.instance.playerInfo.characters.Contains(c.characterName))
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



	public void Buy(Character c)
    {
        Progress.instance.playerInfo.coins -= c.cost;
		Progress.instance.playerInfo.premium -= c.premiumCost;
        Progress.instance.AddCharacter(c.characterName);
        Progress.instance.Save();


        // Repopulate to change button accordingly.
        Populate();
    }
}
