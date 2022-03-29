using Packages.MagicLoading.Runtime.Loading;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statu_ChangeModifierBehavior : GenericStatuBehavior
{
    public override Type myType => typeof(Statu_ChangeModifierBehavior);

    public override float Timer => countDown;
    public override float TimerRatio => countDown / maxDuration;

    private GenericStatuData effectData;
    private Statu_ChangeModifierData specialData;
    private float tickRatecountDown = 0;
    private float maxDuration = 0;

    private List<GlobalDataModifier> modifiers = new List<GlobalDataModifier>();

    public override void Setup(GenericStatuData data, float specificStartTime = 0)
    {
        effectData = data;
        specialData = data as Statu_ChangeModifierData;
        countDown = specificStartTime;
        tickRatecountDown = 0;
        maxDuration = effectData.duration;

        for (int i = 0; i < specialData.modifier.Length; i++)
        {
            modifiers.Add(specialData.modifier[i].modifier);
        }
    }

    public override void OnStatuStart()
    {
        effectData.OnStatuStartFeedbacks(manager);
        ApplyEffect();

        if (effectData.duration <= 0)
            OnStatuEnd();

        if (effectData.target.HasFlag(StatuEffectToAddData.Target.Receiver))
        {
            if (effectData.displayMessageOnReceiverApplication)
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
        tickRatecountDown += Time.fixedDeltaTime;
        countDown += Time.fixedDeltaTime;

        if (countDown > maxDuration && !effectData.isIllimitedTimeEffect)
        {
            OnStatuEnd();
            return;
        }

        if(effectData.tickRate >= 0)
        {
            if (tickRatecountDown > effectData.tickRate)
            {
                ApplyEffect();
                tickRatecountDown = 0;
            }
        }
    }

    public override void OnStatuEnd(bool callingFromDispel = false)
    {
        effectData.OnStatuEndFeedbacks(manager);

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (GlobalDataModifierManager.singleton) GlobalDataModifierManager.singleton.RemoveModifier(modifiers[i]);

        }

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
        for (int i = 0; i < specialData.modifier.Length; i++)
        {
            modifiers.Add(specialData.modifier[i].modifier);
        }

        ApplyEffect();

    }

    public override void OnRefreshable()
    {
        countDown = 0;
    }

    public override void ApplyEffect()
    {
        specialData.OnStatuStartModifier(manager);
        effectData.OnStatuTickFeedbacks(manager);


        if (effectData.displayTextModule)
        {
            GameObject go = PoolManager.InstantiateAtPos(effectData.displayValuePrefab, manager.controller.transform.position
                + new Vector3(UnityEngine.Random.Range(effectData.textXOffset.x, effectData.textXOffset.y), UnityEngine.Random.Range(effectData.textYOffset.x, effectData.textYOffset.y), 0f), Quaternion.identity).instance;
            go.SetActive(true);
            OnHitDisplayDamageBehaviour bvh = go.GetComponent<OnHitDisplayDamageBehaviour>();

            if (effectData.displayMessageInsteadValue)
            {
                bvh.SetDamageText(effectData.messageToDisplay);
            }

            bvh.SetColorDamage(effectData.displayTextColor);

            if (effectData.spriteImage != null && effectData.showIcon)
                bvh.SetSpriteImage(effectData.spriteImage);
            else
                bvh.SetSpriteImage(null);
        }

    }
}
