#import "UnityAppController.h"
#import <LMGameSDK/cls_com_GameSDK.h>

@interface CustomAppController : UnityAppController

@end
extern "C"{
    void UnityToOC(int type, char* c);
}
IMPL_APP_CONTROLLER_SUBCLASS (CustomAppController)

@implementation CustomAppController
- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    [super application: application didFinishLaunchingWithOptions : launchOptions];
    
   //sdk初始化
    [[cls_com_GameSDK sharedInstance] initSDKWithApplication:application didFinishLaunchingWithOptions:launchOptions gameId:@"289" pakcageId:@"2880099" ADJustAppToken:@"j1syzpzzfk00"];
    [[cls_com_GameSDK sharedInstance] setIsLandscape:YES];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(sdkLoginStatus:) name:cls_com_kGameLoginStatusNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(IAPChangeStatus:) name:cls_com_kGameIAPChangedNotification object:nil];
    NSLog(@"demo application applicationWillEnterForeground！");
    return YES;
}

-(void)sdkLoginStatus:(NSNotification *)sender{
    NSNumber* result = @1;
    NSString* userinfo = @"";
    if (sender&&sender.userInfo) {
        NSDictionary *userInfo=sender.userInfo;
        // 登录状态
        BOOL loginStatus=[[userInfo objectForKey:@"loginStatus"] boolValue];
        if (loginStatus) {
            // 登录成功
            NSString *account=[userInfo objectForKey:@"account"];
            NSString *session=[userInfo objectForKey:@"session"];
            NSLog(@"登录成功\n账号是：%@\nsession是：%@",account,session);
            
            NSDictionary* tempDict = @{@"loginAccount":account, @"channelLabel":@"Apple", @"sessionId":session};
            NSData *tempData = [NSJSONSerialization dataWithJSONObject:tempDict options:kNilOptions error:nil];
            result = @0;
            userinfo = [[NSString alloc] initWithData:tempData encoding:NSUTF8StringEncoding];
        }else{
            // 登录失败
            NSLog(@"登录失败");
            result = @1;
        }
    }else{
        NSLog(@"登录失败");
        result = @1;
    }
    //最后拼的串
    NSDictionary* dict = @{@"result":result, @"userInfo":userinfo};
    NSData *data = [NSJSONSerialization dataWithJSONObject:dict options:kNilOptions error:nil];
    NSString* str = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    UnitySendMessage("ShoumengObject", "LoginResult", [str UTF8String]);
}

-(void)IAPChangeStatus:(NSNotification *)sender{
    NSString* str = @"";
    if (sender&&sender.userInfo) {
        NSDictionary *userInfo=sender.userInfo;
        BOOL loginStatus=[[userInfo objectForKey:@"result"] boolValue];
        if (loginStatus) {
            // 充值成功
            NSString *message=[userInfo objectForKey:@"message"];
            NSLog(@"充值成功\n返回消息是：%@\n",message);
            str = @"Success";
        }else{
            // 充值失败
            NSString *message=[userInfo objectForKey:@"message"];
            NSLog(@"充值失败\n返回消息是：%@\n",message);
            str = @"Fail";
        }
    }else{
        NSLog(@"充值失败");
        str = @"Fail";
    }
    UnitySendMessage("ShoumengObject", "PayResult", [str UTF8String]);
}

- (void)applicationWillTerminate:(UIApplication *)application {
    [[cls_com_GameSDK sharedInstance] removeTransactionObserver];
}

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation {
    NSLog(@"openURL sourceApplication"); // ios 8 调用这里
    
    return [[cls_com_GameSDK sharedInstance] openAppSchemeApplication:application openURL:url options:nil];
}
- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<NSString*, id> *)options{
    NSLog(@"openURL options"); // ios 9 调用这里

    return [[cls_com_GameSDK sharedInstance] openAppSchemeApplication:app openURL:url options:options];
}
@end

void UnityToOC(int type, char* c){
    switch(type){
            //登陆
        case 0:{
            [[cls_com_GameSDK sharedInstance] showMemberCenter];
            break;
        }
            //充值
        case 1:{
            NSString *str = [[NSString alloc] initWithUTF8String:c];
            NSLog(@"调用充值\nstr是：%@\n",str);
            NSData *data =[str dataUsingEncoding:NSUTF8StringEncoding];
            NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
//
//            int price=[[dict objectForKey:@"totalFee"] intValue];
//            NSLog(@"调用充值\nprice是：%@\n",[[NSNumber numberWithInt:price] stringValue]);
//            NSNumber *nub= [NSNumber numberWithFloat:(float)price/100];
//            NSLog(@"调用充值\nnub是：%@\n",[nub stringValue]);
            [[cls_com_GameSDK sharedInstance]
             startPIAPWithServerId:[dict objectForKey:@"gameServerId"]
             OrderId:[dict objectForKey:@"cpOrderId"]
             Amount:[dict objectForKey:@"totalFee"]
             ProductId:[dict objectForKey:@"channelOrderId"]];
            break;
        }
            //角色信息
        case 2:{
            NSString *str = [[NSString alloc] initWithUTF8String:c];
            NSLog(@"调用角色信息\nstr是：%@\n",str);
            NSData *data =[str dataUsingEncoding:NSUTF8StringEncoding];
            NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
            [[cls_com_GameSDK sharedInstance]
             recordLogSetRoleInfoWithRoleId:[dict objectForKey:@"roleId"]
             andRoleName:[dict objectForKey:@"roleName"]
             andServerId:[dict objectForKey:@"areaId"]
             andRoleLevel:[dict objectForKey:@"level"]
             andRoleVipLevel:[dict objectForKey:@"vip"]];
            break;
        }
            //轨迹
        case 5:{
            NSLog(@"调用轨迹\nstr是：%@\n",[[NSString alloc] initWithUTF8String:c]);
            [[cls_com_GameSDK sharedInstance] trackEventWithKey:[[NSString alloc] initWithUTF8String:c]];
            break;
        }
    }
//    NSLog([NSString stringWithUTF8String:c]);
//    UnitySendMessage("Main Camera", "Call", "[Return Unity Successful]");
}
