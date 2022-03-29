using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statu_ChangeModifierData : GenericStatuData
{
    public override Type myBehavior => typeof(Statu_ChangeModifierBehavior);

    [Title("Modifier list")]
    [InlineEditor]
    public AddGlobalDataModifierAction[] modifier;


    public void OnStatuStartModifier(StatuManager bhv)
    {
        for (int i = 0; i < modifier.Length; i++)
        {
            if (modifier[i])
            {
                modifier[i].Action(bhv);
            }
        }
    }

}
