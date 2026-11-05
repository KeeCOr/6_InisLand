using System;
using UnityEngine;
namespace IL6
{
    public static class QuestTrackerLayout
    {
        public static Rect Panel(float width,bool collapsed,bool complete=false,int nextCount=2)
            =>new Rect(width-296,150,280,complete?112:collapsed?148:nextCount>0?252+24*Mathf.Min(2,nextCount):216);
        public static Rect Header(Rect p)=>new Rect(p.x+42,p.y+4,p.width-94,32);
        public static Rect Count(Rect p)=>new Rect(p.xMax-72,p.y+48,56,24);
        public static Rect Toggle(Rect p)=>new Rect(p.xMax-44,p.y,44,44);
        public static Rect Title(Rect p)=>new Rect(p.x+16,p.y+48,p.width-32,p.height<200?24:28);
        public static Rect Detail(Rect p)=>new Rect(p.x+16,p.y+80,p.width-32,44);
        public static Rect Condition(Rect p,bool collapsed,int i)=>new Rect(p.x+16,p.y+(collapsed?74:132)+i*(collapsed?20:22),p.width-32,collapsed?20:22);
        public static Rect Next(Rect p,int i)=>new Rect(p.x+16,p.y+240+i*24,p.width-32,22);
        public readonly struct ProgressRow
        {
            public readonly string Label,Value;
            public readonly bool Complete;
            public ProgressRow(string label,string value,bool complete){Label=label;Value=value;Complete=complete;}
        }
        public static ProgressRow[] Rows(string progress)
        {
            var parts=(progress??"").Split(new[]{"  "},StringSplitOptions.RemoveEmptyEntries);
            var rows=new ProgressRow[parts.Length];
            for(int i=0;i<parts.Length;i++)
            {
                string part=parts[i].Trim();int space=part.LastIndexOf(' ');
                string label=space>0?part.Substring(0,space):part,value=space>0?part.Substring(space+1):"";
                var ratio=value.Replace("Lv.","").Split('/');
                bool complete=ratio.Length==2 && int.TryParse(ratio[0],out int current) && int.TryParse(ratio[1],out int required) && required>0 && current>=required;
                rows[i]=new ProgressRow(label,value,complete);
            }
            return rows;
        }
    }
}
