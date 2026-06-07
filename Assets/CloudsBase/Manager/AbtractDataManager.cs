using System.Collections;
using System.Collections.Generic;
using Clouds.Ultilities;
using UnityEngine;

public abstract class AbtractDataManager<T> : Singleton<AbtractDataManager<T>> where T : AbtractDataManager<T>
{
    private static bool iSCOMPLETEDLOADDATA;
    public static bool ISCOMPLETEDLOADDATA { get => iSCOMPLETEDLOADDATA;}
    public override void Awake()
    {
        base.Awake();
        StartCoroutine(this.CrLoadGameData());
    }
    public IEnumerator CrLoadGameData() {
        iSCOMPLETEDLOADDATA = false;
        yield return new WaitUntil(()=> {
            return LoadGameDatas();
        });
        iSCOMPLETEDLOADDATA = true;
    }
    public abstract bool LoadGameDatas();
    public abstract void SaveGame();
}
