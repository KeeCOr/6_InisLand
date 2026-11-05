using UnityEngine;
namespace IL6
{
    public enum StoryTutorialStage { Prologue, Explore, WoodMemory, GatherWood, BuildMemory, BuildFence, Resolve, Complete }
    public sealed class StoryTutorial
    {
        public readonly struct Line
        {
            public readonly string Speaker,Text,Chapter;
            public Line(string speaker,string text,string chapter) {Speaker=speaker;Text=text;Chapter=chapter;}
        }
        private static readonly Line[] Prologue={
            new Line("Hero","아버지는 마을 사람들과 함께 떠났다. 돌아올 테니, 이곳에서 기다려 달라고 했다.","떠나던 날"),
            new Line("Hero","발자국이 보이지 않을 때까지 문 앞에 서 있었다. 이제 마을에는 나 혼자뿐이다.","떠나던 날"),
            new Line("Hero","꼭 돌아온다고 했으니까. 그때까지 우리 집과 마을을 잘 지키고 있어야지.","떠나던 날"),
            new Line("Hero","모두 떠나고 나만 남았다. 그러던 어느 날, 눈폭풍이 마을을 덮쳤다.","눈폭풍"),
            new Line("Hero","마을이… 온통 망가졌어. 그래도 집 한 채와 울타리, 불씨는 남았네.","폭풍이 지나간 뒤"),
            new Line("Hero","이대로라면 아버지가 돌아와도 우리 마을을 알아보지 못할 거야.","폭풍이 지나간 뒤"),
            new Line("Hero","다시 만들 거야. 아버지가 돌아올 우리 마을을.","다시 세우는 약속")};
        private static readonly Line[] Wood={
            new Line("Hero","아버지는 늘 쓸 만한 나무부터 모으셨지. 나도 그렇게 시작해 보자.","첫 번째 준비"),
            new Line("Hero","나무를 베고 떨어진 목재를 주워야겠어. 작은 일부터 해 보자.","첫 번째 준비")};
        private static readonly Line[] Build={
            new Line("Hero","이 목재라면 울타리 한 칸은 세울 수 있겠어.","다시 세우는 첫 울타리"),
            new Line("Hero","한꺼번에 다 고칠 수는 없어. 우선 한 칸… 하나씩 세우면 다시 마을이 될 거야.","다시 세우는 첫 울타리")};
        private static readonly Line[] Ending={
            new Line("Hero","겨우 한 칸이지만… 이제 시작이야.","돌아올 곳"),
            new Line("Hero","밤에는 숲에서 로봇들이 몰려와. 불씨와 새로 지은 마을을 지켜야 해.","첫날 밤을 향해"),
            new Line("Hero","길에서 만난 사람들과 힘을 모으면 할 수 있을 거야. 아버지, 기다리고 있을게.","돌아올 곳")};
        public StoryTutorialStage Stage { get; private set; }
        public int LineIndex { get; private set; }
        public bool IsComplete => Stage==StoryTutorialStage.Complete;
        public bool IsDialogue => Stage==StoryTutorialStage.Prologue || Stage==StoryTutorialStage.WoodMemory || Stage==StoryTutorialStage.BuildMemory || Stage==StoryTutorialStage.Resolve;
        public bool StormRevealed => Stage>StoryTutorialStage.Prologue || LineIndex>=4;
        public float Walked { get; private set; }
        public int CollectedWood { get; private set; }
        public Line CurrentLine => Lines[Mathf.Clamp(LineIndex,0,Lines.Length-1)];
        public string IllustrationKey => Stage==StoryTutorialStage.Prologue
            ? (LineIndex<3?"waiting":LineIndex==3?"storm":LineIndex<6?"ruins":"promise")
            : Stage==StoryTutorialStage.WoodMemory?"ruins":"promise";
        public string IllustrationPath => "UI/StoryIllustrations/"+IllustrationKey;
        private Line[] Lines => Stage==StoryTutorialStage.WoodMemory?Wood:Stage==StoryTutorialStage.BuildMemory?Build:Stage==StoryTutorialStage.Resolve?Ending:Prologue;
        public string Objective => Stage==StoryTutorialStage.Explore?"마을의 흔적 둘러보기":Stage==StoryTutorialStage.GatherWood?"첫 목재 줍기":"울타리 한 칸 세우기";
        public string Hint => Stage==StoryTutorialStage.Explore?"이동하며 주변을 살펴보세요 · WASD / 이동 패드":Stage==StoryTutorialStage.GatherWood?"나무 근처에서 Space / 행동 → 떨어진 목재에 다가가기":"3 / 건설 망치 → 나무 울타리 → 위치 선택·설치";
        public StoryTutorial(int stage=0) {Stage=(StoryTutorialStage)Mathf.Clamp(stage,0,(int)StoryTutorialStage.Complete);}
        public bool AdvanceDialogue()
        {
            if(!IsDialogue) return false;
            if(++LineIndex>=Lines.Length) {Stage++;LineIndex=0;return true;}
            return false;
        }
        public bool Observe(float distance,int woodDelivered,bool newFence)
        {
            if(IsDialogue || IsComplete) return false;
            if(Stage==StoryTutorialStage.Explore) Walked+=Mathf.Max(0,distance);
            if(Stage==StoryTutorialStage.GatherWood) CollectedWood+=Mathf.Max(0,woodDelivered);
            bool done=Stage==StoryTutorialStage.Explore?Walked>=2.5f:Stage==StoryTutorialStage.GatherWood?CollectedWood>=1:newFence;
            if(!done) return false;
            Stage++;LineIndex=0;return true;
        }
        public void Skip() {Stage=StoryTutorialStage.Complete;LineIndex=0;}
    }
}
