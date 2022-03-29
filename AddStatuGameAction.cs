using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatuGameAction : GameAction
{
    [InlineEditor]
    public StatuEffectToAddData effect;
    public float timeDelay = 0;
    public bool addStatuDirectlyToPlayer = false;


    public override void Action(MonoBehaviour actionCaller)
    {
        if (timeDelay == 0)
            AddStatu(actionCaller);
        else
            Magic.Helpers.MagicUtility.RunAfter(() => AddStatu(actionCaller), timeDelay);
    }

    private void AddStatu(MonoBehaviour actionCaller)
    {
        StatuManager manager = null;
        if(addStatuDirectlyToPlayer)
        {
            if (PlayerSpawner.SpawnedPlayers[0].controller.TryGetSpecificBehavior(ref manager))
            {
                for (int i = 0; i < effect.Effect.Length; i++)
                {
                    manager.RegisterEffect(effect, i);

                }
            }
        }
        else
        {
            if(actionCaller.gameObject.transform.root.GetComponent<MagicCustomController>().TryGetSpecificBehavior(ref manager))
            {
                for (int i = 0; i < effect.Effect.Length; i++)
                {
                    manager.RegisterEffect(effect, i);

                }
            }

        }
    }
}
