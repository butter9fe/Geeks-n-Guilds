using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using DG.Tweening.Plugins;
using System.Linq;

public enum CloudScriptType
{
    OnUserRegister
}

public class CloudScriptManager : DesignPatterns.SingletonPersistent<CloudScriptManager>
{
    public delegate void OnSuccess(ExecuteCloudScriptResult r);
    public delegate void OnError(PlayFabError e);
    public delegate void OnGetExpertise(string Expertise);
    public delegate void OnGetBoxId(int boxId);
    public delegate void OnGetFoodCounts(int[] foodCounts);
    public delegate void OnGetHasApprovedApplication(bool approved);

    public void ExecBasicCoudScriptFunction(CloudScriptType type, OnSuccess onSuccess = null, OnError onError = null)
    {
        // Convert type to function name
        string functionName = System.Enum.GetName(typeof(CloudScriptType), type);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = functionName,
            FunctionParameter = new { },
            GeneratePlayStreamEvent = true
        },
            (r) => {
                var succFunc = onSuccess == null ? OnExecSucc : onSuccess;
                succFunc(r);
            },
            (e) => {
                var failFunc = onError == null ? OnExecFail : onError;
                failFunc(e);
            }
        );
    }

    public void ExecGetExpertise(OnGetExpertise onSuccess, OnError onError)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GetExpertises",
            FunctionParameter = new { },
            GeneratePlayStreamEvent = true
        },
        (r) => onSuccess(r.FunctionResult.ToString()),
        (e) => onError(e));
    }

    public void ExecGetHelmetIndex(OnGetBoxId onSuccess, OnError onError)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GetHelmetIndex",
            FunctionParameter = new { },
            GeneratePlayStreamEvent = true
        },
        (r) => onSuccess((r == null || r.FunctionResult == null) ? 0 : System.Convert.ToInt32(r.FunctionResult.ToString())),
        (e) => onError(e));
    }

    public void ExecUpdateHelmet(int helmetIndex, OnSuccess onSuccess, OnError onError)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdateHelmet",
            FunctionParameter = new
            {
                Helmet = helmetIndex
            },
            GeneratePlayStreamEvent = true,
        },
        r => onSuccess(r),
        e => onError(e));
    }

    public void ExecGetFoodCounts(OnGetFoodCounts onSuccess, OnError onError)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GetFoodCounts",
            FunctionParameter = new { },
            GeneratePlayStreamEvent = true
        },
        (r) =>
        {
            if (r.FunctionResult == null)
                return; // No food counts

            int[] foodCounts = r.FunctionResult.ToString().Split(',')
                .Select(x => { x.Trim(); return int.Parse(x); })
                    .ToArray();

            onSuccess(foodCounts);
        },
        (e) => onError(e));
    }

    public void ExecGetHasApprovedApplication(OnGetHasApprovedApplication onSuccess, OnError onError)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GetHasApprovedApplication",
            FunctionParameter = new { },
            GeneratePlayStreamEvent = true
        },
        (r) => onSuccess(r.FunctionResult == null ? false : (bool)r.FunctionResult),
        (e) => onError(e));
    }

    public void ExecSubmitApplication(string hobbies, OnSuccess onSuccess, OnError onError)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "ApproveApplication",
            FunctionParameter = new
            {
                Hobbies = hobbies
            },
            GeneratePlayStreamEvent = true,
        },
        r => onSuccess(r),
        e => onError(e));
    }

    public void ExecSubmitExpertise(string expertises, OnSuccess onSuccess, OnError onError)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "SubmitExpertise",
            FunctionParameter = new
            {
                Expertises = expertises
            },
            GeneratePlayStreamEvent = true,
        },
        r => onSuccess(r),
        e => onError(e));
    }

    void OnExecSucc(ExecuteCloudScriptResult r)
    {
        Debug.Log("Response from server: " + r.FunctionResult.ToString());
    }

    void OnExecFail(PlayFabError e)
    {
        Debug.LogError("Error from cloud script: " + e.GenerateErrorReport());
    }
}
