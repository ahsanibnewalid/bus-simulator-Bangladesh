using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using Nusantara.Editor;
namespace Nusantara.Tests {
public class SimulationTests {
    [TestCase(39.99f,0L)] [TestCase(40f,72000L)] [TestCase(87f,156600L)] [TestCase(100f,180000L)]
    public void ComfortDeterminesFare(float comfort,long expected) {Assert.AreEqual(expected,EconomyRules.Payout(10,18000,comfort));}
    [Test] public void NoNegativeOrInvalidRewards() {
        Assert.AreEqual(0,EconomyRules.Payout(-2,18000,100));Assert.AreEqual(0,EconomyRules.Payout(45,18000,float.NaN));
        Assert.AreEqual(810000,EconomyRules.Payout(45,18000,120));
    }
    [Test] public void HarshInputsReduceComfortButSmoothDrivingRecovers() {
        Assert.Less(EconomyRules.ComfortDelta(-.7f,0,40,false),0);
        Assert.Less(EconomyRules.ComfortDelta(0,.5f,40,false),0);
        Assert.Less(EconomyRules.ComfortDelta(0,0,100,false),0);
        Assert.Greater(EconomyRules.ComfortDelta(0,0,40,false),0);
        Assert.Greater(EconomyRules.ComfortDelta(0,.1f,40,true),EconomyRules.ComfortDelta(0,.1f,40,false));
    }
    [Test] public void BezierEndpointsAndContinuity() {
        var road=new RoadSegment{a=Vector3.zero,b=Vector3.forward*3,c=Vector3.forward*7,d=Vector3.forward*10};
        Assert.AreEqual(road.a,road.Point(0));Assert.AreEqual(road.d,road.Point(1));Assert.AreEqual(Vector3.forward,road.Tangent(.5f));
        Assert.AreEqual(road.a,road.Point(-1));Assert.AreEqual(road.d,road.Point(2));
    }
    [Test] public void RejectUnknownImagesAndReadPngDimensionsBeforeDecoding() {
        Assert.Throws<IOException>(()=>LiveryManager.ReadDimensions(new byte[32],out _,out _));
        var png=new byte[24];png[0]=137;png[1]=80;png[2]=78;png[3]=71;png[18]=4;png[22]=4;
        LiveryManager.ReadDimensions(png,out int w,out int h);Assert.AreEqual(1024,w);Assert.AreEqual(1024,h);
    }
    class FakeAds:IRewardedAds {public bool IsReady=>true;public Action<bool> callback;public void Show(Action<bool> completed){callback=completed;}}
    [Test] public void RewardsRequireProviderAndOnlyCompleteOnce() {
        var go=new GameObject("Commerce test");
        try {
            var commerce=go.AddComponent<Commerce>();int grants=0;
            Assert.IsFalse(commerce.RewardReady);commerce.Reward(()=>grants++);Assert.AreEqual(0,grants);
            var provider=new FakeAds();commerce.Ads=provider;commerce.Reward(()=>grants++);var first=provider.callback;
            Assert.IsFalse(commerce.RewardReady);first(true);first(true);Assert.AreEqual(1,grants);
            commerce.Reward(()=>grants++);first(true);Assert.AreEqual(1,grants);provider.callback(false);Assert.AreEqual(1,grants);
        }finally{UnityEngine.Object.DestroyImmediate(go);}
    }
    [Test] public void GeneratedProjectHasConnectedPlayerPoolAndUrp() {
        ProjectBuilder.Generate();
        var bus=UnityEngine.Object.FindFirstObjectByType<BusController>();Assert.NotNull(bus);Assert.AreEqual(6,bus.wheels.Length);
        foreach(var wheel in bus.wheels)Assert.NotNull(wheel);
        var passengers=bus.GetComponent<PassengerManager>();Assert.AreEqual(45,passengers.passengerPool.Length);
        Assert.AreEqual(20,UnityEngine.Object.FindFirstObjectByType<TrafficSpawner>().pool.Length);
        Assert.AreEqual(4,UnityEngine.Object.FindFirstObjectByType<SectorStreamer>().sectors.Length);
        Assert.IsInstanceOf<UniversalRenderPipelineAsset>(GraphicsSettings.defaultRenderPipeline);
        Assert.IsTrue(File.Exists(ProjectBuilder.BootPath));
    }
}
}
