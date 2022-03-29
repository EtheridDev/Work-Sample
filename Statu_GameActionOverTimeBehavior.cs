using Packages.MagicLoading.Runtime.Loading;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statu_GameActionOverTimeBehavior : GenericStatuBehavior
{
    public override Type myType => typeof(Statu_GameActionOverTimeBehavior);

    public override float Timer => countDown;
    public override float TimerRatio => countDown / maxDuration;

    private GenericStatuData effectData;
    private Statu_GameActionOverTimeData specialData;
    
    private float tickRatecountDown = 0;
    private float maxDuration = 0;
    private float amount = 0;

    public override void Setup(GenericStatuData data, float specificStartTime = 0)
    {
        effectData = data;
        specialData = data as Statu_GameActionOverTimeData;
        countDown = specificStartTime;
        tickRatecountDown = 0;
        maxDuration = effectData.duration;
        amount = specialData.amount;
    }

    public override void OnStatuStart()
    {
        specialData.OnStatuStartActions(manager, effectData, this, amount);
        effectData.OnStatuStartFeedbacks(manager);

        if (effectData.duration <= 0)
            OnStatuEnd();

        if(effectData.target.HasFlag(StatuEffectToAddData.Target.Receiver))
        {
            if(effectData.displayMessageOnReceiverApplication)
            {
                GameObject go = PoolManager.InstantiateAtPos(effectData.displayValuePrefab, manager.controller.transform.position
                    + new Vector3(UnityEngine.Random.Range(effectData.textXOffset.x, effectData.textXOffset.y), UnityEngine.Random.Range(effectData.textYOffset.x, effectData.textYOffset.y), 0f), Quaternion.identity).instance;
                go.SetActive(true);
                OnHitDisplayDamageBehaviour bvh = go.GetComponent<OnHitDisplayDamageBehaviour>();

                bvh.SetDamageText(effectData.messageToAddOnReceiver);


                bvh.SetColorDamage(effectData.displayTextColor);

                if (effectData.spriteImage != null && effectData.showIcon)
                    bvh.SetSpriteImage(effectData.spriteImage);
                else
                    bvh.SetSpriteImage(null);
            }
        }
    }

    public override void OnStatuFixedUpdate()
    {
        if(effectData.duration >= 0)
        {
            tickRatecountDown += Time.fixedDeltaTime;
            countDown += Time.fixedDeltaTime;

            if(countDown > maxDuration && !effectData.isIllimitedTimeEffect)
            {
                OnStatuEnd();
                return;
            }

            if(tickRatecountDown > effectData.tickRate)
            {
                try
                {
                    ApplyEffect();
                }
                catch(Exception e)
                {
                    Magic.Helpers.MagicDebug.DebugLogException(e);
                }
                tickRatecountDown = 0;
            }
        }
    }



    public override void OnStatuEnd(bool callingFromDispel = false)
    {
        if (!effectData.isDispelable && callingFromDispel) return;


        specialData.OnStatuEndActions(manager, effectData, this, amount);
        effectData.OnStatuEndFeedbacks(manager);

        manager.Unregister(this, myStatuData);

        if (effectData.displayTextModule && effectData.addMessageAtEndStatu)
        {
            GameObject go = PoolManager.InstantiateAtPos(effectData.displayValuePrefab, manager.controller.transform.position
                + new Vector3(UnityEngine.Random.Range(effectData.textXOffset.x, effectData.textXOffset.y), UnityEngine.Random.Range(effectData.textYOffset.x, effectData.textYOffset.y), 0f), Quaternion.identity).instance;
            go.SetActive(true);
            OnHitDisplayDamageBehaviour bvh = go.GetComponent<OnHitDisplayDamageBehaviour>();

            bvh.SetDamageText(effectData.messageToAddAtEnd);


            bvh.SetColorDamage(effectData.displayTextColor);

            if (effectData.spriteImage != null && effectData.showIcon)
                bvh.SetSpriteImage(effectData.spriteImage);
            else
                bvh.SetSpriteImage(null);
        }

        if (OnEnd != null) OnEnd();
    }

    public override void OnAddable()
    {
        amount += specialData.amount;

    }

    public override void OnRefreshable()
    {
        countDown = 0;
    }

    public override void ApplyEffect()
    {
        specialData.OnStatuTickActions(manager, effectData, this, amount);
        effectData.OnStatuTickFeedbacks(manager);


        if (effectData.displayTextModule)
        {
            GameObject go = PoolManager.InstantiateAtPos(effectData.displayValuePrefab, manager.controller.transform.position
                + new Vector3(UnityEngine.Random.Range(effectData.textXOffset.x, effectData.textXOffset.y), UnityEngine.Random.Range(effectData.textYOffset.x, effectData.textYOffset.y), 0f), Quaternion.identity).instance;
            go.SetActive(true);
            OnHitDisplayDamageBehaviour bvh = go.GetComponent<OnHitDisplayDamageBehaviour>();

            if(!effectData.displayMessageInsteadValue)
            {
                if(effectData.addMessageAfterValue)
                {
                    if (specialData.amount > 0)
                        bvh.SetDamageText("+" + specialData.amount + " " + effectData.messageToAdd);
                    else
                        bvh.SetDamageText(specialData.amount.ToString() + " " + effectData.messageToAdd);
                }
                else
                {
                    if(specialData.amount > 0)
                        bvh.SetDamageText("+" + specialData.amount);
                    else
                        bvh.SetDamageText(specialData.amount.ToString());
                }


            }
            else
                bvh.SetDamageText(effectData.messageToDisplay);

            bvh.SetColorDamage(effectData.displayTextColor);

            if(effectData.spriteImage != null && effectData.showIcon)
                bvh.SetSpriteImage(effectData.spriteImage);
            else
                bvh.SetSpriteImage(null);
        }

    }
}
