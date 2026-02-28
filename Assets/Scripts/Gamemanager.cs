using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Marcador")]
    public int scoreTeamA = 0;
    public int scoreTeamB = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void TeamAScored()
    {
        scoreTeamA++;
        Debug.Log("GOOL Equipo A! | A:" + scoreTeamA + " - B:" + scoreTeamB);
    }

    public void TeamBScored()
    {
        scoreTeamB++;
        Debug.Log("GOOL Equipo B! | A:" + scoreTeamA + " - B:" + scoreTeamB);
    }
}