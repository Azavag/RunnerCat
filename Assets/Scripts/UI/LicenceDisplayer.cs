using UnityEngine;

public class LicenceDisplayer : MonoBehaviour
{
    void Start ()
    {
        Progress.Create();

	    if(Progress.instance.playerInfo.licenceAccepted)
        {
            // If we have already accepted the licence, we close the popup, no need for it.
            Close();
        }	
	}
	
	public void Accepted()
    {
        Progress.instance.playerInfo.licenceAccepted = true;
        Progress.instance.Save();
        Close();
    }

    public void Refuse()
    {
        Application.Quit();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
