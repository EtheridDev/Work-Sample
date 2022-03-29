using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatuEffectToAddData : GameData
{
    [Flags]
    public enum Type
    {
        None = 0,
        Stackable = 1,
        Addeable = 2,
        Refreshable = 4,
        Unique = 8,
    }

    [Flags]
    public enum Target
    {
        None = 0,
        Launcher = 1,
        Receiver = 2,
    }

    [InlineEditor]
    public GenericStatuData[] Effect;



}
