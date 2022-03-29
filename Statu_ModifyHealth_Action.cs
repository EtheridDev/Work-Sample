using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statu_ModifyHealth_Action : StatuGameAction
{
    public float timeDelay;
    public float amount = 5f;

    private float _amount = 0;

    public override void Action(MonoBehaviour actionCaller)
    {
        _amount = amount;
        if (timeDelay == 0)
            ModifyHealth(actionCaller);
        else
            Magic.Helpers.MagicUtility.RunAfter(() => ModifyHealth(actionCaller), timeDelay);
    }

    public override void Action(MonoBehaviour actionCaller, float value, GenericStatuData data, GenericStatuBehavior statubvh) 
    {
        _amount = value;
        if (timeDelay == 0)
            ModifyHealth(actionCaller, data, statubvh);
        else
            Magic.Helpers.MagicUtility.RunAfter(() => ModifyHealth(actionCaller, data, statubvh), timeDelay);
    }

    private void ModifyHealth(MonoBehaviour actionCaller, GenericStatuData data = null, GenericStatuBehavior statubvh = null)
    {
        ClassicHealthManager healthManager = null;
        if(actionCaller.transform.root.GetComponent<MagicCustomController>().TryGetSpecificBehavior(ref healthManager))
        {
            StatuManager statuManager = null;
            healthManager.controller.TryGetSpecificBehavior(ref statuManager);
            if (Mathf.Abs(_amount) >= healthManager.CurrentLife && data != null && data.cantBeLethal)
            {
                if (statuManager.allEffects.Contains(statubvh))
                    statuManager.allEffects.Remove(statubvh);

                if (statuManager.allEffectsOverTime.Contains(statubvh))
                    statuManager.allEffectsOverTime.Remove(statubvh);
               
                return;
            }

            if(_amount > 0)
                healthManager.AddHealth(_amount);
            if (_amount < 0)
                healthManager.ApplyDamage(Mathf.Abs(_amount));

            if(healthManager.CurrentLife <= 0)
            {
                DamageReceiver damageReceiver = null;
                if(healthManager.controller.TryGetSpecificBehavior(ref damageReceiver))
                {
                    damageReceiver.ReactToDeath();

                    if (statuManager)
                    {
                        if (statuManager.allEffects.Contains(statubvh))
                            statuManager.allEffects.Remove(statubvh);

                        if (statuManager.allEffectsOverTime.Contains(statubvh))
                            statuManager.allEffectsOverTime.Remove(statubvh);

                    }

                }
            }
        }
    }
}
