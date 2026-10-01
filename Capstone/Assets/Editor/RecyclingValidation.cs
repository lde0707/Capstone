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
            Check(q.Count==1 && pollution.Value==1 && rewards.Calls==5,"Failure/Escape lost state");
            s.Open(); Check(s.Current(out var remaining) && remaining.Category==RecyclingCategory.Paper,"Resume FIFO mismatch");
            s.Submit(RecyclingCategory.Paper); s.Close(); s.Advance(1);
            Check(rewards.Calls==6 && q.Count==0 && !s.Open(),"Closing transition duplicated payment");
            q.Enqueue(new[]{Item(RecyclingCategory.Specialist),Item(RecyclingCategory.Can)}); s.Open();
            Check(s.State==RecyclingState.Transition && rewards.Calls==7 && q.Count==1,"Specialist path mismatch");
            s.Advance(1); Check(s.State==RecyclingState.Ready,"Specialist swallowed next item");
            for (int i=0;i<30;i++) pollution.Add(1);
            Check(pollution.Value==PollutionMeter.MaxValue,"Pollution upper clamp failed");
            pollution.Add(-100); Check(pollution.Value==0,"Pollution recovery clamp failed");

            s.Close(); q.Enqueue(new[]{Item(RecyclingCategory.Plastic)}); s.Open();
            s.Advance(1.9f);
            Check(s.State==RecyclingState.Ready && rewards.Calls==7,"Answer timer expired too early");
            s.Advance(.1f);
            Check(s.State==RecyclingState.Transition && s.LastResult.TimedOut && !s.LastResult.Correct,
                "Two-second timeout did not fail the item");
            Check(rewards.Calls==7 && pollution.Value==1 && q.Count==0,
                "Timeout paid a reward, skipped pollution, or kept the item");

            var capped = new RecyclingQueue();
            var overflow = new LootItem[RecyclingQueue.MaxCapacity + 1];
            for (int i=0;i<overflow.Length;i++) overflow[i]=Item(RecyclingCategory.Can);
            Check(capped.Enqueue(overflow)==RecyclingQueue.MaxCapacity && capped.IsFull && capped.RemainingCapacity==0,
                "Recycling queue exceeded or missed its 30-item capacity");

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
                for (int i=0;i<RecyclingQueue.MaxCapacity-1;i++)
                    game.Player.Inventory.Add(Item(RecyclingCategory.Can));
                int money=game.Money, delivered=game.DeliveredToday;
                Check(controller.Deposit() && controller.Session.Queue.Count==RecyclingQueue.MaxCapacity &&
                    game.Player.Inventory.Count==1 && game.Money==money,"Partial capped deposit failed or sold items");
                Check(controller.Open() && !controller.Deposit(),"Modal accepted another deposit");
                controller.Submit(RecyclingCategory.Can);
                Check(game.Money>money && game.DeliveredToday>delivered,"Reward/quota integration missing");
                controller.Close();
                Check(controller.Deposit() && controller.Session.Queue.IsFull && game.Player.Inventory.Count==0,
                    "Freed queue slot did not accept the remaining bag item");
                controller.Open(); controller.Submit(RecyclingCategory.Can);
                Check(controller.Session.Pollution.Value==1,"Wrong answer pollution missing");
                controller.Close();
                controller.Session.Queue.Enqueue(new[]{Item(RecyclingCategory.Pet)}); controller.Open();
                game.AddRelicIncome(game.BaseQuota,"검증");
                typeof(GameManager).GetMethod("DoEndDay",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
                Check(!controller.IsOpen && controller.Session.Queue.Count==1 && game.State==GameState.Night,"Day end discarded queue");
                game.StartNextDay();
                Check(controller.Session.Queue.Count==1 && controller.Session.Pollution.Value==2,"Overnight pollution or queue retention failed");
                PlaceAtStation(game); controller.Open(); game.GameOver("검증");
                Check(!controller.IsOpen,"Game over left modal open");
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssets.PathFor(PrefabId.RecyclingStation));
                var data=new SerializedObject(prefab.GetComponent<RecyclingAudioFeedback>());
                Check(data.FindProperty("_success").objectReferenceValue!=null && data.FindProperty("_failure").objectReferenceValue!=null,"Audio references missing");
                Check(new SerializedObject(prefab.GetComponent<RecyclingMinigameController>()).FindProperty("_viewPrefab").objectReferenceValue!=null,"View prefab missing");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Random.state=previousRandom; }
            Debug.Log("[DigGame] RECYCLING_VALIDATION_PASSED: five categories, FIFO, 30-item capacity/partial deposit, two-second timeout, rewards, pollution recovery/mistakes/overnight, clamp, lock, resume, specialist, proximity, modal, audio references.");
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
