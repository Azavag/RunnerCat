using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class ShopItemList : ShopList
{

    static public Consumable.ConsumableType[] s_ConsumablesTypes = System.Enum.GetValues(typeof(Consumable.ConsumableType)) as Consumable.ConsumableType[];

	public override void Populate()
    {
        m_RefreshCallback = null;
        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        for(int i = 0; i < s_ConsumablesTypes.Length; ++i)
        {
            Consumable c = ConsumableDatabase.GetConsumbale(s_ConsumablesTypes[i]);
            if(c != null)
            {
               
                prefabItem.InstantiateAsync().Completed += (op) =>
                {
                    if (op.Result == null || !(op.Result is GameObject))
                    {
                        Debug.LogWarning(string.Format("Unable to load item shop list {0}.", prefabItem.RuntimeKey));
                        return;
                    }
                   // GameObject newEntry = op.Result;
                    GameObject newEntry = Instantiate(prefab);
                    newEntry.transform.SetParent(listRoot, false);

                    ShopItemListItem itm = newEntry.GetComponent<ShopItemListItem>();

                    itm.buyButton.image.sprite = itm.buyButtonSprite;

                    
                    itm.pricetext.text = c.GetPrice().ToString();

                    if (Language.Instance.currentLanguage == "ru")
                    {
                        switch (c.GetConsumableName())
                        {
                            case "Magnet":
                                itm.nameText.text = "Магнит";
                                break;
                            case "Life":
                                itm.nameText.text = "Жизнь";
                                break;
                            case "Invincible":
                                itm.nameText.text = "Неузязвимость";
                                break;
                            case "x2":
                                itm.nameText.text = "Множитель х2";
                                break;
                        }
                    }
                    else itm.nameText.text = c.GetConsumableName();

                    if (c.GetPremiumCost() > 0)
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(true);
                        itm.premiumText.text = c.GetPremiumCost().ToString();
                    }
                    else
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(false);
                    }

                    itm.icon.sprite = c.icon;

                    itm.countText.gameObject.SetActive(true);

                    itm.buyButton.onClick.AddListener(delegate() { Buy(c); });
                    m_RefreshCallback += delegate() { RefreshButton(itm, c); };
                    RefreshButton(itm, c);
                };
            }
        }
    }

	protected void RefreshButton(ShopItemListItem itemList, Consumable c)
	{
		int count = 0;
		Progress.instance.playerInfo.consumables.TryGetValue(c.GetConsumableType(), out count);
		itemList.countText.text = count.ToString();

		if (c.GetPrice() > Progress.instance.playerInfo.coins)
		{
			itemList.buyButton.interactable = false;
			itemList.pricetext.color = new Color(0.81f, 0.24f, 0.24f);
        }
		else
		{
			itemList.pricetext.color = Color.black;
		}

		if (c.GetPremiumCost() > Progress.instance.playerInfo.premium)
		{
			itemList.buyButton.interactable = false;
			itemList.premiumText.color = new Color(0.81f, 0.24f, 0.24f);
        }
		else
		{
			itemList.premiumText.color = Color.black;
		}
	}

    public void Buy(Consumable c)
    {
        Progress.instance.playerInfo.coins -= c.GetPrice();
		Progress.instance.playerInfo.premium -= c.GetPremiumCost();
		Progress.instance.Add(c.GetConsumableType());
        Progress.instance.Save();

        Refresh();
    }
}
