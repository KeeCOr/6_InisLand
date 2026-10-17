using System.Globalization;
using UnityEngine;
namespace IL6
{
    public static class ResourceHudFormat
    {
        public static string Compact(int value)
        {
            value=Mathf.Max(0,value);
            if(value<1000) return value.ToString(CultureInfo.InvariantCulture);
            double unit=value>=1000000?1000000d:1000d;
            return (System.Math.Floor(value/unit*10)/10).ToString("0.#",CultureInfo.InvariantCulture)+(unit==1000?"K":"M");
        }
    }
}
