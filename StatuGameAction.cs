using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatuGameAction : GameAction
{

    public override void Action(MonoBehaviour actionCaller) {}

    public abstract void Action(MonoBehaviour actionCaller, float value, GenericStatuData data, GenericStatuBehavior bvh);
}
