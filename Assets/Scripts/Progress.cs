using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif

public struct HighscoreEntry : System.IComparable<HighscoreEntry>
{
	public string name;
	public int score;

	public int CompareTo(HighscoreEntry other)
	{
		// We want to sort from highest to lowest, so inverse the comparison.
		return other.score.CompareTo(score);
    }
}

[Serializable]
public class PlayerInfo
{
    public int coins = 0;
    public int premium = 0;
    //Улучшения // Inventory of owned consumables and quantity.
    public Dictionary<Consumable.ConsumableType, int> consumables =
        new Dictionary<Consumable.ConsumableType, int>();

    public List<int> consumablesType = new List<int>();
    public List<int> consumablesCount = new List<int>();

    public List<string> characters = new List<string>();    // Inventory of characters owned.
    
    public int usedCharacter = 0;                               // Currently equipped character.
    public int usedAccessory = -1;
    public List<string> characterAccessories = new List<string>();  // List of owned accessories, in the form "charName:accessoryName".
    public List<string> themes = new List<string>();                // Owned themes.
    public int usedTheme = 0;                                           // Currently used theme.
    //Рекорды
    public List<HighscoreEntry> highscores = new List<HighscoreEntry>();
    public int highScore = 0;
    // Миссии     
    public List<MissionBase> missions = new List<MissionBase>();
    public List<int> missonsType = new List<int>();
    public List<float> missonsProgress = new List<float>();
    public List<float> missonsMax = new List<float>();
    public List<int> missonsReward = new List<int>();

    public string previousName = "Your result";
  
    public bool licenceAccepted;
    public bool tutorialDone = false;

    public float masterVolume = float.MinValue, musicVolume = float.MinValue, masterSFXVolume = float.MinValue;

    //ftue = First Time User Expeerience. This var is used to track thing a player do for the first time. It increment everytime the user do one of the step
    //e.g. it will increment to 1 when they click Start, to 2 when doing the first run, 3 when running at least 300m etc.
    public int ftueLevel = 0;
    //Player win a rank ever 300m (e.g. a player having reached 1200m at least once will be rank 4)
    public int rank = 0;

    // This will allow us to add data even after production, and so keep all existing save STILL valid. See loading & saving for how it work.
    // Note in a real production it would probably reset that to 1 before release (as all dev save don't have to be compatible w/ final product)
    // Then would increment again with every subsequent patches. We kept it to its dev value here for teaching purpose. 
    static int s_Version = 12;

}



/// <summary>
/// Save data for the game. This is stored locally in this case, but a "better" way to do it would be to store it on a server
/// somewhere to avoid player tampering with it. Here potentially a player could modify the binary file to add premium currency.
/// </summary>
public class Progress : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void SaveExtern(string date);

    [DllImport("__Internal")]
    private static extern void LoadExtern();


    public PlayerInfo playerInfo;
    public static Progress m_Instance;
    public static Progress instance { get { return m_Instance; } }
    
    protected string saveFile = "";
    bool isRewarding = false;

    private void Awake()
    {
        
        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(gameObject);

            //instance.NewSave();
            if (playerInfo.characters.Count == 0)
                instance.playerInfo.characters.Add("Trash Cat");
            if (playerInfo.themes.Count == 0)
                instance.playerInfo.themes.Add("Day");
            instance.CheckMissionsCount();

#if !UNITY_EDITOR

            LoadExtern();
#endif

        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnApplicationFocus(bool focus)
    {
        Silence(!focus);
    }
    private void OnApplicationPause(bool pause)
    {
        Silence(!pause);
    }
    public void Silence(bool silence)
    {
        if (isRewarding)
            return;
        else AudioListener.pause = silence;
        
    }

    public void MuteGame()
    {
        AudioListener.pause = true;
        isRewarding = true;
    }

    public void UnmuteGame()
    {
        AudioListener.pause = false;
        isRewarding = false;
    }

 
    // Вызывается в jslib
    public void SetPlayerInfo(string value)
    {
        playerInfo = JsonUtility.FromJson<PlayerInfo>(value);

        if(playerInfo.characters.Count == 0)
            instance.playerInfo.characters.Add("Trash Cat");
        if (playerInfo.themes.Count == 0)
            instance.playerInfo.themes.Add("Day");

        //Рекорд
        HighscoreEntry entry = new HighscoreEntry();
        entry.name = "Your result";
        entry.score = instance.playerInfo.highScore;
        instance.playerInfo.highscores.Add(entry);

        //Объединение списков в словарь паверапов
        for (int i = 0; i != instance.playerInfo.consumablesType.Count; i++)
        {
            Consumable.ConsumableType type = (Consumable.ConsumableType)instance.playerInfo.consumablesType[i];
            instance.playerInfo.consumables.Add(type, instance.playerInfo.consumablesCount[i]);

        }
        //бъединение 4 списков в список миссий
        for (int i = 0; i != instance.playerInfo.missonsType.Count; i++)           
        {
            MissionBase tempMission = MissionBase.GetNewMissionFromType(
                (MissionBase.MissionType)instance.playerInfo.missonsType[i]);

            tempMission.progress = instance.playerInfo.missonsProgress[i];
            tempMission.reward = instance.playerInfo.missonsReward[i];
            tempMission.max = instance.playerInfo.missonsMax[i];
            instance.playerInfo.missions.Add(tempMission);
        }
        instance.CheckMissionsCount();
    }

    public void Save()
    {
        instance.playerInfo.missonsMax.Clear();
        instance.playerInfo.missonsProgress.Clear();
        instance.playerInfo.missonsReward.Clear();
        instance.playerInfo.missonsType.Clear();

        instance.playerInfo.consumablesCount.Clear();
        instance.playerInfo.consumablesType.Clear();
        //Раздление словаря(поверап - количество) на 2 списка
        foreach (var consKey in instance.playerInfo.consumables.Keys)
        {
            int typeAsInt = (int)consKey;
            instance.playerInfo.consumablesType.Add(typeAsInt);
            instance.playerInfo.consumablesCount.Add(instance.playerInfo.consumables[consKey]);
        }
        //Раздление списка миссий на 4 отдельных
        foreach (var mis in instance.playerInfo.missions)
        {
            instance.playerInfo.missonsType.Add((int)mis.GetMissionType());
            instance.playerInfo.missonsMax.Add(mis.max);
            instance.playerInfo.missonsProgress.Add(mis.progress);
            instance.playerInfo.missonsReward.Add(mis.reward);
        }
        //instance.playerInfo.highScore = instance.playerInfo.highscores[0].score;


        
        string jsonString = JsonUtility.ToJson(playerInfo);

#if !UNITY_EDITOR
        SaveExtern(jsonString);
#endif
    }
    

    public void Consume(Consumable.ConsumableType type)
    {
        if (!instance.playerInfo.consumables.ContainsKey(type))
            return;

            instance.playerInfo.consumables[type] -= 1;
        if(instance.playerInfo.consumables[type] == 0)
        {
                instance.playerInfo.consumables.Remove(type);
        }

        Save();
    }

    public void Add(Consumable.ConsumableType type)
    {
        if (!instance.playerInfo.consumables.ContainsKey(type))
        {
            instance.playerInfo.consumables[type] = 0;
        }

        instance.playerInfo.consumables[type] += 1;

        Save();
    }

    public void AddCharacter(string name)
    {
        instance.playerInfo.characters.Add(name);
    }

    public void AddTheme(string theme)
    {
        instance.playerInfo.themes.Add(theme);
    }

    public void AddAccessory(string name)
    {
        instance.playerInfo.characterAccessories.Add(name);
    }

    // Mission management

    // Will add missions until we reach 2 missions.
    public void CheckMissionsCount()
    {
        while ( instance.playerInfo.missions.Count < 3)
            AddMission();
    }

    public void AddMission()
    {
        int val = Random.Range(0, (int)MissionBase.MissionType.MAX);
        
        MissionBase newMission = MissionBase.GetNewMissionFromType((MissionBase.MissionType)val);
        newMission.Created();

        instance.playerInfo.missions.Add(newMission);
    }

    public void StartRunMissions(TrackManager manager)
    {
        for(int i = 0; i < instance.playerInfo.missions.Count; ++i)
        {
            instance.playerInfo.missions[i].RunStart(manager);
        }
    }

    public void UpdateMissions(TrackManager manager)
    {
        for(int i = 0; i < instance.playerInfo.missions.Count; ++i)
        {
            instance.playerInfo.missions[i].Update(manager);
        }
    }

    public bool AnyMissionComplete()
    {
        for (int i = 0; i < instance.playerInfo.missions.Count; ++i)
        {
            if (instance.playerInfo.missions[i].isComplete) return true;
        }

        return false;
    }

    public void ClaimMission(MissionBase mission)
    {
        instance.playerInfo.premium += mission.reward;

        instance.playerInfo.missions.Remove(mission);

        CheckMissionsCount();

        Save();
    }

	// High Score management

	public int GetScorePlace(int score)
	{
		HighscoreEntry entry = new HighscoreEntry();
		entry.score = score;
		entry.name = "";

		int index = instance.playerInfo.highscores.BinarySearch(entry);

		return index < 0 ? (~index) : index;
	}

	public void InsertScore(int score, string name)
	{
		HighscoreEntry entry = new HighscoreEntry();
		entry.score = score;
		entry.name = name;

        instance.playerInfo.highscores.Insert(GetScorePlace(score), entry);

        // Keep only the 10 best scores.
        while (instance.playerInfo.highscores.Count > 1)
            instance.playerInfo.highscores.RemoveAt(instance.playerInfo.highscores.Count - 1);
	}

    // File management

    static public void Create()
    {
        CoroutineHandler.StartStaticCoroutine(CharacterDatabase.LoadDatabase());
        CoroutineHandler.StartStaticCoroutine(ThemeDatabase.LoadDatabase());
        if (m_Instance == null)
		{
            
            //m_Instance = new Progress();

            //if we create the Progress, mean it's the very first call, so we use that to init the database
            //this allow to always init the database at the earlier we can, i.e. the start screen if started normally on device
            //or the Loadout screen if testing in editor

            //Комент
                     
            

        }
        

        //m_Instance.saveFile = Application.persistentDataPath + "/save.bin";

        //if (File.Exists(m_Instance.saveFile))
        //{
        //    // If we have a save, we read it.
        //    m_Instance.Read();
        //}
        //else
        //{
        //    // If not we create one with default data.
        //    NewSave();
        //}

        //m_Instance.CheckMissionsCount();
    }


    //Перенести присваивание в класс PlayerInfo
    //Посмотреть управление на телефоне
    //Посмотреть прогрузку database
	public void NewSave()
	{
        instance.playerInfo.characters.Clear();
        instance.playerInfo.themes.Clear();
        instance.playerInfo.missions.Clear();
		instance.playerInfo.characterAccessories.Clear();
		instance.playerInfo.consumables.Clear();

        instance.playerInfo.missonsMax.Clear();
        instance.playerInfo.missonsProgress.Clear();
        instance.playerInfo.missonsReward.Clear();
        instance.playerInfo.missonsType.Clear();

        instance.playerInfo.consumablesCount.Clear();
        instance.playerInfo.consumablesType.Clear();

        instance.playerInfo.usedCharacter = 0;
		instance.playerInfo.usedTheme = 0;
		instance.playerInfo.usedAccessory = -1;

        instance.playerInfo.coins = 0;
        instance.playerInfo.premium = 0;

        instance.playerInfo.characters.Add("Trash Cat");
        instance.playerInfo.themes.Add("Day");
        instance.playerInfo.musicVolume = 0;
        instance.playerInfo.masterSFXVolume = 0;
        instance.playerInfo.masterVolume = 0;
        MusicPlayer.instance.mixer.SetFloat("MasterVolume", playerInfo.masterVolume);
        MusicPlayer.instance.mixer.SetFloat("MusicVolume", playerInfo.musicVolume);
        MusicPlayer.instance.mixer.SetFloat("MasterSFXVolume", playerInfo.masterSFXVolume);
        instance.playerInfo.ftueLevel = 0;
        instance.playerInfo.rank = 0;

        CheckMissionsCount();
        instance.Save();

	}

    public void Read()
    {


        //BinaryReader r = new BinaryReader(new FileStream(saveFile, FileMode.Open));

        //int ver = r.ReadInt32();

        //if (ver < 6)
        //{
        //    r.Close();

        //    NewSave();
        //    r = new BinaryReader(new FileStream(saveFile, FileMode.Open));
        //    ver = r.ReadInt32();
        //}

        //coins = r.ReadInt32();

        //consumables.Clear();
        //int consumableCount = r.ReadInt32();
        //for (int i = 0; i < consumableCount; ++i)
        //{
        //    consumables.Add((Consumable.ConsumableType)r.ReadInt32(), r.ReadInt32());
        //}

        //// Read character.
        //characters.Clear();
        //int charCount = r.ReadInt32();
        //for (int i = 0; i < charCount; ++i)
        //{
        //    string charName = r.ReadString();

        //    if (charName.Contains("Raccoon") && ver < 11)
        //    {//in 11 version, we renamed Raccoon (fixing spelling) so we need to patch the save to give the character if player had it already
        //        charName = charName.Replace("Racoon", "Raccoon");
        //    }

        //    characters.Add(charName);
        //}

        //usedCharacter = r.ReadInt32();

        //// Read character accesories.
        //characterAccessories.Clear();
        //int accCount = r.ReadInt32();
        //for (int i = 0; i < accCount; ++i)
        //{
        //    characterAccessories.Add(r.ReadString());
        //}

        //// Read Themes.
        //themes.Clear();
        //int themeCount = r.ReadInt32();
        //for (int i = 0; i < themeCount; ++i)
        //{
        //    themes.Add(r.ReadString());
        //}

        //usedTheme = r.ReadInt32();

        //// Save contains the version they were written with. If data are added bump the version & test for that version before loading that data.
        //if (ver >= 2)
        //{
        //    premium = r.ReadInt32();
        //}

        //// Added highscores.
        //if (ver >= 3)
        //{
        //    highscores.Clear();
        //    int count = r.ReadInt32();
        //    for (int i = 0; i < count; ++i)
        //    {
        //        HighscoreEntry entry = new HighscoreEntry();
        //        entry.name = r.ReadString();
        //        entry.score = r.ReadInt32();

        //        highscores.Add(entry);
        //    }
        //}

        //// Added missions.
        //if (ver >= 4)
        //{
        //    missions.Clear();

        //    int count = r.ReadInt32();
        //    for (int i = 0; i < count; ++i)
        //    {
        //        MissionBase.MissionType type = (MissionBase.MissionType)r.ReadInt32();
        //        MissionBase tempMission = MissionBase.GetNewMissionFromType(type);

        //        tempMission.Deserialize(r);

        //        if (tempMission != null)
        //        {
        //            missions.Add(tempMission);
        //        }
        //    }
        //}

        //// Added highscore previous name used.
        //if (ver >= 7)
        //{
        //    previousName = r.ReadString();
        //}

        //if (ver >= 8)
        //{
        //    licenceAccepted = r.ReadBoolean();
        //}

        //if (ver >= 9)
        //{
        //    masterVolume = r.ReadSingle();
        //    musicVolume = r.ReadSingle();
        //    masterSFXVolume = r.ReadSingle();
        //}

        //if (ver >= 10)
        //{
        //    ftueLevel = r.ReadInt32();
        //    rank = r.ReadInt32();
        //}

        //if (ver >= 12)
        //{
        //    tutorialDone = r.ReadBoolean();
        //}

        //r.Close();
    }

    //public void Save()
    //{
  //      BinaryWriter w = new BinaryWriter(new FileStream(saveFile, FileMode.OpenOrCreate));

  //      w.Write(s_Version);
  //      w.Write(coins);

  //      w.Write(consumables.Count);
  //      foreach(KeyValuePair<Consumable.ConsumableType, int> p in consumables)
  //      {
  //          w.Write((int)p.Key);
  //          w.Write(p.Value);
  //      }

  //      // Write characters.
  //      w.Write(characters.Count);
  //      foreach (string c in characters)
  //      {
  //          w.Write(c);
  //      }

  //      w.Write(usedCharacter);

  //      w.Write(characterAccessories.Count);
  //      foreach (string a in characterAccessories)
  //      {
  //          w.Write(a);
  //      }

  //      // Write themes.
  //      w.Write(themes.Count);
  //      foreach (string t in themes)
  //      {
  //          w.Write(t);
  //      }

  //      w.Write(usedTheme);
  //      w.Write(premium);

		//// Write highscores.
		//w.Write(highscores.Count);
		//for(int i = 0; i < highscores.Count; ++i)
		//{
		//	w.Write(highscores[i].name);
		//	w.Write(highscores[i].score);
		//}

  //      // Write missions.
  //      w.Write(missions.Count);
  //      for(int i = 0; i < missions.Count; ++i)
  //      {
  //          w.Write((int)missions[i].GetMissionType());
  //          missions[i].Serialize(w);
  //      }

		//// Write name.
		//w.Write(previousName);

  //      w.Write(licenceAccepted);

		//w.Write (masterVolume);
		//w.Write (musicVolume);
		//w.Write (masterSFXVolume);

  //      w.Write(ftueLevel);
  //      w.Write(rank);

  //      w.Write(tutorialDone);

  //      w.Close();
    //}


}

// Helper class to cheat in the editor for test purpose
#if UNITY_EDITOR
public class PlayerDataEditor : Editor
{
	[MenuItem("Trash Dash Debug/Clear Save")]
    static public void ClearSave()
    {
        File.Delete(Application.persistentDataPath + "/save.bin");
    } 

    [MenuItem("Trash Dash Debug/Give 1000000 fishbones and 1000 premium")]
    static public void GiveCoins()
    {
        Progress.instance.playerInfo.coins += 1000000;
		Progress.instance.playerInfo.premium += 1000;
        Progress.instance.Save();
    }

    [MenuItem("Trash Dash Debug/Give 10 Consumables of each types")]
    static public void AddConsumables()
    {
       
        for(int i = 0; i < ShopItemList.s_ConsumablesTypes.Length; ++i)
        {
            Consumable c = ConsumableDatabase.GetConsumbale(ShopItemList.s_ConsumablesTypes[i]);
            if(c != null)
            {
                Progress.instance.playerInfo.consumables[c.GetConsumableType()] = 10;
            }
        }

        Progress.instance.Save();
    }
}
#endif