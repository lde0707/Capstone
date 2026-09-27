using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace DigGame.Editor
{
    /// <summary>입력을 예약하고 실제 PlayerLoop 프레임 이후 결과를 확인한다.</summary>
    public sealed class RecyclingInputProbe : IDisposable
    {
        Keyboard _keyboard;
        InputSettings.UpdateMode _previousUpdateMode;
        InputSettings.BackgroundBehavior _previousBackground;
        bool _previousRunInBackground;
        InputSettings.EditorInputBehaviorInPlayMode _previousBehavior;
        public bool Completed { get; private set; }
        public string Failure { get; private set; }
        public IEnumerator Run()
        {
            _previousBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;
            _previousUpdateMode=InputSystem.settings.updateMode;
            _previousBackground=InputSystem.settings.backgroundBehavior;
            _previousRunInBackground=Application.runInBackground;
            Application.runInBackground=true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsManually;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            _keyboard=InputSystem.AddDevice<Keyboard>();
            var test=Exercise();
            while (true)
            {
                bool next;
                try { next=test.MoveNext(); }
                catch (Exception e) { Failure=e.Message; next=false; }
                if (!next) { Cleanup(); Completed=true; yield break; }
                yield return test.Current;
            }
        }
        IEnumerator Exercise()
        {
            var game=GameManager.Instance;
            RecyclingValidation.PlaceAtStation(game);
            game.Player.TakeAll();
            game.Player.Inventory.Add(new LootItem("캔 검증",100,Color.white,1,MaterialKind.Metal,RecyclingCategory.Can));
            game.Player.Inventory.Add(new LootItem("종이 검증",100,Color.white,1,MaterialKind.General,RecyclingCategory.Paper));
            game.Player.Gear.EnergyDrinks=2;
            Send(Key.E); yield return null;
            Check(game.Recycling.Session.Queue.Count==2,$"E deposit failed: queue={game.Recycling.Session.Queue.Count}, pressed={_keyboard.eKey.wasPressedThisFrame}");
            Send(Key.F); yield return null;
            Check(game.Recycling.IsOpen,"F did not open");
            var position=game.Player.transform.position;
            Send(Key.D,Key.Q,Key.B,Key.H,Key.J,Key.E); yield return null;
            Send(Key.F); yield return null;
            Check(game.Player.transform.position==position && game.Player.Gear.EnergyDrinks==2 && !game.ShopOpen
                && game.Workers.Count==0 && game.Recycling.Session.Queue.Count==2,"Modal leaked gameplay input");
            Send(Key.Digit1,Key.Digit3); yield return null;
            Check(game.Recycling.Session.Queue.Count==1,"Simultaneous number keys did not resolve exactly one item");
            Send(); yield return null;
            Send(Key.Digit3); yield return null;
            Check(game.Recycling.Session.Queue.Count==1,"Transition accepted repeated input");
            Send(Key.Escape); yield return null;
            Check(!game.Recycling.IsOpen && game.Recycling.InputCaptured,"Escape did not close/capture frame");
            Send(Key.F); yield return null;
            Check(game.Recycling.IsOpen && game.Recycling.Session.Current(out var item) && item.Category==RecyclingCategory.Paper,"F resume failed");
            Send(Key.Digit3); yield return null;
            yield return new WaitForSeconds(.65f);
            Check(game.Recycling.Session.State==RecyclingState.Completed,"Last item completion failed");
            Send(Key.Enter); yield return null;
            Check(!game.Recycling.IsOpen,"Completion confirmation did not close");
            Send(); yield return null;
            position=game.Player.transform.position;
            Send(Key.D); yield return null;
            Check(game.Player.transform.position!=position,"Movement not restored after close");
            Send();

            var pool=(PooledGameObjectFactory)game.Prefabs;
            var effect=pool.Create<RecyclingOutputFx>(PrefabId.RecyclingOutput,game.transform);
            yield return new WaitForSeconds(2f);
            Check(effect!=null && !effect.gameObject.activeSelf,"Expired effect was not returned to pool");
            int reused=pool.ReusedCount;
            effect=pool.Create<RecyclingOutputFx>(PrefabId.RecyclingOutput,game.transform);
            Check(pool.ReusedCount==reused+1 && effect.GetComponent<SpriteRenderer>().color.a==1f,"Effect reuse did not reset alpha");
            yield return new WaitForSeconds(.2f);
            Check(effect.gameObject.activeSelf,"Reused effect retained expired lifetime");
            effect.Release();
            Debug.Log("[DigGame] RUNTIME_POOL_PLAYMODE_PASSED: expiry returns to pool; reused alpha/lifetime reset.");
            Debug.Log("[DigGame] RECYCLING_INPUT_VALIDATION_PASSED: real frames E/F, movement/tools/hiring lock, simultaneous keys, transition lock, Escape/resume, completion and restored movement.");
        }
        void Send(params Key[] keys)
        {
            InputSystem.QueueStateEvent(_keyboard,new KeyboardState(keys));
            InputSystem.Update();
        }
        static void Check(bool value,string message) { if (!value) throw new InvalidOperationException(message); }
        void Cleanup()
        {
            if (_keyboard==null) return;
            InputSystem.RemoveDevice(_keyboard); _keyboard=null;
            InputSystem.settings.editorInputBehaviorInPlayMode=_previousBehavior;
            InputSystem.settings.updateMode=_previousUpdateMode;
            InputSystem.settings.backgroundBehavior=_previousBackground;
            Application.runInBackground=_previousRunInBackground;
        }
        public void Dispose() => Cleanup();
    }
}
