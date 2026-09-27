using UnityEngine;
namespace DigGame
{
    /// <summary>표시만 담당하며 판정/보상/대기열 변경은 컨트롤러에 위임한다.</summary>
    public sealed class RecyclingMinigameUI : MonoBehaviour
    {
        RecyclingMinigameController _controller;
        GUIStyle _label, _title, _button;
        static readonly string[] Names = { "캔", "페트병", "종이", "플라스틱", "알루미늄" };
        public void Bind(RecyclingMinigameController controller) => _controller = controller;
        void OnGUI()
        {
            if (_controller == null || !_controller.IsOpen) return;
            if (_label == null)
            {
                var font = _controller.Game.Hud != null ? _controller.Game.Hud.UIFont : null;
                _label = new GUIStyle(GUI.skin.label) { font = font, fontSize = 19, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                _title = new GUIStyle(_label) { fontSize = 32, fontStyle = FontStyle.Bold };
                _button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 20, wordWrap = true };
            }
            var matrix = GUI.matrix; var color = GUI.color; bool enabled = GUI.enabled; int depth = GUI.depth;
            try
            {
                GUI.depth = -100;
                Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(.015f,.025f,.04f,.96f));
                float scale = Mathf.Min(Screen.width / 1000f, Screen.height / 700f);
                GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width-1000*scale)/2, (Screen.height-700*scale)/2), Quaternion.identity, Vector3.one*scale);
                var session = _controller.Session;
                GUI.Label(new Rect(60,25,650,50), "분리수거 작업대", _title);
                if (GUI.Button(new Rect(835,30,120,42), "닫기  Esc", _button)) { _controller.Close(); return; }
                GUI.Label(new Rect(60,80,880,35), $"대기 {session.Queue.Count}개   ·   오염도 {session.Pollution.Value}/100   ·   마감까지 {Mathf.CeilToInt(_controller.Game.TimeLeft)}초", _label);
                Fill(new Rect(60,120,880,8), new Color(.15f,.2f,.25f));
                Fill(new Rect(60,120,880*session.Pollution.Value/100f,8), new Color(.95f,.3f,.27f));
                if (session.State == RecyclingState.Completed)
                {
                    GUI.Label(new Rect(100,210,800,90), "분리수거 완료!", _title);
                    GUI.Label(new Rect(150,320,700,60), "보관한 쓰레기를 모두 처리했습니다.", _label);
                    if (GUI.Button(new Rect(350,430,300,55), "돌아가기  Enter", _button)) _controller.Close();
                    return;
                }
                Rect current = new Rect(250,170,290,290), next = new Rect(660,225,150,150);
                bool moving = session.State == RecyclingState.Transition;
                float t = moving ? Mathf.SmoothStep(0,1,session.Progress) : 0;
                if (session.Current(out var item))
                {
                    var rect = current; rect.x -= t*170;
                    Card(rect,item,1-t,moving ? (session.LastResult.Correct ? new Color(.2f,.8f,.5f) : new Color(.95f,.3f,.3f)) : new Color(.27f,.4f,.5f));
                }
                if (session.Next(out var upcoming))
                {
                    var rect = new Rect(Mathf.Lerp(next.x,current.x,t),Mathf.Lerp(next.y,current.y,t),Mathf.Lerp(next.width,current.width,t),Mathf.Lerp(next.height,current.height,t));
                    Card(rect,upcoming,1,new Color(.27f,.4f,.5f));
                    if (!moving) GUI.Label(new Rect(650,180,170,35), "다음 물건", _label);
                }
                if (moving && session.Queue.TryPeek(1,out var later)) Card(next,later,t,new Color(.27f,.4f,.5f));
                string message = moving ? (session.LastResult.Specialist ? $"전문 처리 완료  +{session.LastResult.Reward:N0}원" :
                    session.LastResult.Correct ? $"정확한 분류!  +{session.LastResult.Reward:N0}원" : "분류 실패 · 보상 없이 폐기 / 오염도 상승") : "물건을 보고 알맞은 분류 키를 누르세요";
                GUI.Label(new Rect(50,485,900,45),message,_label);
                GUI.enabled = !moving;
                for (int i=0;i<5;i++)
                    if (GUI.Button(new Rect(55+i*180,550,170,70),$"{i+1}\n{Names[i]}",_button))
                        _controller.Submit((RecyclingCategory)(i+1));
                GUI.enabled = true;
                GUI.Label(new Rect(60,635,880,35),"분류 중에도 마감 시간은 진행됩니다 · Esc로 나가도 남은 물건은 보관됩니다",_label);
            }
            finally { GUI.matrix=matrix; GUI.color=color; GUI.enabled=enabled; GUI.depth=depth; }
        }
        void Card(Rect rect, LootItem item, float alpha, Color border)
        {
            Color old=GUI.color; GUI.color=new Color(1,1,1,alpha);
            Fill(rect,border); var inner=new Rect(rect.x+4,rect.y+4,rect.width-8,rect.height-8);
            Fill(inner,new Color(.09f,.13f,.18f));
            Sprite sprite=TrashSpriteCatalog.For(item);
            if (sprite!=null)
            {
                Rect pixels=sprite.rect; float fit=Mathf.Min((rect.width-32)/pixels.width,(rect.height-70)/pixels.height);
                var image=new Rect(rect.center.x-pixels.width*fit/2,rect.y+18,pixels.width*fit,pixels.height*fit);
                GUI.DrawTextureWithTexCoords(image,sprite.texture,new Rect(pixels.x/sprite.texture.width,pixels.y/sprite.texture.height,pixels.width/sprite.texture.width,pixels.height/sprite.texture.height));
            }
            var style=new GUIStyle(_label){fontSize=rect.width<200 ? 15 : 21};
            GUI.Label(new Rect(rect.x+8,rect.yMax-55,rect.width-16,50),item.Name,style);
            GUI.color=old;
        }
        static void Fill(Rect rect,Color color)
        {
            var old=GUI.color; GUI.color=old*color; GUI.DrawTexture(rect,Texture2D.whiteTexture); GUI.color=old;
        }
    }
}
