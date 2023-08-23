using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurrencyManager
{
    public static string ConvertMoneyToString(int money, bool includeZeros)
    {
        string playerMoneyString = "";
        int copper = money;
        int gold = copper / 10000;
        copper = copper % 10000;
        int silver = copper / 100;
        copper = copper % 100;

        if (includeZeros || gold > 0)
        {
            playerMoneyString = playerMoneyString + gold + "G ";
        }

        if (includeZeros || silver > 0)
        {
            playerMoneyString = playerMoneyString + silver + "S ";
        }

        if (includeZeros || copper > 0)
        {
            playerMoneyString = playerMoneyString + copper + "C";
        }

        return playerMoneyString;
    }
}
