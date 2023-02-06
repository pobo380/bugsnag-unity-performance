﻿using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;

[Serializable]
public class Command
{
    public string action;
    public string scenarioName;
}

public class Main : MonoBehaviour
{

    private static Main _instance;

#if UNITY_IOS || UNITY_TVOS
    [DllImport("__Internal")]
    private static extern void ClearPersistentData();
#endif

    public TextMeshProUGUI DebugText;

    private const string API_KEY = "a35a2a72bd230ac0aa0f52715bbdc6aa";
    private string _mazeHost;

    public ScenarioRunner ScenarioRunner;

    private void Awake()
    {
        _instance = this;
    }

    public void Start()
    {
        Log("Maze Runner app started");
        _mazeHost = "http://localhost:9339";

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.Android)
        {
            _mazeHost = "http://bs-local.com:9339";
        }
        InvokeRepeating("DoRunNextMazeCommand", 0, 1);
    }

    private void DoRunNextMazeCommand()
    {
        StartCoroutine(RunNextMazeCommand());
    }

    IEnumerator RunNextMazeCommand()
    {
        var url = _mazeHost + "/command";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            var result = request != null && request.result == UnityWebRequest.Result.Success;
#else
            var result = request != null &&
                !request.isHttpError &&
                !request.isNetworkError;
#endif

            if (result)
            {
                var response = request.downloadHandler?.text;
                if (response == null || response == "null" || response == "No commands to provide" || response.Contains("noop"))
                {
                    
                }
                else
                {
                    var command = JsonUtility.FromJson<Command>(response);
                    if (command != null)
                    {
                        Log("Got Action: " + command.action + " and scenario: " + command.scenarioName);
                        if ("clear_cache".Equals(command.action))
                        {
                            ClearUnityCache();
                        }
                        else if ("run_scenario".Equals(command.action))
                        {
                            ScenarioRunner.RunScenario(command.scenarioName, API_KEY, _mazeHost);
                        }
                        else if ("close_application".Equals(command.action))
                        {
                            CloseFixture();
                        }
                    }
                }
            }
        }
    }

    private void CloseFixture()
    {
        Application.Quit();
    }


    private void ClearUnityCache()
    {
        if (Directory.Exists(Application.persistentDataPath + "/Bugsnag"))
        {
            Directory.Delete(Application.persistentDataPath + "/Bugsnag", true);
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ClearIOSData();
        }
        if (Application.platform != RuntimePlatform.Android &&
            Application.platform != RuntimePlatform.IPhonePlayer)
        {
            Invoke("CloseFixture", 0.25f);
        }
    }

    public static void ClearIOSData()
    {
#if UNITY_IOS
        ClearPersistentData();
#endif
    }

    public static void Log(string msg)
    {
        _instance.DebugText.text += Environment.NewLine + msg;
        Console.WriteLine(msg);
        Debug.Log(msg);
    }

}



