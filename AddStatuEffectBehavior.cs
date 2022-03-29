using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatuEffectBehavior : GenericAttackBehavior
{
    private StatuManager statuManager;
    private List<StatuEffectToAddData> savedData = new List<StatuEffectToAddData>();


    public override void OnAtkStart(AttackSubBehaviorData data, Attack atk)
    {
        AddStatuEffectData statuData = data as AddStatuEffectData;
        if(statuManager == null)
        {
            controller.TryGetSpecificBehavior(ref statuManager);
        }

        if(statuManager != null)
        {
            for (int i = 0; i < statuData.statuToAdd.Effect.Length; i++)
            {
                if (statuData.statuToAdd.Effect[i].target.HasFlag(StatuEffectToAddData.Target.Launcher))
                {
                    if(statuData.statuToAdd.Effect[i].delay <= 0 )
                        statuManager.RegisterEffect(statuData.statuToAdd, i);
                    else
                        ApplyEffect(statuData.statuToAdd, i);
                }

                if (statuData.statuToAdd.Effect[i].target.HasFlag(StatuEffectToAddData.Target.Receiver))
                {
                    if (statuData.statuToAdd.Effect[i].delay <= 0)
                        savedData.Add(statuData.statuToAdd);
                    else
                        Magic.Helpers.MagicUtility.RunAfter(() => savedData.Add(statuData.statuToAdd), statuData.statuToAdd.Effect[i].delay);

                    if(statuData.statuToAdd.Effect[i].displayMessageOnLauncher)
                    {
                        statuManager.ShowMessageOnLauncher(statuData.statuToAdd, i);
                    }
                }

                float durationLauncher = 0;
                if(statuData.statuToAdd.Effect[i].useDifferentTimerForSavedEffect)
                    durationLauncher = statuData.statuToAdd.Effect[i].savedEffectTimer;
                else
                    durationLauncher = statuData.statuToAdd.Effect[i].duration;

                Magic.Helpers.MagicUtility.RunAfter(() => cleanStatuEffect(statuData.statuToAdd), durationLauncher);
            }
        }
    }

    private void ApplyEffect(StatuEffectToAddData statuData, int index)
    {
        Magic.Helpers.MagicUtility.RunAfter(() => statuManager.RegisterEffect(statuData, index), statuData.Effect[index].delay);
    }

    private void cleanStatuEffect(StatuEffectToAddData data)
    {
        if (savedData.Contains(data))
            savedData.Remove(data);
    }

    public List<StatuEffectToAddData> getSavedStatuData()
    {
        return savedData;
    }

}
