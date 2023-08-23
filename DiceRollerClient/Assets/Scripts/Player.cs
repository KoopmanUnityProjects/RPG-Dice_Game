using UnityEngine;

public class Player : MonoBehaviour
{
    public int Id { get; private set; }

    private string userName;
    private int userNumber;

    public Character character;

    private void Start()
    {
        character = new Character();
    }

    public void SetUsername(string name)
    {
        userName = name;
    }

    public void SetUserNumber(int number)
    {
        userNumber = number;
    }

    public string GetUserName()
    {
        return userName;
    }

    public int GetUserNumber()
    {
        return userNumber;
    }
}