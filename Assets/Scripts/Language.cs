using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Language : MonoBehaviour
{
    public string currentLanguage;
    public static Language Instance;

    [DllImport("__Internal")]
    private static extern string GetLang();
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

#if !UNITY_EDITOR
            currentLanguage = GetLang();
            
#endif
        }
        else
            Destroy(gameObject);
    }


}
