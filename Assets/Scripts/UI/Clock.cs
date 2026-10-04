using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Clock : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI clockText;
    private int time;
    private float accumulator = 0f;
    void Start()
    {
        time = 0;
    }
    void Update()
    {
        if (GameManager.Instance.Gamestate == GameState.Playing)
        {
            accumulator += Time.deltaTime;
            if (accumulator >= 1f)
            {
                time += 1;
                accumulator = 0;
            }
        }
        getClock();
    }
    private void getClock()
    {
        int min = time / 60;
        int second = time % 60;
        clockText.text = $"{min:00}:{second:00}";
    }
}
