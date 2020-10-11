#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;


class IOSBuildEditor
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget != BuildTarget.iOS)
        {
            return;
        }
        //获得proj文件
        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject xcodeProj = new PBXProject();
        xcodeProj.ReadFromString(File.ReadAllText(projPath));

        var targetName = xcodeProj.TargetGuidByName(PBXProject.GetUnityTargetName());
        //添加系统依赖库
        xcodeProj.AddFrameworkToProject(targetName, "libxml2.tbd", true);
        xcodeProj.AddFrameworkToProject(targetName, "VideoToolbox.framework", true);
        xcodeProj.AddFrameworkToProject(targetName, "Accelerate.framework", true);
        xcodeProj.AddFrameworkToProject(targetName, "UserNotifications.framework", true);
        //添加第三方库
        //xcodeProj.AddFileToBuild(targetName, xcodeProj.AddFile("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));
        List<string> frameWorkList = new List<string>();
        frameWorkList.Add("Frameworks/Plugins/Ios/LMGameSDK.framework");
        //frameWorkList.Add("Frameworks/Plugins/Ios/Shoumeng/AdjustSdk.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/Shoumeng/AppsFlyerLib.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/Shoumeng/Bugly.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/Shoumeng/FBSDKCoreKit.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/Shoumeng/FBSDKGamingServicesKit.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/Shoumeng/FBSDKLoginKit.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/Shoumeng/FBSDKShareKit.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/Shoumeng/GoogleSignIn.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/Shoumeng/GoogleSignInDependencies.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/Shoumeng/TapDB_iOS.framework");

        //firebase core
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/FIRAnalyticsConnector.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/FirebaseAnalytics.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/FirebaseCore.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/FirebaseCoreDiagnostics.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/FirebaseInstallations.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/GoogleAppMeasurement.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/GoogleDataTransport.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/GoogleUtilities.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/nanopb.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/PromisesObjC.framework");
        //messageing
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/FirebaseInstanceID.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/FirebaseMessaging.framework");
        frameWorkList.Add("Frameworks/Plugins/Ios/FirebaseMessaging/Protobuf.framework");

        //这部分文件目录不一样，特殊处理一下，
        List<string> specialList = new List<string>();
        specialList.Add("File.swift");
        specialList.Add("Unity-iPhone-Bridging-Header.h");
        specialList.Add("language.json");
        Debug.Log(path);
        foreach(var item in specialList)
        {
            CopyAndReplaceDirectory(Application.dataPath + "/Plugins/Ios/" + item, Path.Combine(path, "Libraries/Plugins/Ios/" + item));
            frameWorkList.Add("Libraries/Plugins/Ios/" + item);
        }
        
        foreach (var item in frameWorkList)
        {
            xcodeProj.AddFileToBuild(targetName, xcodeProj.AddFile(item, item, PBXSourceTree.Source));
        }

        //firebase单独处理一下plist，需要放在根目录
        CopyAndReplaceDirectory(Application.dataPath + "/Plugins/Ios/GoogleService-Info.plist", Path.Combine(path, "GoogleService-Info.plist"));
        xcodeProj.AddFileToBuild(targetName, xcodeProj.AddFile("GoogleService-Info.plist", "GoogleService-Info.plist", PBXSourceTree.Source));
        
        // 设置签名
        xcodeProj.SetBuildProperty(targetName, "PRODUCT_BUNDLE_IDENTIFIER", "com.zgame.qlywz");
        xcodeProj.SetBuildProperty(targetName, "DEVELOPMENT_TEAM", "UJVXUZ95H6");
        xcodeProj.SetBuildProperty(targetName, "PROVISIONING_PROFILE", "qlywz ad-hoc");
        xcodeProj.SetBuildProperty(targetName, "CODE_SIGN_IDENTITY", "iPhone Distribution");

        //设置关闭bitcode
        xcodeProj.SetBuildProperty(targetName, "ENABLE_BITCODE", "NO");
        //设置swift版本，不设置会提示版本太低
        xcodeProj.SetBuildProperty(targetName, "SWIFT_VERSION", "4.0");
        //添加oc
        xcodeProj.AddBuildProperty(targetName, "OTHER_LDFLAGS", "-ObjC");
        //保存工程
        xcodeProj.WriteToFile(projPath);


        //修改plist
        var plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        var rootDict = plist.root;
        //info参数
        rootDict.SetString("FacebookAppID", "569646550402479");
        rootDict.SetString("FacebookDisplayName", "com.riseand.falle");
        rootDict.SetString("GoogleClientID", "127820362669-8vh2f2r04e5mbc97lndjul3qgvcdvfld.apps.googleusercontent.com");

        //URL
        var urlTypeArray = rootDict.CreateArray("CFBundleURLTypes");
        var urlTypeDict = urlTypeArray.AddDict();
        urlTypeDict.SetString("CFBundleTypeRole", "Editor");
        var urlScheme = urlTypeDict.CreateArray("CFBundleURLSchemes");
        urlScheme.AddString("com.riseand.falle");
        urlTypeDict = urlTypeArray.AddDict();
        urlScheme = urlTypeDict.CreateArray("CFBundleURLSchemes");
        urlScheme.AddString("fb569646550402479,com.googleusercontent.apps.127820362669-8vh2f2r04e5mbc97lndjul3qgvcdvfld");

        //保存plist
        plist.WriteToFile(plistPath);
        File.WriteAllText(projPath, xcodeProj.WriteToString());
    }

    static void CopyAndReplaceDirectory(string srcPath, string dstPath)
    {
        File.Copy(srcPath, dstPath, true);
    }
}
#endif
