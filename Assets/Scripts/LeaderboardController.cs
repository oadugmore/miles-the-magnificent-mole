using UnityEngine;
using System.Collections;
using GameSparks.Core;
using UnityEngine.UI;
using GameSparks.Api.Requests;
using System.Collections.Generic;
using System;

public class LeaderboardController : MonoBehaviour
{
    private List<Text> nameTextList;
    private List<Text> rankTextList;
    private List<Text> scoreTextList;
    //public Text leaderboardNameText;
    public GameObject namesHost;
    public GameObject ranksHost;
    public GameObject scoresHost;
    //public Text leaderboardRankText;
    //public Text leaderboardScoreText;
    public Text myScoreText;

    private void ClearLeaderboard()
    {
        foreach (Text t in nameTextList)
        {
            t.text = string.Empty;
        }
        foreach (Text t in rankTextList)
        {
            t.text = string.Empty;
        }
        foreach (Text t in scoreTextList)
        {
            t.text = string.Empty;
        }
    }

    //int retrieveAttempts = 0;
    private void RetrieveLeaderboardResults()
    {
        //if (GS.Authenticated)
        //{
            //leaderboardNameText.text = string.Empty;
            ShowLoading();
            //myScoreText.text = "My score: ";
            int entryCount = GameController.current.GetLeaderboardDisplayCount();

            new LeaderboardDataRequest_SCORE_LEADERBOARD().SetEntryCount(entryCount).SetDurable(true).Send(response =>
            {
                if (this == null) return;
                if (!response.HasErrors)
                {
                    Debug.Log("Found Leaderboard Data...");
                    ClearLeaderboard();
                    int count = 0;
                    foreach (var entry in response.Data_SCORE_LEADERBOARD)
                    {
                        string playerRank = ((int)entry.Rank).ToString(); // we can get the rank directly
                        string playerName = entry.UserName;
                        string playerScore = entry.SCORE.ToString();
                        //string score = entry.JSONData["SCORE"].ToString(); // we need to get the key, in order to get the score
                        //if (count < GameController.current.leaderboardDisplayCount)
                        //{
                        rankTextList[count].text = playerRank;
                        nameTextList[count].text = playerName;
                        scoreTextList[count].text = playerScore;
                        count++;
                        //}
                        //leaderboardDataText.text += rank + "   Name: " + playerName + "        Score:" + score + "\n"; // add the score to the output text
                    }
                    //myScoreText.text = "My score: " + myScore; // score.ToString();
                }
                else //has errors
                {
                    ShowError();
                }
            });
        //} //end if GS.Authenticated
        //else if (retrieveAttempts < 3) //and GS not authenticated
        //{
        //    GameSparksManager.instance.FixGSConnection(RetrieveLeaderboardResults);
        //}
        //retrieveAttempts++;
    }

    public void ShowError()
    {
        Debug.Log("Error Retrieving Leaderboard Data...");
        ClearLeaderboard();
        nameTextList[2].text = "Error loading leaderboard!";
    }

    public void ShowLoading()
    {
        ClearLeaderboard();
        nameTextList[2].text = "Loading...";
    }

    public void Refresh()
    {
        if (this == null)
            return;
        //retrieveAttempts = 0;
        RetrieveLeaderboardResults();
    }

    // Use this for initialization
    void Start()
    {
        nameTextList = new List<Text>(namesHost.GetComponentsInChildren<Text>());
        rankTextList = new List<Text>(ranksHost.GetComponentsInChildren<Text>());
        scoreTextList = new List<Text>(scoresHost.GetComponentsInChildren<Text>());
        ShowLoading();
    }

}
