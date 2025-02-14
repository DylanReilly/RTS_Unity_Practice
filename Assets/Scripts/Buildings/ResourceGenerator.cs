﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private int resourcesPerInterval = 10;
    [SerializeField] private float interval = 2f;

    private float timer;
    private RTSPlayer player;

    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();

        health.ServerOnDie += ServerHandleDie;
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            player.SetResources(player.getResources() + resourcesPerInterval);

            timer += interval;
        }
    }

    private void ServerHandleDie()
    {
        //gameObject refers to the object this script is attached to
        NetworkServer.Destroy(gameObject);
    }

    private void ServerHandleGameOver()
    {
        //enabled refers to the current script
        enabled = false;
    }
}
