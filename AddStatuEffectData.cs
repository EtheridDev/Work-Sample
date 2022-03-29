using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class AddStatuEffectData : AttackSubBehaviorData
{
    public override int uniqueID => 28;

    public override Type neededBehavior => typeof(AddStatuEffectBehavior);

    [InlineEditor]
    public StatuEffectToAddData statuToAdd;
}
