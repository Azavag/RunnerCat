using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InternationalScript : MonoBehaviour
{
    [SerializeField] string _ru;
    [SerializeField] string _en;

    private void Start()
    {
        if(Language.Instance.currentLanguage == "ru")
        {
            GetComponent<Text>().text = _ru;
        }
        else 
        {
            GetComponent<Text>().text = _en;
        }

    }
}
