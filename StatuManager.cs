using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Packages.MagicLoading.Runtime.Loading;
using System.Linq;

public class StatuManager : MagicBehavior, IOnFixedUpdate, IOnEnable
{
    [HideInInspector]
    public EffectFeedbackUIManager uiManager;

    public List<GenericStatuBehavior> allEffects = new List<GenericStatuBehavior>();
    public List<GenericStatuBehavior> allEffectsOverTime = new List<GenericStatuBehavior>();
    public List<savedEffectData> savedEffectDatas = new List<savedEffectData>();

    public class savedEffectData
    {
        public StatuEffectToAddData savedData;
        public GenericStatuBehavior effects;

    }

    public void RegisterEffect(StatuEffectToAddData data, int index, bool dontApplyStartingTick = false, float specificStartTime = 0)
    {
        GenericStatuBehavior instance = (GenericStatuBehavior)Activator.CreateInstance(data.Effect[index].myBehavior);

        for (int i = 0; i < data.Effect[index].stackToApply; i++)
        {
            if(allEffects.Count > 0)
            {
                if(data.Effect[i].statuType.HasFlag(StatuEffectToAddData.Type.Stackable))
                {
                    AddToListAndFillData(allEffects, instance, data, index, dontApplyStartingTick, specificStartTime);
                }

                if (data.Effect[i].statuType.HasFlag(StatuEffectToAddData.Type.Addeable))
                {
                    bool isAtLeastOneEffectAddeable = false;
                    for (int j = 0; j < allEffects.Count; j++)
                    {
                        if (allEffects[j].myType == data.Effect[index].myBehavior)
                        {
                            allEffects[j].OnAddable();
                            isAtLeastOneEffectAddeable = true;
                            break;
                        }
                    }
                    if (!isAtLeastOneEffectAddeable)
                        AddToListAndFillData(allEffects, instance, data, index, dontApplyStartingTick, specificStartTime);
                }

                if (data.Effect[i].statuType.HasFlag(StatuEffectToAddData.Type.Refreshable))
                {
                    bool isAtLeastOneEffectRefreshable = false;
                    for (int j = 0; j < allEffects.Count; j++)
                    {
                        if (allEffects[j].myType == data.Effect[index].myBehavior)
                        {
                            allEffects[j].OnRefreshable();
                            isAtLeastOneEffectRefreshable = true;
                            break;
                        }

                    }
                    if (!isAtLeastOneEffectRefreshable)
                        AddToListAndFillData(allEffects, instance, data, index, dontApplyStartingTick, specificStartTime);
                }

                if (data.Effect[i].statuType.HasFlag(StatuEffectToAddData.Type.Unique))
                {
                    for (int j = 0; j < allEffects.Count; j++)
                    {
                        if (allEffects[j].myType == data.Effect[index].myBehavior)
                        {
                            break;
                        }

                        AddToListAndFillData(allEffects, instance, data, index, dontApplyStartingTick, specificStartTime);
                    }
                }
            }
            else
                AddToListAndFillData(allEffects, instance, data, index, dontApplyStartingTick, specificStartTime);
        }
    }

    private void AddToListAndFillData(List<GenericStatuBehavior> list, GenericStatuBehavior instance, StatuEffectToAddData data, int index, bool dontApplyStartingTick = false, float specificStartTime = 0)
    {
        list.Add(instance);
        list[list.Count - 1].callerController = controller;
        list[list.Count - 1].manager = this;
        list[list.Count - 1].myStatuData = data;
        list[list.Count - 1].Setup(data.Effect[index], specificStartTime);


        if (!dontApplyStartingTick)
            ApplyImmediatEffect(list.Count - 1);

        if( data.Effect[index].duration > 0)
            allEffectsOverTime.Add(instance);

        savedEffectDatas.Add(new savedEffectData() { savedData = data, effects = instance });

        if (data.Effect[index].displayHUDFeedback)
            uiManager.RegisterStatuEffect(data.Effect[index], instance);
    }

    public void ApplyImmediatEffect(int index)
    {
        allEffects[index].OnStatuStart();
    }

    public void OnFixedUpdate()
    {
        for (int i = 0; i < allEffectsOverTime.Count; i++)
        {
            allEffectsOverTime[i].OnStatuFixedUpdate();
        }
    }

    public void Unregister(GenericStatuBehavior data, StatuEffectToAddData statuEffectToAddData)
    {
        allEffects.Remove(data);

        if(allEffectsOverTime.Contains(data))
            allEffectsOverTime.Remove(data);


        savedEffectData toDelet = null;
        toDelet = savedEffectDatas.Find(x => (x.effects == data) && (x.savedData == statuEffectToAddData));

        if (toDelet != null)
            savedEffectDatas.Remove(toDelet);

    }

    public GenericStatuBehavior GetGenericStatuBehavior(int index)
    {
        return allEffects[index];
    }

    public void ShowMessageOnLauncher(StatuEffectToAddData data, int index)
    {
        if (data.Effect[index].displayMessageOnLauncher)
        {
            GameObject go = PoolManager.InstantiateAtPos(data.Effect[index].displayValuePrefab, controller.transform.position
                + new Vector3(UnityEngine.Random.Range(data.Effect[index].textXOffset.x, data.Effect[index].textXOffset.y), UnityEngine.Random.Range(data.Effect[index].textYOffset.x, data.Effect[index].textYOffset.y), 0f), Quaternion.identity).instance;
            go.SetActive(true);
            OnHitDisplayDamageBehaviour bvh = go.GetComponent<OnHitDisplayDamageBehaviour>();

            bvh.SetDamageText(data.Effect[index].messageToDisplayOnLauncher);


            bvh.SetColorDamage(data.Effect[index].displayTextColor);

            if (data.Effect[index].spriteImage != null && data.Effect[index].showIcon)
                bvh.SetSpriteImage(data.Effect[index].spriteImage);
            else
                bvh.SetSpriteImage(null);
        }
    }

    protected override void LinkAllDependencies()
    {
        
    }

    public void OnEnableBehavior()
    {
        if(controller.tag != "Player")
        {
            allEffects.Clear();
            allEffectsOverTime.Clear();
        }
    }
}
