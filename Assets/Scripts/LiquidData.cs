using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidInfo
{
    public string preview;

    public LiquidInfo(string preview)
    {
        this.preview= preview;
    }
}

public static class LiquidData
{
    public static List<LiquidInfo> allLiquids = new List<LiquidInfo> {
        new LiquidInfo("preview/preview_water")
    };
}
