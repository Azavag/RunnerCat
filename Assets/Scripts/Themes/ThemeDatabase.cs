using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

// Handles loading data from the Asset Bundle to handle different themes for the game
public class ThemeDatabase : MonoBehaviour
{
    static protected Dictionary<string, ThemeData> themeDataDict;
    static public Dictionary<string, ThemeData> dictionnary { get { return themeDataDict; } }

    static protected bool m_Loaded = false;
    static public bool loaded { get { return m_Loaded; } }

    [SerializeField] public ThemeData[] themeArr = new ThemeData[2];
    private void Start()
    {
        //if (themeDataDict == null)
        //{
        //    themeDataDict = new Dictionary<string, ThemeData>();
        //    foreach (var t in themeArr)
        //    {
        //        if (!themeDataDict.ContainsKey(t.themeName))
        //            themeDataDict.Add(t.themeName, t);
        //    }
        //}
        //m_Loaded = true;
    }
    static public ThemeData GetThemeData(string type)
    {
        ThemeData list;
        if (themeDataDict == null || !themeDataDict.TryGetValue(type, out list))
            return null;

        return list;
    }
    //Загрузка
    static public IEnumerator LoadDatabase()
    {
        // If not null the dictionary was already loaded.
        if (themeDataDict == null)
        {
            themeDataDict = new Dictionary<string, ThemeData>();


            yield return Addressables.LoadAssetsAsync<ThemeData>("themeData", op =>
            {
                if (op != null)
                {
                    if (!themeDataDict.ContainsKey(op.themeName))
                        themeDataDict.Add(op.themeName, op);
                }
            });

            m_Loaded = true;
        }

    }

}
