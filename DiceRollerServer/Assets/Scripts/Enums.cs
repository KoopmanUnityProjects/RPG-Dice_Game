using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums
{
    public enum DieColor
    {
        Black = 0,
        Blue,
        Green,
        Red,
        White,
        Yellow,

        // keep this on the bottom
        NumberOfColors
    }

    public enum ShipCaptainCrewValue
    {
        Ship = 0,
        Captain,
        Crew,
        Wench
    }

    public enum ShipCaptainCrewTurnState
    {
        ReadyToRoll = 0,
        Rolling,
        Rerolling,
        SelectingDice,
        EndTurn,
    }


    public enum Clan
    {
        // Must stay in order

        // Major Clans
        Lion = 0,
        Pig,
        Rabbit,
        Raccoon,
        Rat,
        Tiger,
        Wolf,

        // Minor Clans
        Bear,

        // No Clan 
        Unalligned,

        // Must stay at the buttom
        NumberOfClans
    }

    public enum Stats
    {
        strength = 0,
        dexterity,
        agility,
        reflex,
        endurance,
        social,
        awareness,
        intellegence,

        // keep this on the bottom
        NumberOfStats
    }

    public enum Skills
    {
        animalhusbandry = 0,
        archery,
        armorsmith,
        athletics,
        awareness,
        balancing,
        bravery,
        climbing,
        dancing,
        deception,
        defense,
        disguise,
        endurance,
        fishing,
        forgery,
        herbalism,
        hunting,
        jumping,
        leatherworking,
        lifting,
        locksmith,
        polearms,
        research,
        sling,
        stealth,
        swordsmanship,
        throwing,
        unarmed,
        warhammer,
        weaponsmith,

        // keep this on the bottom
        NumberOfSkills
    }
}
