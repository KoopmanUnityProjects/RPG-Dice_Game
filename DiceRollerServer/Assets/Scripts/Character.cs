using System;

[Serializable]
public class Character
{
    // Basic Char Information
    public string characterName;
    public Enums.Clan clan;
    public bool isGm;

    // inventory
    public int money = 0;

    // Stats
    public int hp = 0;
    public int maxHp = 0;

    // attributes
    public int strength = 0;
    public int agility = 0;
    public int reflex = 0;
    public int endurance = 0;
    public int social = 0;
    public int awareness = 0;
    public int intellegence = 0;

    // skills
    public int animalhusbandry = 0;
    public int archery = 0;
    public int armorsmith = 0;
    public int athletics = 0;
    public int balancing = 0;
    public int bravery = 0;
    public int climbing = 0;
    public int dancing = 0;
    public int deception = 0;
    public int defense = 0;
    public int disguise = 0;
    public int fishing = 0;
    public int forgery = 0;
    public int herbalism = 0;
    public int hunting = 0;
    public int jumping = 0;
    public int leatherworking = 0;
    public int lifting = 0;
    public int locksmith = 0;
    public int polearms = 0;
    public int research = 0;
    public int sling = 0;
    public int stealth = 0;
    public int swordsmanship = 0;
    public int throwing = 0;
    public int unarmed = 0;
    public int warhammer = 0;
    public int weaponsmith = 0;

    // -----Tracking-----

    public int mostMoney = 0;

    // ShipCaptainCrew
    public int sccHighestBet = 0;
    public int sccBiggestWin = 0;
    public int sccBiggestLoss = 0;
    public int sccTotalBet = 0;
    public int sccTotalWonorLost = 0;
    public int sccGamesPlayed = 0;
    public int sccGamesWon = 0;
    public int sccFirstTurnWins = 0;
    public int sccTotalRolls = 0;  
}
