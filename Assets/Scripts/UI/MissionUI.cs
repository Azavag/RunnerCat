using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MissionUI : MonoBehaviour
{
    public RectTransform missionPlace;
    public AssetReference missionEntryPrefab;
    public AssetReference addMissionButtonPrefab;
    public List<MissionEntry> missionsList; 
   [SerializeField] GameObject prefab;

    private void Start()
    {
        
    }

    public IEnumerator Open()
    {
        gameObject.SetActive(true);

        foreach (Transform t in missionPlace)
        {
            Addressables.ReleaseInstance(t.gameObject);
            Destroy(t.gameObject);
        }

        for(int i = 0; i < 3; ++i)
        {
            if (Progress.instance.playerInfo.missions.Count > i)
            {
                AsyncOperationHandle op = missionEntryPrefab.InstantiateAsync();
                yield return op;
                if (op.Result == null || !(op.Result is GameObject))
                {
                    Debug.LogWarning(string.Format("Unable to load mission entry {0}.", missionEntryPrefab.Asset.name));
                    yield break;
                }
                //MissionEntry entry = (op.Result as GameObject).GetComponent<MissionEntry>();

                MissionEntry entry = Instantiate(prefab).GetComponent<MissionEntry>();
                entry.transform.SetParent(missionPlace, false);
                entry.FillWithMission(Progress.instance.playerInfo.missions[i], this);
                missionsList.Add(entry);
            }
            else
            {
                AsyncOperationHandle op = addMissionButtonPrefab.InstantiateAsync();
                yield return op;
                if (op.Result == null || !(op.Result is GameObject))
                {
                    Debug.LogWarning(string.Format("Unable to load button {0}.", addMissionButtonPrefab.Asset.name));
                    yield break;
                }
                AdsForMission obj = (op.Result as GameObject)?.GetComponent<AdsForMission>();
                obj.missionUI = this;
                obj.transform.SetParent(missionPlace, false);
            }
        }
    }

    public void CallOpen()
    {
        gameObject.SetActive(true);
        StartCoroutine(Open());
    }

    public void Claim(MissionBase m)
    {
        Progress.instance.ClaimMission(m);

        // Rebuild the UI with the new missions
        StartCoroutine(Open());
    }

    public void Close()
    {
        //for(int i=0; i< 3; i++)
        //{
            
        //}
        missionsList.Clear();
        gameObject.SetActive(false);
    }
}
