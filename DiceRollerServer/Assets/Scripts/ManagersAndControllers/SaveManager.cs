using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public void Save(Player player)
    {
        try
        {
            string filepath = Application.persistentDataPath + "/" + player.Username + player.characterNumber + ".dat";
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(filepath);
            bf.Serialize(file, player.playersCharacter);
            file.Close();
            Debug.Log($"Saved File {filepath}");
        }
        catch (IOException)
        {
            Debug.Log("failed To save");
            StartCoroutine("TryToSaveCharacterAgain", player);
        }
    }

    IEnumerator TryToSaveCharacterAgain(Player player)
    {
        yield return new WaitForSeconds(2.0f);
        try
        {
            string filepath = Application.persistentDataPath + "/" + player.Username + player.characterNumber + ".dat";
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(filepath);
            bf.Serialize(file, player.playersCharacter);
            file.Close();
            Debug.Log("Saved Player " + player.Username);
        }
        catch (IOException)
        {
            Debug.Log("failed To save");
            StartCoroutine("TryToSavePlayerAgain");
        }
    }

    public bool LoadCharacter(Player player)
    {
        string characterName = player.Username;
        int characterNumber = player.characterNumber;
        string filepath = Application.persistentDataPath + "/" + characterName + characterNumber + ".dat";
        BinaryFormatter bf = new BinaryFormatter();
        Character character = null;
        if (File.Exists(filepath))
        {
            FileStream file = File.Open(filepath, FileMode.Open);
            character = (Character)bf.Deserialize(file);
            Debug.Log("Loaded Player " + character.characterName);
            file.Close();
            if (character != null)
            {
                player.playersCharacter = character;               
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        //if (player.tribe == CharacterManager.CharacterTribe.Unassigned)
        //{
        //    TODO send character through character creation.
        //}

        // TODO Remove this later when its not just ship captain crew
        if (player.playersCharacter.money <= 0)
        {
            player.playersCharacter.money = 10000;
        }

        if (player.playersCharacter.characterName == "LocalBlaze")
        {
            player.SetIsGM(true);
        }

        return true;
    }
}
