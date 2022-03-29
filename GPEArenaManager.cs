using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Magic.GPE;
using Magic.GPE.Link;
using Sirenix.OdinInspector;
using Magic.RoomEditor;
using Packages.MagicLoading.Runtime.Loading;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class GPEArenaManager : GPETrigger, IOnEnable, ILevelSaved
{

    public class RegisterEnnemy
    {
        public MagicCustomController controller;
        public GPEArenaManager manager;
        public GPEArenaSpawnEnnemies fromSpawner;
        public RegisterEnnemy(MagicCustomController newController, GPEArenaManager newManager, GPEArenaSpawnEnnemies newFromSpawner)
        {
            controller = newController;
            manager = newManager;
            fromSpawner = newFromSpawner;

            DeathManager deathManager = null;
            if (controller.TryGetSpecificBehavior(ref deathManager))
            {
                deathManager.OnDeath += Unregister;
            }
        }
        public void Unregister()
        {
            manager.UnRegisterEnnemy(this);

            DeathManager deathManager = null;
            if (controller.TryGetSpecificBehavior(ref deathManager))
            {
                deathManager.OnDeath -= Unregister;
            }
        }

    }

    [Flags]
    public enum CameraBlocking
    {
        None = 0,
        Left = 1,
        Right = 2,
        Up = 4,
        Down = 8
    }

    [FoldoutGroup("Links", 10)] [Required] public GameObject[] cameraFrames;
    [FoldoutGroup("Links", 10)] [Required] public GameObject doorPrefab;
    [FoldoutGroup("Links", 10)] [Required] public GameObject doorParentObject;
    [FoldoutGroup("Links", 10)] [Required] public GameObject ColliderPrefab;
    [FoldoutGroup("Links", 10)] [Required] public GameObject ColliderParentObject;
    [FoldoutGroup("Links", 10)] [Required] public GameObject SpawnerPrefab;
    [FoldoutGroup("Links", 10)] [Required] public GameObject SpawnerParentObject;

    [Title("Arena Settings")]
    [InlineEditor]
    public WavesDataList[] arenaDatas;
    public bool WaitForFullWaveBeforeRespawn = true;
    public CameraBlocking BlockCamera = CameraBlocking.None;
    public bool ReactivateArenaAtEnd = false;
    public bool CloseAllDoorsAtStart = true;
    [HideIf("CloseAllDoorsAtStart")]
    public List<MagicCustomController> specificDoorToClose;
    public bool OpenAllDoorsAtEnd = true;
    [HideIf("OpenAllDoorsAtEnd")]
    public List<MagicCustomController> specificDoorToOpen;
    public PlayerDetectorData DetectorType;


    [Title("Linked Object")]
    public List<MagicCustomController> Doors;
    public List<GameObject> StartColliders;
    public List<GPEArenaSpawnEnnemies> Spawners;

    public List<List<RegisterEnnemy>> spawnedEnnemies = new List<List<RegisterEnnemy>>();


    private List<RegisterEnnemy> _spawnedEnemies = new List<RegisterEnnemy>();

    private WavesDataList arenaData;
    private int currentWave = 0;
    private int currentActionState = 0;

    private List<ArenaActionData.SpawnEnnemyParam> EnnemyToRegisterToSpawnLater = new List<ArenaActionData.SpawnEnnemyParam>();

    private List<bool> delayedActions = new List<bool>();

    private ArenaSavedData savedData = null;

    public bool ArenaDone { get; private set; } = false;

    public void Start()
    {
        DesactivateCameraFrame();
    }

    public void ReceiveEvent(MagicEvent magicEvent)
    {
        if (magicEvent.eventName == MagicEventName.activableEvent)
        {
            LaunchArena();
        }
    }

    public void OnEnableBehavior()
    {
        if (arenaDatas.Length > 0)
            arenaData = arenaDatas[UnityEngine.Random.Range(0, arenaDatas.Length)];
    }

    public void LaunchArena()
    {
        GetArenaInfo();
        CloseDoorsAtStart();
        DesactivateAllCollider();
        CheckCameraFrame();
        ChoseAction(arenaData.wavesData[currentWave]);

    }

    public void GetArenaInfo()
    {
        currentWave = 0;
        currentActionState = 0;
    }

    public void AreAllActionDone(bool isDelayedAction = false)
    {

        if (currentActionState < arenaData.wavesData[currentWave].arenaActions.Count)
        {
            ChoseAction(arenaData.wavesData[currentWave]);
        }
        else
        {
            currentActionState = 0;
            currentWave++;
        }
        if (isDelayedAction)
            delayedActions.RemoveAt(0);
    }

    public void CheckRefillMode()
    {
        if (WaitForFullWaveBeforeRespawn)
        {
            for (int i = 0; i < Spawners.Count; i++)
            {
                if (Spawners[i].EnnemiesSpawnedFromthisSpawner > 0)
                    return;
            }
            if (delayedActions.Count > 0)
                return;

            if (currentWave < arenaData.wavesData.Count)
                ChoseAction(arenaData.wavesData[currentWave]); // all actions are done and last ennemy is dead, going to next wave
        }
        else
        {
            if (EnnemyToRegisterToSpawnLater.Count > 0)
            {
                if (EnnemyToRegisterToSpawnLater[0].SpawnWithDelay)
                    spawnEnnemy(EnnemyToRegisterToSpawnLater[0].EnnemyToSpawn, EnnemyToRegisterToSpawnLater[0].SpawnerID, EnnemyToRegisterToSpawnLater[0].timer, EnnemyToRegisterToSpawnLater[0].FXToPlay);
                else
                    spawnEnnemy(EnnemyToRegisterToSpawnLater[0].EnnemyToSpawn, EnnemyToRegisterToSpawnLater[0].SpawnerID, 0f, EnnemyToRegisterToSpawnLater[0].FXToPlay);

                EnnemyToRegisterToSpawnLater.RemoveAt(0);
            }
            else
            {
                for (int i = 0; i < Spawners.Count; i++)
                {
                    if (Spawners[i].EnnemiesSpawnedFromthisSpawner > 0)
                        return;
                }
                if (delayedActions.Count > 0)
                    return;
                if (currentWave < arenaData.wavesData.Count)
                    ChoseAction(arenaData.wavesData[currentWave]); // all actions are done and last ennemy is dead, going to next wave
            }

        }
    }

    public void WaveWithNoSpawning()
    {
        if (currentWave < arenaData.wavesData.Count)
            ChoseAction(arenaData.wavesData[currentWave]);
        else
            CheckEndOfArena();
    }

    public void ChoseAction(WavesData wavedata)
    {
        float delayForNextAction = 0;
        bool hadSpawnAction = false;
        for (int i = 0; i < wavedata.arenaActions[currentActionState].Actions.Length; i++)
        {
            if (wavedata.arenaActions[currentActionState].Actions[i] is ArenaActionData.SpawnEnnemyParam)
            {
                if (!(wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.SpawnEnnemyParam).CanBeRegisterToSpawn)
                {
                    if (!(wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.SpawnEnnemyParam).SpawnWithDelay)
                        spawnEnnemy((wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.SpawnEnnemyParam).EnnemyToSpawn, (wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.SpawnEnnemyParam).SpawnerID, 0f, (wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.SpawnEnnemyParam).FXToPlay);
                    else
                        spawnEnnemy((wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.SpawnEnnemyParam).EnnemyToSpawn, (wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.SpawnEnnemyParam).SpawnerID, (wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.SpawnEnnemyParam).timer, (wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.SpawnEnnemyParam).FXToPlay);
                }
                else
                    EnnemyToRegisterToSpawnLater.Add((wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.SpawnEnnemyParam));

                hadSpawnAction = true;
                if (!wavedata.arenaActions[currentActionState].ExecuteNextActionInstantly)
                    delayForNextAction = wavedata.arenaActions[currentActionState].delay;
            }

            if (wavedata.arenaActions[currentActionState].Actions[i] is ArenaActionData.OpenAllDoors)
            {
                OpenDoors();

                if (!wavedata.arenaActions[currentActionState].ExecuteNextActionInstantly)
                    delayForNextAction = wavedata.arenaActions[currentActionState].delay;
            }

            if (wavedata.arenaActions[currentActionState].Actions[i] is ArenaActionData.CloseAllDoors)
            {
                CloseAllDoors();


                if (!wavedata.arenaActions[currentActionState].ExecuteNextActionInstantly)
                    delayForNextAction = wavedata.arenaActions[currentActionState].delay;
            }

            if (wavedata.arenaActions[currentActionState].Actions[i] is ArenaActionData.OpenSpecificDoor)
            {
                OpenSpecificDoorFunction((wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.OpenSpecificDoor).DoorID);


                if (!wavedata.arenaActions[currentActionState].ExecuteNextActionInstantly)
                    delayForNextAction = wavedata.arenaActions[currentActionState].delay;
            }

            if (wavedata.arenaActions[currentActionState].Actions[i] is ArenaActionData.CloseSpecificDoor)
            {
                CloseSpecificDoorFunction((wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.CloseSpecificDoor).DoorID);


                if (!wavedata.arenaActions[currentActionState].ExecuteNextActionInstantly)
                    delayForNextAction = wavedata.arenaActions[currentActionState].delay;
            }

            if (wavedata.arenaActions[currentActionState].Actions[i] is ArenaActionData.ModifyCameraFrame)
            {
                ArenaActionData.ModifyCameraFrame dataRef = (wavedata.arenaActions[currentActionState].Actions[i] as ArenaActionData.ModifyCameraFrame);
                ModifyCameraFrame(dataRef.newCameraBlocking, dataRef.newLeftValue, dataRef.newRightValue, dataRef.newUpValue, dataRef.newDownValue);



                if (!wavedata.arenaActions[currentActionState].ExecuteNextActionInstantly)
                    delayForNextAction = wavedata.arenaActions[currentActionState].delay;
            }
        }

        currentActionState++;

        if (delayForNextAction <= 0)
        {
            AreAllActionDone();
            if (!hadSpawnAction)
                WaveWithNoSpawning();
        }
        else
        {
            Magic.Helpers.MagicUtility.RunAfter(() => AreAllActionDone(true), delayForNextAction);
            delayedActions.Add(true);
            Debug.Log(hadSpawnAction);
            if (hadSpawnAction == false)
                Magic.Helpers.MagicUtility.RunAfter(() => WaveWithNoSpawning(), delayForNextAction);

        }
    }

    public void spawnEnnemy(MagicAsset[] ennemyType, int spawnnerID, float delay, GameAction spawnFX)
    {
        MagicAsset ChosenEnemy = null;
        if (ennemyType.Length == 1)
            ChosenEnemy = ennemyType[0];
        else
            ChosenEnemy = ennemyType[UnityEngine.Random.Range(0, ennemyType.Length)];

        int spawnerIDToUse = 0;

        if (spawnnerID < Spawners.Count && spawnnerID < Spawners.Count && ChosenEnemy != null)
        {
            Spawners[spawnnerID].Spawn(ChosenEnemy, delay);
            spawnerIDToUse = spawnnerID;
        }
        if (spawnnerID < Spawners.Count && spawnnerID >= Spawners.Count && ChosenEnemy != null)
        {
            spawnerIDToUse = UnityEngine.Random.Range(0, Spawners.Count - 1);
            Spawners[spawnerIDToUse].Spawn(ChosenEnemy, delay);
        }

        if (spawnFX != null)
        {
            spawnFX.Action(Spawners[spawnerIDToUse]);
        }
    }

    public void RegisterEnnemies(GPEArenaSpawnEnnemies spawner, MagicCustomController magicController)
    {
        for (int i = 0; i < Spawners.Count; i++)
        {
            if (Spawners[i] != null && spawner == Spawners[i])
            {
                RegisterEnnemy registerEnnemy = new RegisterEnnemy(magicController, this, spawner);
                spawnedEnnemies[i].Add(registerEnnemy);
                _spawnedEnemies.Add(registerEnnemy);

                PlayerDetectorBehavior detector = null;
                if (registerEnnemy.controller.TryGetSpecificBehavior(ref detector))
                {
                    detector.allData.CurrentData = DetectorType;
                }
            }
        }
    }

    public void UnRegisterEnnemy(RegisterEnnemy controller)
    {
        for (int i = 0; i < Spawners.Count; i++)
        {
            if (Spawners[i] != null && controller.fromSpawner == Spawners[i])
            {
                for (int j = 0; j < spawnedEnnemies[i].Count; j++)
                {
                    if (spawnedEnnemies[i][j].controller = controller.controller)
                    {
                        spawnedEnnemies[i].RemoveAt(j);
                        controller.fromSpawner.EnnemiesSpawnedFromthisSpawner--;

                    }
                }
            }
        }

        _spawnedEnemies.Remove(controller);


        CheckRefillMode();
        CheckEndOfArena();
    }

    public bool IsEnnemiesStillAliveFromSpawner(RegisterEnnemy controller)
    {
        if (controller != null && controller.fromSpawner.EnnemiesSpawnedFromthisSpawner > 0)
            return true;
        else
            return false;
    }

    public void CheckEndOfArena()
    {
        if (currentWave < arenaData.wavesData.Count)
            return;
        for (int i = 0; i < Spawners.Count; i++)
        {
            if (Spawners[i].EnnemiesSpawnedFromthisSpawner > 0)
                return;
        }

        if (EnnemyToRegisterToSpawnLater.Count > 0)
            return;

        if (_spawnedEnemies.Count > 0)
            return;

        StopArena();
    }

    public void RegisterSpawner()
    {
        spawnedEnnemies.Add(new List<RegisterEnnemy>());
    }

    public void DesactivateAllCollider()
    {
        for (int i = 0; i < StartColliders.Count; i++)
        {
            if (StartColliders[i] != null)
                StartColliders[i].gameObject.SetActive(false);
        }
    }

    public void ActivateAllCollider()
    {
        for (int i = 0; i < StartColliders.Count; i++)
        {
            if (StartColliders[i] != null)
                StartColliders[i].gameObject.SetActive(true);
        }
    }

    public void CloseSpecificDoorFunction(int[] id)
    {
        for (int i = 0; i < id.Length; i++)
        {
            DoorArena doorScript = null;
            if (Doors[id[i]].TryGetSpecificBehavior(ref doorScript))
                doorScript.CloseDoor();
        }
    }

    public void OpenSpecificDoorFunction(int[] id)
    {
        for (int i = 0; i < id.Length; i++)
        {
            DoorArena doorScript = null;
            if (Doors[id[i]].TryGetSpecificBehavior(ref doorScript))
                doorScript.OpenDoor();
        }
    }

    public void CloseDoorsAtStart()
    {
        if (CloseAllDoorsAtStart)
        {
            for (int i = 0; i < Doors.Count; i++)
            {
                if (Doors[i] != null)
                {
                    DoorArena doorScript = null;
                    if (Doors[i].TryGetSpecificBehavior(ref doorScript))
                    {
                        doorScript.CloseDoor();
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < specificDoorToClose.Count; i++)
            {
                if (specificDoorToClose[i] != null)
                {
                    DoorArena doorScript = null;
                    if (specificDoorToClose[i].TryGetSpecificBehavior(ref doorScript))
                    {
                        doorScript.CloseDoor();
                    }
                }
            }
        }
    }

    public void CloseAllDoors()
    {
        if (CloseAllDoorsAtStart)
        {
            for (int i = 0; i < Doors.Count; i++)
            {
                if (Doors[i] != null)
                {
                    DoorArena doorScript = null;
                    if (Doors[i].TryGetSpecificBehavior(ref doorScript))
                    {
                        doorScript.CloseDoor();
                    }
                }
            }
        }
    }

    public void CheckCameraFrame()
    {
        if (BlockCamera.HasFlag(CameraBlocking.Left))
            cameraFrames[0].SetActive(true);
        if (BlockCamera.HasFlag(CameraBlocking.Right))
            cameraFrames[1].SetActive(true);
        if (BlockCamera.HasFlag(CameraBlocking.Up))
            cameraFrames[2].SetActive(true);
        if (BlockCamera.HasFlag(CameraBlocking.Down))
            cameraFrames[3].SetActive(true);



    }

    public void ModifyCameraFrame(CameraBlocking newCameraBlocking, float newleftValue, float newrightValue, float newupValue, float newdownValue)
    {
        if (newCameraBlocking.HasFlag(CameraBlocking.Left))
        {
            cameraFrames[0].SetActive(true);
            CameraFrame refCameraFrame = cameraFrames[0].GetComponentInChildren<CameraFrame>();
            if (newleftValue > 0)
                refCameraFrame.width = newleftValue;

            refCameraFrame.RefreshCameraFrame();
        }
        else
            cameraFrames[0].SetActive(false);


        if (newCameraBlocking.HasFlag(CameraBlocking.Right))
        {
            cameraFrames[1].SetActive(true);
            CameraFrame refCameraFrame = cameraFrames[1].GetComponentInChildren<CameraFrame>();

            if (newrightValue > 0)
                refCameraFrame.width = newrightValue;

            refCameraFrame.RefreshCameraFrame();
        }
        else
            cameraFrames[1].SetActive(false);


        if (newCameraBlocking.HasFlag(CameraBlocking.Up))
        {
            cameraFrames[2].SetActive(true);
            CameraFrame refCameraFrame = cameraFrames[2].GetComponentInChildren<CameraFrame>();

            if (newupValue > 0)
                refCameraFrame.height = newupValue;

            refCameraFrame.RefreshCameraFrame();

        }
        else
            cameraFrames[2].SetActive(false);


        if (newCameraBlocking.HasFlag(CameraBlocking.Down))
        {
            cameraFrames[3].SetActive(true);
            CameraFrame refCameraFrame = cameraFrames[3].GetComponentInChildren<CameraFrame>();

            if (newdownValue > 0)
                refCameraFrame.height = newdownValue;

            refCameraFrame.RefreshCameraFrame();

        }
        else
            cameraFrames[3].SetActive(false);
    }

    public void DesactivateCameraFrame()
    {
        if (BlockCamera.HasFlag(CameraBlocking.Left))
            cameraFrames[0].SetActive(false);
        if (BlockCamera.HasFlag(CameraBlocking.Right))
            cameraFrames[1].SetActive(false);
        if (BlockCamera.HasFlag(CameraBlocking.Up))
            cameraFrames[2].SetActive(false);
        if (BlockCamera.HasFlag(CameraBlocking.Down))
            cameraFrames[3].SetActive(false);
    }

    public void OpenDoors()
    {
        if (OpenAllDoorsAtEnd)
        {
            for (int i = 0; i < Doors.Count; i++)
            {
                if (Doors[i] != null)
                {
                    DoorArena doorScript = null;
                    if (Doors[i].TryGetSpecificBehavior(ref doorScript))
                    {
                        doorScript.OpenDoor();
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < specificDoorToOpen.Count; i++)
            {
                if (specificDoorToOpen[i] != null)
                {
                    DoorArena doorScript = null;
                    if (specificDoorToOpen[i].TryGetSpecificBehavior(ref doorScript))
                    {
                        doorScript.OpenDoor();
                    }
                }
            }
        }

    }

    public void StopArena()
    {
        OpenDoors();

        DesactivateCameraFrame();

        if (ReactivateArenaAtEnd)
            ActivateAllCollider();
        else
            ArenaDone = true;
    }

#if UNITY_EDITOR
    [Button("Add a door"), HideInPlayMode, Title("Adding Objects")]
    public void AddDoor()
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
        go.transform.position = gameObject.transform.position;
        go.transform.SetParent(doorParentObject.transform);
        go.gameObject.name = "GPE_ArenaDoor_" + Doors.Count;
        Doors.Add(go.GetComponent<MagicCustomController>());
    }

    [Button("Add a collider"), HideInPlayMode]
    public void AddCollider()
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(ColliderPrefab);
        go.transform.position = gameObject.transform.position;
        go.transform.SetParent(ColliderParentObject.transform);
        go.gameObject.name = "StartArenaCollider_" + StartColliders.Count;
        go.GetComponent<GPEArenaTriggerCollider>().arenaManager = this;
        StartColliders.Add(go);
    }

    [Button("Add a spawner"), HideInPlayMode]
    public void AddSpawner()
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(SpawnerPrefab);
        go.transform.position = gameObject.transform.position;
        go.transform.SetParent(SpawnerParentObject.transform);
        go.gameObject.name = "Spawner_Arena" + Spawners.Count;
        go.GetComponent<GPEArenaSpawnEnnemies>().arenaManager = this;
        Spawners.Add(go.GetComponent<GPEArenaSpawnEnnemies>());
    }

    [Button("Refill and verify all lists"), HideInPlayMode, Title("Utility")]
    public void CheckAllLists()
    {
        Doors.Clear();
        StartColliders.Clear();
        Spawners.Clear();

        foreach (Transform go in doorParentObject.transform)
        {
            if (go.GetComponent<MagicCustomController>() != null)
                Doors.Add(go.GetComponent<MagicCustomController>());
        }

        foreach (Transform go in ColliderParentObject.transform)
        {
            if (go != null)
            {
                StartColliders.Add(go.gameObject);
                go.GetComponent<GPEArenaTriggerCollider>().arenaManager = this;
            }
        }

        foreach (Transform go in SpawnerParentObject.transform)
        {
            if (go.GetComponent<GPEArenaSpawnEnnemies>() != null)
            {
                Spawners.Add(go.GetComponent<GPEArenaSpawnEnnemies>());
                go.GetComponent<GPEArenaSpawnEnnemies>().arenaManager = this;
            }
        }
    }
#endif

    protected override void LinkAllDependencies()
    {
        base.LinkAllDependencies();

    }

    [Serializable]
    public class ArenaSavedData : SaveIdentifierHolder.GPESaveData
    {
        public bool arenaDone = false;
    }

    public SaveIdentifierHolder.GPESaveData FillSavableData()
    {
        if (savedData == null) savedData = new ArenaSavedData();

        savedData.arenaDone = ArenaDone;

        return savedData;
    }

    public void ApplySavableData(SaveIdentifierHolder.GPESaveData data)
    {
        if (!(data is ArenaSavedData))
        {
            Magic.Helpers.MagicDebug.LogSaveErrorFormat("GPEArenaManager : Try to apply wrong data type ({0}) to {1}", data.GetType().FullName, this.controller.gameObject.name);
            return;
        }

        savedData = data as ArenaSavedData;

        if(savedData.arenaDone)
        {
            ArenaDone = true;
            DesactivateAllCollider();
        }
    }
}
