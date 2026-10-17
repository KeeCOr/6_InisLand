using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests {
public class HudTextFitTests {
[TestCase("999.9K",69,30,17)]
[TestCase("자동 ON",66,22,12)]
[TestCase("자원 · 보유량 / 보관 한도",300,24,16)]
[TestCase("정착지 목표를 달성하고 다음 지역으로 이동하세요",240,18,13)]
public void LabelsMeasureInsideTheirBounds(string text,float width,float height,int size) {
var source=new GUIStyle {font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),fontSize=size};
var rect=new Rect(0,0,width,height);var content=new GUIContent(text);
var fit=HudText.Fit(rect,content,source);
Assert.That(fit.CalcSize(content).x,Is.LessThanOrEqualTo(width));
Assert.That(fit.CalcHeight(content,width-2),Is.LessThanOrEqualTo(height));
Assert.That(fit.padding.horizontal,Is.Zero);
Assert.That(source.fontSize,Is.EqualTo(size));
}
}}
