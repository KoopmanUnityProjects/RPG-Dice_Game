public static class Enums
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
        NumberOfClans,
    }

    public enum Statistics
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

    public enum Elements
    {
        // Must stay in order
        Leather_Working = 0,

        // Must stay at the buttom
        NumberOfElements,
    }

    public enum Spells
    {
        // Must stay in order
        Leather_Working = 0,

        // Must stay at the buttom
        NumberOfSpells,
    }

}
