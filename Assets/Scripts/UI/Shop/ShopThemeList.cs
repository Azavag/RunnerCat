using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using TMPro;


public class ShopThemeList : ShopList
{
    public override void Populate()
    {
        m_RefreshCallback = null;
        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        foreach (KeyValuePair<string, ThemeData> pair in ThemeDatabase.dictionnary)
        {
            ThemeData theme = pair.Value;
            if (theme != null)
            {
                prefabItem.InstantiateAsync().Completed += (op) =>
                {
                    if (op.Result == null || !(op.Result is GameObject))
                    {
                        Debug.LogWarning(string.Format("Unable to load theme shop list {0}.", prefabItem.Asset.name));
                        return;
                    }
                    //GameObject newEntry = op.Result;
                    GameObject newEntry = Instantiate(prefab);
                    newEntry.transform.SetParent(listRoot, false);

                    ShopItemListItem itm = newEntry.GetComponent<ShopItemListItem>();
                    
                    if (Language.Instance.currentLanguage == "ru")
                    {
                        switch (theme.themeName)
                        {
                            case "Day":
                                itm.nameText.text = "День";
                                break;
                            case "NightTime":
                                itm.nameText.text = "Ночь";
                                break;

                        }
                    }
                    else itm.nameText.text = theme.themeName;
  

                    itm.pricetext.text = theme.cost.ToString();
                    itm.icon.sprite = theme.themeIcon;

                    if (theme.premiumCost > 0)
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(true);
                        itm.premiumText.text = theme.premiumCost.ToString();
                    }
                    else
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(false);
                    }

                    itm.buyButton.onClick.AddListener(delegate() { Buy(theme); });

                    itm.buyButton.image.sprite = itm.buyButtonSprite;

                    RefreshButton(itm, theme);
                    m_RefreshCallback += delegate() { RefreshButton(itm, theme); };
                };
            }
        }
    }

	protected void RefreshButton(ShopItemListItem itm, ThemeData theme)
	{
		if (theme.cost > Progress.instance.playerInfo.coins)
		{
			itm.buyButton.interactable = false;
			itm.pricetext.color = new Color(0.81f, 0.24f, 0.24f);
		}
		else
		{
			itm.pricetext.color = Color.black;
		}

		if (theme.premiumCost > Progress.instance.playerInfo.premium)
		{
			itm.buyButton.interactable = false;
			itm.premiumText.color = new Color(0.81f, 0.24f, 0.24f);
        }
		else
		{
			itm.premiumText.color = Color.black;
		}

		if (Progress.instance.playerInfo.themes.Contains(theme.themeName))
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


	public void Buy(ThemeData t)
    {
        Progress.instance.playerInfo.coins -= t.cost;
		Progress.instance.playerInfo.premium -= t.premiumCost;
        Progress.instance.AddTheme(t.themeName);
        Progress.instance.Save();


        // Repopulate to change button accordingly.
        Populate();
    }
}
