using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class Progress
{
    //You can check whether player completed this level with - Progress.CompletedLevels.Contains(LevelID);
    //To add completed level call - Progress.CompletedLevels.Add(LevelID);
    public static List<int> CompletedLevels = new List<int>();

    public static void Save()
    {
        BinaryFormatter form = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into Debug.Log if you want to know where save games are located
        FileStream file = File.Create(Application.persistentDataPath + "/PlayerProgress.data"); //you can call it anything you want
        form.Serialize(file, CompletedLevels);
        file.Close();
    }

    public static void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/PlayerProgress.data"))
        {
            BinaryFormatter form = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/PlayerProgress.data", FileMode.Open);
            CompletedLevels = (List<int>)form.Deserialize(file);
            file.Close();
        }
    }
}
