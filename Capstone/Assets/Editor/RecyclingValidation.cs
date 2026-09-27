using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace DigGame.Editor
{
    public static class RecyclingValidation
    {
        sealed class Rewards : IRecyclingRewardSink
        {
            public int Calls;
            public int Pay(LootItem item) { Calls++; return item.Value; }
        }
        static LootItem Item(RecyclingCategory category) => new LootItem(category.ToString(),100,Color.white,1,MaterialKind.Metal,category);
        static void Check(bool value,string message) { if (!value) throw new InvalidOperationException(message); }
        [MenuItem("DigGame/Recycling/Validate Domain and Integration")]
        public static void Run()
        {
            var q = new RecyclingQueue(); var rewards = new Rewards(); var pollution = new PollutionMeter();
            var s = new RecyclingSession(q,pollution,rewards);
            Check(!s.Open(),"Empty queue opened");
            for (int i=1;i<=5;i++) q.Enqueue(new[]{Item((RecyclingCategory)i)});
            Check(rewards.Calls==0 && s.Open(),"Deposit paid or open failed");
            Check(!s.Submit(RecyclingCategory.Specialist),"Invalid input accepted");
            for (int i=1;i<=5;i++)
            {
                Check(s.Current(out var item) && (int)item.Category==i,"FIFO mismatch");
                Check(s.Submit((RecyclingCategory)i),"Correct category rejected");
                Check(!s.Submit((RecyclingCategory)i),"Transition accepted repeated input");
                Check(rewards.Calls==i,"Duplicate or absent payment");
                s.Advance(.2f); Check(s.State==RecyclingState.Transition,"Transition too short");
                s.Advance(.3f);
            }
            Check(s.State==RecyclingState.Completed && pollution.Value==0,"Completion mismatch");
            s.Close(); q.Enqueue(new[]{Item(RecyclingCategory.Can),Item(RecyclingCategory.Paper)}); s.Open();
            s.Submit(RecyclingCategory.Pet); s.Close();
            Check(q.Count==1 && pollution.Value==5 && rewards.Calls==5,"Failure/Escape lost state");
            s.Open(); Check(s.Current(out var remaining) && remaining.Category==RecyclingCategory.Paper,"Resume FIFO mismatch");
            s.Submit(RecyclingCategory.Paper); s.Close(); s.Advance(1);
            Check(rewards.Calls==6 && q.Count==0 && !s.Open(),"Closing transition duplicated payment");
            q.Enqueue(new[]{Item(RecyclingCategory.Specialist),Item(RecyclingCategory.Can)}); s.Open();
            Check(s.State==RecyclingState.Transition && rewards.Calls==7 && q.Count==1,"Specialist path mismatch");
            s.Advance(1); Check(s.State==RecyclingState.Ready,"Specialist swallowed next item");
            for (int i=0;i<30;i++) pollution.Add(5);
            pollution.Add(-100); Check(pollution.Value==100,"Pollution not clamped");

            var root = new GameObject("Recycling Integration Validation");
            var previousRandom=UnityEngine.Random.state;
            try
            {
                var game = root.AddComponent<GameManager>(); game.StartGame(20260927,false);
                var controller=game.Recycling;
                Check(!controller.Open(),"Far interaction accepted");
                PlaceAtStation(game);
                game.Player.Inventory.Add(Item(RecyclingCategory.Can));
                game.Player.Inventory.Add(Item(RecyclingCategory.Paper));
                int money=game.Money, delivered=game.DeliveredToday;
                Check(controller.Deposit() && game.Player.Inventory.Count==0 && game.Money==money,"Deposit sold items");
                Check(controller.Open() && !controller.Deposit(),"Modal accepted another deposit");
                controller.Submit(RecyclingCategory.Can);
                Check(game.Money>money && game.DeliveredToday>delivered,"Reward/quota integration missing");
                controller.Close(); controller.Open(); controller.Submit(RecyclingCategory.Can);
                Check(controller.Session.Pollution.Value==5,"Wrong answer pollution missing");
                controller.Close();
                controller.Session.Queue.Enqueue(new[]{Item(RecyclingCategory.Pet)}); controller.Open();
                game.AddRelicIncome(game.BaseQuota,"검증");
                typeof(GameManager).GetMethod("DoEndDay",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
                Check(!controller.IsOpen && controller.Session.Queue.Count==1 && game.State==GameState.Night,"Day end discarded queue");
                game.StartNextDay();
                Check(controller.Session.Queue.Count==1 && controller.Session.Pollution.Value==5,"Next day reset queue/pollution");
                PlaceAtStation(game); controller.Open(); game.GameOver("검증");
                Check(!controller.IsOpen,"Game over left modal open");
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssets.PathFor(PrefabId.RecyclingStation));
                var data=new SerializedObject(prefab.GetComponent<RecyclingAudioFeedback>());
                Check(data.FindProperty("_success").objectReferenceValue!=null && data.FindProperty("_failure").objectReferenceValue!=null,"Audio references missing");
                Check(new SerializedObject(prefab.GetComponent<RecyclingMinigameController>()).FindProperty("_viewPrefab").objectReferenceValue!=null,"View prefab missing");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Random.state=previousRandom; }
            Debug.Log("[DigGame] RECYCLING_VALIDATION_PASSED: five categories, FIFO, rewards, mistakes, clamp, lock, resume, specialist, proximity, modal, day retention, audio references.");
        }
        public static void PlaceAtStation(GameManager game)
        {
            var p=game.Player.transform.position;
            p.x=(game.RecycleDropCell.x+.5f)*GameConfig.CellSize;
            game.Player.transform.position=p;
            typeof(PlayerController).GetMethod("RefreshCell",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game.Player,null);
            Check(game.PlayerNearRecycle(),"Test station position invalid");
        }
    }
}
