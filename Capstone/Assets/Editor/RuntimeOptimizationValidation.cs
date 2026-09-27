using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace DigGame.Editor
{
    public static class RuntimeOptimizationValidation
    {
        static void Require(bool value,string message) { if (!value) throw new InvalidOperationException(message); }
        [MenuItem("DigGame/Validate Runtime Reuse")]
        public static void Run()
        {
            var root=new GameObject("Runtime reuse validation");
            var random=UnityEngine.Random.state;
            try
            {
                var factory=new PooledGameObjectFactory(new PrefabGameObjectFactory(Resources.Load<GamePrefabCatalog>(GamePrefabCatalog.ResourcePath)),root.transform);
                var first=factory.Create<RecyclingOutputFx>(PrefabId.RecyclingOutput,root.transform);
                int identity=first.GetInstanceID();
                var defaultScale=first.transform.localScale;
                first.transform.localScale=Vector3.one*99;
                first.transform.localRotation=Quaternion.Euler(0,0,75);
                first.Release(); first.Release();
                Require(factory.AvailableCount(PrefabId.RecyclingOutput)==1,"Duplicate return entered pool twice");
                bool rejected=false;
                try { factory.Create<VillageUpgradeFx>(PrefabId.RecyclingOutput,root.transform); }
                catch(InvalidOperationException) { rejected=true; }
                Require(rejected && factory.AvailableCount(PrefabId.RecyclingOutput)==1,"Type mismatch consumed cached object");
                for(int i=0;i<100;i++)
                {
                    var effect=factory.Create<RecyclingOutputFx>(PrefabId.RecyclingOutput,root.transform);
                    Require(effect.GetInstanceID()==identity && effect.gameObject.activeSelf,"Effect not reused");
                    Require(effect.transform.localScale==defaultScale && effect.transform.localRotation==Quaternion.identity,"Effect transform not reset");
                    effect.Release();
                }
                Require(factory.CreatedCount==1 && factory.ReusedCount==100,"Warm pool still instantiates");
                var simultaneous=new List<RecyclingOutputFx>();
                for(int i=0;i<24;i++) simultaneous.Add(factory.Create<RecyclingOutputFx>(PrefabId.RecyclingOutput,root.transform));
                foreach(var effect in simultaneous) effect.Release();
                Require(factory.AvailableCount(PrefabId.RecyclingOutput)==16,"Output pool retained beyond cap");
                VillageUpgradeFx.Play(factory,root.transform,Vector3.zero,4);
                var upgrade=root.GetComponentInChildren<VillageUpgradeFx>(); int upgradeId=upgrade.GetInstanceID();
                upgrade.Release(); VillageUpgradeFx.Play(factory,root.transform,Vector3.one,2);
                upgrade=root.GetComponentInChildren<VillageUpgradeFx>();
                Require(upgrade.GetInstanceID()==upgradeId,"Upgrade effect not reused");
                var sparks=(List<SpriteRenderer>)typeof(VillageUpgradeFx).GetField("_sparks",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(upgrade);
                Require(sparks.Count==14,"Upgrade sparks duplicated on reuse");
                upgrade.Release();

                var gameRoot=new GameObject("Lazy UI validation"); gameRoot.transform.SetParent(root.transform);
                var game=gameRoot.AddComponent<GameManager>(); game.StartGame(20260927,false);
                Require(!game.Recycling.IsViewCreated && gameRoot.GetComponentInChildren<RecyclingMinigameUI>(true)==null,"UI was instantiated before first use");
                RecyclingValidation.PlaceAtStation(game);
                Require(!game.Recycling.Open() && !game.Recycling.IsViewCreated,"Empty interaction allocated UI");
                game.Player.Inventory.Add(new LootItem("캔",100,Color.white,1,MaterialKind.Metal,RecyclingCategory.Can));
                game.Recycling.Deposit(); game.Recycling.Open();
                var view=gameRoot.GetComponentInChildren<RecyclingMinigameUI>();
                Require(view!=null,"Lazy view did not instantiate"); int viewId=view.GetInstanceID();
                game.Recycling.Close(); Require(!view.gameObject.activeSelf,"Closed UI still active");
                for(int i=0;i<10;i++) { game.Recycling.Open(); game.Recycling.Close(); }
                Require(gameRoot.GetComponentsInChildren<RecyclingMinigameUI>(true).Length==1 && view.GetInstanceID()==viewId,"UI duplicated on reopen");
                Require(game.Recycling.Session.Queue.Count==1,"Hiding UI discarded session data");
                Debug.Log("[DigGame] RUNTIME_REUSE_VALIDATION_PASSED: 100 leases = 1 created/100 reused; idle cap 16; duplicate return/type rejection; upgrade reset; lazy UI empty/first-open/10 reopens preserve data.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Random.state=random; }
        }
    }
}
