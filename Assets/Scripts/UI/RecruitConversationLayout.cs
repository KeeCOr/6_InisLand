using UnityEngine;
namespace IL6
{
    public struct RecruitConversationLayout
    {
        public Rect Character,Bubble,Name,Role,Dialogue,Details,Accept,Decline;
        public static RecruitConversationLayout ForSize(float width,float height)
        {
            float total=Mathf.Min(1080,width-48),left=(width-total)/2;
            float portraitWidth=Mathf.Min(320,total*.30f);
            var character=new Rect(left,height-424,portraitWidth,408);
            var bubble=new Rect(character.xMax+12,height-312,total-portraitWidth-12,288);
            // Keep text and choices clear of the raster bubble's tail and corner ornaments.
            float inset=bubble.width*.16f;
            float x=bubble.x+inset,w=bubble.width-2*inset;
            return new RecruitConversationLayout {
                Character=character,Bubble=bubble,
                Name=new Rect(x,bubble.y+22,w-148,32),
                Role=new Rect(x+w-132,bubble.y+26,132,24),
                Dialogue=new Rect(x,bubble.y+70,w,84),
                Details=new Rect(x,bubble.y+168,w,24),
                Accept=new Rect(x,bubble.y+212,(w-12)*.62f,50),
                Decline=new Rect(x+(w-12)*.62f+12,bubble.y+212,(w-12)*.38f,50)
            };
        }
    }
}
