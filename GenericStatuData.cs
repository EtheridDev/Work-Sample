using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Packages.MagicLoading.Runtime.Loading;

public abstract class GenericStatuData : GameData
{
    public abstract Type myBehavior { get; }

    [Title("Basic statu parameters")]
    public float delay = 0;
    public StatuEffectToAddData.Target target;
    public StatuEffectToAddData.Type statuType;
    public bool isIllimitedTimeEffect = false;
    [HideIf("isIllimitedTimeEffect")]
    public float duration;
    public float tickRate;
    [ShowIf("mustShowStackToApplyField")]
    public float stackToApply;
    public bool isDispelable = false;
    [ShowIf("mustShowsavedEffectTimerField")]
    public bool useDifferentTimerForSavedEffect = false;
    [ShowIf("useDifferentTimerForSavedEffect")]
    public float savedEffectTimer;
    public bool cantBeLethal = false;

    // ----------- Display parameters ----------
    [Title("Display parameters")]
    public bool displayTextModule = false;
    [ShowIf("displayTextModule"),BoxGroup("DisplayParam", ShowLabel = false)]
    public MagicAsset displayValuePrefab;
    [ShowIf("displayTextModule"), BoxGroup("DisplayParam")]
    [MinMaxSlider(-10, 10, true)]
    public Vector2 textXOffset = new Vector3(-2f, 2f);
    [ShowIf("displayTextModule"), BoxGroup("DisplayParam")]
    [MinMaxSlider(-10, 10, true)]
    public Vector2 textYOffset = new Vector3(1f, 2f);
    [ShowIf("displayTextModule"), BoxGroup("DisplayParam")]
    public Color displayTextColor;
    [ShowIf("displayTextModule"), BoxGroup("DisplayParam")]
    public Sprite spriteImage;
    [ShowIf("displayTextModule"), BoxGroup("DisplayParam")]
    public bool showIcon = true;

    [ShowIf("displayTextModule"), BoxGroup("DisplayParam")]
    public bool addMessageAtEndStatu = false;
    [ShowIf("@displayTextModule && addMessageAtEndStatu"), BoxGroup("DisplayParam")]
    public string messageToAddAtEnd;

    [ShowIf("displayTextModule"), BoxGroup("DisplayParam")]
    public bool addMessageAfterValue = false;
    [ShowIf("@displayTextModule && addMessageAfterValue"), BoxGroup("DisplayParam")]
    public string messageToAdd;

    [ShowIf("displayTextModule"), BoxGroup("DisplayParam")]
    public bool displayMessageOnReceiverApplication = false;
    [ShowIf("@displayTextModule && displayMessageOnReceiverApplication"), BoxGroup("DisplayParam")]
    public string messageToAddOnReceiver;

    [ShowIf("displayTextModule"), BoxGroup("DisplayParam")]
    public bool displayMessageInsteadValue = false;
    [ShowIf("@displayTextModule && displayMessageInsteadValue"), BoxGroup("DisplayParam")]
    public string messageToDisplay;

    [ShowIf("mustShowMessageToLauncherField")]
    public bool displayMessageOnLauncher = false;
    [ShowIf("displayMessageOnLauncher")]
    public string messageToDisplayOnLauncher;

    // ----------- Display HUD Feedback ----------
    public bool displayHUDFeedback;
    [ShowIf("displayHUDFeedback"), BoxGroup("DisplayFeeback", ShowLabel = false)]
    public Sprite HUDIcon;
    [ShowIf("displayHUDFeedback"), BoxGroup("DisplayFeeback", ShowLabel = false)]
    public int iconStyleIndex = 0;

    [Title("Feedback Game action")]
    public GameAction[] OnStatuStartFeedback;
    public GameAction[] OnStatuEndFeedback;
    public GameAction[] OnStatuTickFeedback;

    [Title("Condition")]
    public bool applyEffectOnlyUsingSpecialWeapon = false;
    [ShowIf("applyEffectOnlyUsingSpecialWeapon")]
    public AttackSetGroup[] AttackSetGroup;


    private bool mustShowStackToApplyField
    {
        get
        {
            if (statuType == StatuEffectToAddData.Type.Stackable)
                return true;

            return false;
        }
    }

    private bool mustShowMessageToLauncherField
    {
        get
        {
            if (target == StatuEffectToAddData.Target.Receiver)
                return true;

            return false;
        }
    }

    private bool mustShowsavedEffectTimerField
    {
        get
        {
            if (target == StatuEffectToAddData.Target.Receiver)
                return true;

            return false;
        }
    }

    public virtual void OnStatuStartFeedbacks(StatuManager bhv)
    {
        for (int i = 0; i < OnStatuStartFeedback.Length; i++)
        {
            if (OnStatuStartFeedback[i])
            {
                OnStatuStartFeedback[i].Action(bhv);
            }
        }
    }

    public virtual void OnStatuEndFeedbacks(StatuManager bhv)
    {
        for (int i = 0; i < OnStatuEndFeedback.Length; i++)
        {
            if (OnStatuEndFeedback[i])
            {
                OnStatuEndFeedback[i].Action(bhv);
            }
        }
    }

    public virtual void OnStatuTickFeedbacks(StatuManager bhv)
    {
        for (int i = 0; i < OnStatuTickFeedback.Length; i++)
        {
            if (OnStatuTickFeedback[i])
            {
                OnStatuTickFeedback[i].Action(bhv);
            }
        }
    }
}
