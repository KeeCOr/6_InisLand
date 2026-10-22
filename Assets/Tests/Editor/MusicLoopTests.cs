using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public class MusicLoopTests
    {
        [TestCase(true,11,196f,247f,294f)]
        [TestCase(false,23,110f,131f,156f)]
        [TestCase(false,31,165f,196f,247f)]
        [TestCase(true,41,220f,277f,330f)]
        public void GeneratedLoopKeepsSoundAcrossItsBoundary(bool peaceful,int seed,float a,float b,float c)
        {
            var method=typeof(Music).GetMethod("MakeLoop",BindingFlags.Static|BindingFlags.NonPublic);
            var clip=(AudioClip)method.Invoke(null,new object[]{new[]{a,b,c},peaceful,seed});
            try
            {
                var data=new float[clip.samples];Assert.That(clip.GetData(data,0),Is.True);
                int window=Mathf.RoundToInt(clip.frequency*.08f);
                float middle=Rms(data,clip.samples/2,window);
                Assert.That(Rms(data,0,window),Is.GreaterThan(middle*.35f),"The loop start must not fade from silence");
                Assert.That(Rms(data,data.Length-window,window),Is.GreaterThan(middle*.35f),"The loop end must not fade to silence");
                float localStep=0;
                for(int i=1;i<window;i++) localStep=Mathf.Max(localStep,Mathf.Abs(data[i]-data[i-1]),Mathf.Abs(data[data.Length-i]-data[data.Length-i-1]));
                Assert.That(Mathf.Abs(data[0]-data[data.Length-1]),Is.LessThanOrEqualTo(localStep*1.2f+.0001f),"Wrapping must not introduce a click-sized jump");
                float peak=0;foreach(float sample in data) peak=Mathf.Max(peak,Mathf.Abs(sample));
                Assert.That(peak,Is.LessThanOrEqualTo(.701f));
            }
            finally { Object.DestroyImmediate(clip); }
        }
        private static float Rms(float[] data,int start,int count)
        {
            double sum=0;for(int i=start;i<start+count;i++) sum+=data[i]*data[i];return Mathf.Sqrt((float)(sum/count));
        }
    }
}
