using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statu_DispelStatu_Action : StatuGameAction
{
    public float timeDelay;
    public bool cancelAll;
    public int amount;

    private float _amount = 0;


    public override void Action(MonoBehaviour actionCaller)
    {
        _amount = amount;

        if (timeDelay == 0)
            CancelStatusEffects(actionCaller);
        else
            Magic.Helpers.MagicUtility.RunAfter(() => CancelStatusEffects(actionCaller), timeDelay);
    }

    public override void Action(MonoBehaviour actionCaller, float value, GenericStatuData data, GenericStatuBehavior statubvh)
    {
        _amount = value;
        if (timeDelay == 0)
            CancelStatusEffects(actionCaller, data, statubvh);
        else
            Magic.Helpers.MagicUtility.RunAfter(() => CancelStatusEffects(actionCaller, data, statubvh), timeDelay);
    }

    private void CancelStatusEffects(MonoBehaviour actionCaller, GenericStatuData data = null, GenericStatuBehavior statubvh = null)
    {
        StatuManager statuManager = null;
        if (actionCaller.transform.root.GetComponent<MagicCustomController>().TryGetSpecificBehavior(ref statuManager))
        {
            if (cancelAll)
            {
                for (int i = 0; i < 99999; i++)
                {
                    if (i > statuManager.allEffects.Count - 1)
                        break;

                    statuManager.allEffects[i].OnStatuEnd(true);
                }
            }
            else
            {
                for (int i = 0; i < _amount; i++)
                {
                    if (i > statuManager.allEffects.Count - 1)
                        break;

                    statuManager.allEffects[i].OnStatuEnd(true);
                }
            }
        }
    }
}
