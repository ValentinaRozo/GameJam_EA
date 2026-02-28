using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI ScoreTeamAText;
    public TextMeshProUGUI ScoreTeamBText;

    private int ScoreA = 0;
    private int ScoreB = 0;

    void Start()
    {
        UpdateUI();
    }

    public void AddPointToTeamA()
    {
        ScoreA++;
        UpdateUI();
    }

    public void AddPointToTeamB()
    {
        ScoreB++;
        UpdateUI();
    }

    void UpdateUI()
    {
        ScoreTeamAText.text = "Team A: " + ScoreA;
        ScoreTeamBText.text = "Team B: " + ScoreB;
    }
}
