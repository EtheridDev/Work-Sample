using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class Statu_GameActionOverTimeData : GenericStatuData
{
    public override Type myBehavior => typeof(Statu_GameActionOverTimeBehavior);

    [Title("Game action")]
    [InlineEditor]
    public GameAction[] OnStatuStart;
    [InlineEditor]
    public GameAction[] OnStatuEnd;
    [InlineEditor]
    public GameAction[] OnStatuTick;

    [Title("Specific paramaters")]
    public float amount = 5;


    public virtual void OnStatuStartActions(StatuManager bhv, GenericStatuData data, GenericStatuBehavior statubvh, float value = 0)
    {
        for (int i = 0; i < OnStatuStart.Length; i++)
        {
            if (OnStatuStart[i])
            {
                if(OnStatuStart[i] is StatuGameAction statuAction)
                {
                    statuAction.Action(bhv, value, data, statubvh);
                }
                else
                {
                    OnStatuStart[i].Action(bhv);
                }
            }
        }
    }

    public virtual void OnStatuEndActions(StatuManager bhv, GenericStatuData data, GenericStatuBehavior statubvh, float value = 0)
    {
        for (int i = 0; i < OnStatuEnd.Length; i++)
        {
            if (OnStatuEnd[i])
            {
                if (OnStatuEnd[i] is StatuGameAction statuAction)
                {
                    statuAction.Action(bhv, value, data, statubvh);
                }
                else
                {
                    OnStatuEnd[i].Action(bhv);
                }
            }
        }
    }

    public virtual void OnStatuTickActions(StatuManager bhv, GenericStatuData data, GenericStatuBehavior statubvh, float value = 0)
    {
        for (int i = 0; i < OnStatuTick.Length; i++)
        {
            if (OnStatuTick[i])
            {
                if (OnStatuTick[i] is StatuGameAction statuAction)
                {
                    statuAction.Action(bhv, value, data, statubvh);
                }
                else
                {
                    OnStatuTick[i].Action(bhv);
                }
            }
        }
    }
}
