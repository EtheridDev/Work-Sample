using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class GenericStatuBehavior
{
    public MagicCustomController callerController { get; set; }
    public StatuManager manager { get; set; }
    public StatuEffectToAddData myStatuData { get; set; }
    public float countDown { get; set; }
    public abstract Type myType { get; }

    public abstract float Timer { get; }
    public abstract float TimerRatio { get; }


    public Action OnStart;
    public Action OnEnd;

    public abstract void OnStatuStart();
    public abstract void OnStatuFixedUpdate();
    public abstract void OnStatuEnd(bool callingFromDispel = false);
    public virtual void OnRefreshable() { }
    public virtual void OnAddable() { }
    public virtual void ApplyEffect() { }
    public abstract void Setup(GenericStatuData data, float specificStartTime = 0);

}
