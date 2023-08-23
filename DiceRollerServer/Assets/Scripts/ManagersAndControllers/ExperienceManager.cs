using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public enum xpOperations
    {
        Add = 0,
        Remove,
        Set,
    }

    // level 1 is 0 min xp
    const int LEVEL_2_SKILL_EXP_REQUIREMENT = 100; // 100
    const int LEVEL_3_SKILL_EXP_REQUIREMENT = 300; // + 200
    const int LEVEL_4_SKILL_EXP_REQUIREMENT = 600; // + 300
    const int LEVEL_5_SKILL_EXP_REQUIREMENT = 1000; // + 400
    const int LEVEL_6_SKILL_EXP_REQUIREMENT = 1500; // + 500
    const int LEVEL_7_SKILL_EXP_REQUIREMENT = 2100; // + 600
    const int LEVEL_8_SKILL_EXP_REQUIREMENT = 2800; // + 700
    const int LEVEL_9_SKILL_EXP_REQUIREMENT = 3600; // + 800
    const int LEVEL_10_SKILL_EXP_REQUIREMENT = 4500; // + 900

    // level 1 is 0 min xp
    const int LEVEL_2_STAT_EXP_REQUIREMENT = 100; // 100
    const int LEVEL_3_STAT_EXP_REQUIREMENT = 500; // + 400
    const int LEVEL_4_STAT_EXP_REQUIREMENT = 1400; // + 900
    const int LEVEL_5_STAT_EXP_REQUIREMENT = 3000; // + 1600
    const int LEVEL_6_STAT_EXP_REQUIREMENT = 5500; // + 2500
    const int LEVEL_7_STAT_EXP_REQUIREMENT = 9100; // + 3600
    const int LEVEL_8_STAT_EXP_REQUIREMENT = 14000; // + 4900
    const int LEVEL_9_STAT_EXP_REQUIREMENT = 20400; // + 6400
    const int LEVEL_10_STAT_EXP_REQUIREMENT = 28500; // + 8100

    public float xpMultiplier = 1.0f;

    private int unknown;

    public int GetLevel(Player player, Enums.Stats stat)
    {
        return GetStatLevel(GetStat(player, stat));
    }

    public int GetLevel(Player player, Enums.Skills skill)
    {
        return GetSkillLevel(GetSkill(player, skill));
    }

    public void ChangeXP(Player player, Enums.Stats stat, xpOperations operation, int xpAmount)
    {
        GetStat(player, stat) = RunXpOperation(operation, xpAmount, GetStat(player, stat));
    }

    // Runs on server
    public void ChangeXP(Player player, Enums.Skills skill, xpOperations operation, int xpAmount)
    {
        GetSkill(player, skill) = RunXpOperation(operation, xpAmount, GetSkill(player, skill));
    }

    public int GetXP(Player player, Enums.Skills skill)
    {
        return GetSkill(player, skill);
    }

    public int GetXP(Player player, Enums.Stats stat)
    {
        return GetStat(player, stat);
    }

    private int RunXpOperation(xpOperations operation, int xpchange, int currentXP)
    {
        int xpMultiplied = (int) (xpchange * xpMultiplier);
        switch (operation)
        {
            case xpOperations.Add:

                return currentXP + xpMultiplied;

            case xpOperations.Remove:
                int totalXP = currentXP - xpMultiplied;
                if (totalXP < 0)
                {
                    totalXP = 0;
                }
                return totalXP;

            case xpOperations.Set:
                return xpchange;

            default:
                Debug.Log("Failed to identify xpOperation " + operation.ToString());
                return currentXP;
        }
    }

    private int GetSkillLevel(int skillExp)
    {
        if (skillExp < LEVEL_2_SKILL_EXP_REQUIREMENT)
            return 1;
        if (skillExp < LEVEL_3_SKILL_EXP_REQUIREMENT)
            return 2;
        if (skillExp < LEVEL_4_SKILL_EXP_REQUIREMENT)
            return 3;
        if (skillExp < LEVEL_5_SKILL_EXP_REQUIREMENT)
            return 4;
        if (skillExp < LEVEL_6_SKILL_EXP_REQUIREMENT)
            return 5;
        if (skillExp < LEVEL_7_SKILL_EXP_REQUIREMENT)
            return 6;
        if (skillExp < LEVEL_8_SKILL_EXP_REQUIREMENT)
            return 7;
        if (skillExp < LEVEL_9_SKILL_EXP_REQUIREMENT)
            return 8;
        if (skillExp < LEVEL_10_SKILL_EXP_REQUIREMENT)
            return 9;

        return 10;
    }

    private int GetStatLevel(int statXP)
    {
        if (statXP < LEVEL_2_STAT_EXP_REQUIREMENT)
            return 1;
        if (statXP < LEVEL_3_STAT_EXP_REQUIREMENT)
            return 2;
        if (statXP < LEVEL_4_STAT_EXP_REQUIREMENT)
            return 3;
        if (statXP < LEVEL_5_STAT_EXP_REQUIREMENT)
            return 4;
        if (statXP < LEVEL_6_STAT_EXP_REQUIREMENT)
            return 5;
        if (statXP < LEVEL_7_STAT_EXP_REQUIREMENT)
            return 6;
        if (statXP < LEVEL_8_STAT_EXP_REQUIREMENT)
            return 7;
        if (statXP < LEVEL_9_STAT_EXP_REQUIREMENT)
            return 8;
        if (statXP < LEVEL_10_STAT_EXP_REQUIREMENT)
            return 9;

        return 10;
    }

    private ref int GetStat(Player player, Enums.Stats stat)
    {
        Character character = player.playersCharacter;
        switch (stat)
        {
            case Enums.Stats.strength:
                return ref character.strength;
            case Enums.Stats.agility:
                return ref character.agility;
            case Enums.Stats.reflex:
                return ref character.reflex;
            case Enums.Stats.endurance:
                return ref character.endurance;
            case Enums.Stats.social:
                return ref character.social;
            case Enums.Stats.awareness:
                return ref character.awareness;
            case Enums.Stats.intellegence:
                return ref character.intellegence;
            default:
                Debug.Log("Failed to identify Stat " + stat.ToString());
                return ref unknown;
        }
    }

    private ref int GetSkill(Player player, Enums.Skills skill)
    {
        Character character = player.playersCharacter;
        switch (skill)
        {
            case Enums.Skills.animalhusbandry:
                return ref character.animalhusbandry;
            case Enums.Skills.archery:
                return ref character.archery;
            case Enums.Skills.armorsmith:
                return ref character.armorsmith;
            case Enums.Skills.athletics:
                return ref character.athletics;
            case Enums.Skills.awareness:
                return ref character.awareness;
            case Enums.Skills.balancing:
                return ref character.balancing;
            case Enums.Skills.bravery:
                return ref character.bravery;
            case Enums.Skills.climbing:
                return ref character.climbing;
            case Enums.Skills.dancing:
                return ref character.dancing;
            case Enums.Skills.deception:
                return ref character.deception;
            case Enums.Skills.defense:
                return ref character.defense;
            case Enums.Skills.disguise:
                return ref character.disguise;
            case Enums.Skills.endurance:
                return ref character.endurance;
            case Enums.Skills.fishing:
                return ref character.fishing;
            case Enums.Skills.forgery:
                return ref character.forgery;
            case Enums.Skills.herbalism:
                return ref character.herbalism;
            case Enums.Skills.hunting:
                return ref character.hunting;
            case Enums.Skills.jumping:
                return ref character.jumping;
            case Enums.Skills.leatherworking:
                return ref character.leatherworking;
            case Enums.Skills.lifting:
                return ref character.lifting;
            case Enums.Skills.locksmith:
                return ref character.locksmith;
            case Enums.Skills.polearms:
                return ref character.polearms;
            case Enums.Skills.research:
                return ref character.research;
            case Enums.Skills.sling:
                return ref character.sling;
            case Enums.Skills.stealth:
                return ref character.stealth;
            case Enums.Skills.swordsmanship:
                return ref character.swordsmanship;
            case Enums.Skills.throwing:
                return ref character.throwing;
            case Enums.Skills.unarmed:
                return ref character.unarmed;
            case Enums.Skills.warhammer:
                return ref character.warhammer;
            case Enums.Skills.weaponsmith:
                return ref character.weaponsmith;
            default:
                Debug.Log("Failed to identify Skill " + skill.ToString());
                return ref unknown;
        }
    }
}
