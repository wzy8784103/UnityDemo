using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using LitJson;

public class NewBehaviourScript : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void UnityToOC(int type, string s);
    // Use this for initialization
    void Start () {
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(100, 100, 100, 100), "登陆"))
        {
            UnityToOC(0, "");
        }
        if (GUI.Button(new Rect(200, 100, 100, 100), "支付"))
        {
            JsonData jsonData = new JsonData();
            jsonData["gameServerId"] = 1;
            jsonData["cpOrderId"] = "sdsdsf";
            jsonData["totalFee"] = 4.99d;
            jsonData["currency"] = 1;
            jsonData["coinName"] = "金币";
            jsonData["userId"] = 123;
            jsonData["channelOrderId"] = "adventure.product.250";
            UnityToOC(1, jsonData.ToJson());
        }
        if (GUI.Button(new Rect(300, 100, 100, 100), "轨迹"))
        {
            UnityToOC(5, "City");
        }
        if (GUI.Button(new Rect(400, 100, 100, 100), "上报"))
        {
            JsonData jsonData = new JsonData();
            jsonData["roleId"] = 1;
            jsonData["roleName"] = "1";
            jsonData["areaId"] = 111;
            jsonData["area"] = "一区";
            jsonData["sociaty"] = "联盟";
            jsonData["vip"] = 16;
            jsonData["level"] = 60;
            jsonData["extInfo"] = "";
            UnityToOC(2, jsonData.ToJson());
        }
    }

    private void LoginResult(string result)
    {
        Debug.Log("LoginResult===" + result);
    }

    private void PayResult(string result)
    {
        Debug.Log("PayResult===" + result);
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void Call(string s)
    {
        Debug.Log(s);
    }
}
