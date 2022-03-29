using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Packages.MagicLoading.Runtime.Loading;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;

public class ArenaActionData : SerializedGameData
{
    public enum logicalOperator
    {
        None,
        And,
        Or
    }
    public enum conditions
    {
        None,
        KillAllEnnemies,
        killPourcentageOfEnnemies,
        Timer
    }

    public conditions[] conditionsToUse;
    public logicalOperator operatorToUse;
    [OdinSerialize]
    [ShowInInspector]
    public ActionsParam[] Actions;

    public bool ExecuteNextActionInstantly = true;
    [HideIf("ExecuteNextActionInstantly")]
    public float delay = 0f;

    [Serializable]
    public abstract class ActionsParam { }


    [HideLabel]
    [Serializable]
    [HideReferenceObjectPicker]
    public class SpawnEnnemyParam : ActionsParam
    {
        public MagicAsset[] EnnemyToSpawn;
        public int SpawnerID;
        public bool CanBeRegisterToSpawn;
        public bool SpawnWithDelay = false;
        [ShowIf("SpawnWithDelay")]
        public float timer = 0.5f;
        public GameAction FXToPlay;
    }

    [HideLabel]
    [Serializable]
    [HideReferenceObjectPicker]
    public class OpenAllDoors : ActionsParam
    {

    }

    [HideLabel]
    [Serializable]
    [HideReferenceObjectPicker]
    public class CloseAllDoors : ActionsParam
    {

    }

    [HideLabel]
    [Serializable]
    [HideReferenceObjectPicker]
    public class OpenSpecificDoor : ActionsParam
    {
        public int[] DoorID;
    }

    [HideLabel]
    [Serializable]
    [HideReferenceObjectPicker]
    public class CloseSpecificDoor : ActionsParam
    {
        public int[] DoorID;
    }

    public class ModifyCameraFrame : ActionsParam
    {
        public GPEArenaManager.CameraBlocking newCameraBlocking;
        [ShowIf("AsLeftValue")]
        public float newLeftValue;
        [ShowIf("AsRightValue")]
        public float newRightValue;
        [ShowIf("AsUpValue")]
        public float newUpValue;
        [ShowIf("AsDownValue")]
        public float newDownValue;

        public bool AsLeftValue
        {
            get
            {
                if (newCameraBlocking.HasFlag(GPEArenaManager.CameraBlocking.Left))
                    return true;

                return false;
            }
        }

        public bool AsRightValue
        {
            get
            {
                if (newCameraBlocking.HasFlag(GPEArenaManager.CameraBlocking.Right))
                    return true;

                return false;
            }
        }

        public bool AsUpValue
        {
            get
            {
                if (newCameraBlocking.HasFlag(GPEArenaManager.CameraBlocking.Up))
                    return true;

                return false;
            }
        }

        public bool AsDownValue
        {
            get
            {
                if (newCameraBlocking.HasFlag(GPEArenaManager.CameraBlocking.Down))
                    return true;

                return false;
            }
        }


    }
}
