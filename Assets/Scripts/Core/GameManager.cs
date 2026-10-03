using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameState Gamestate;
    // Start is called before the first frame update
    void Start()
    {
        Gamestate = GameState.Playing;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
