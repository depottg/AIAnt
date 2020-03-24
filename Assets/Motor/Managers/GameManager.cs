﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStatus
{
    ROTATING,
    ANIMATING,
    THINKING,
    ACTING
}

[System.Serializable]
public class TileContent
{
    public Tile tile;
    public Ant ant;
    // List<Pheromone> pheromones
    // Food food

    public TileContent(Tile tile, Ant ant)
    {
        this.tile = tile;
        this.ant = ant;
    }
}

public class Team
{
    public int teamId;
    public AntAI ai;

    public Queen queen;
    public List<Worker> workers;

    public Team(int teamId, Queen queen, AntAI ai)
    {
        this.teamId = teamId;
        this.queen = queen;
        this.workers = new List<Worker>();
        this.ai = ai;
    }
}

public class GameManager : MonoBehaviour
{
    private GameStatus status = GameStatus.THINKING;

    [Header("Prefabs")]
    public Tile groundTilePrefab;
    public Tile waterTilePrefab;

    [Header("Terrain")]
    public int terrainWidth;
    public int terrainHeight;
    private TileContent[][] terrain;

    [Header("Gameplay")]
    public Queen queenPrefab;
    public Worker workerPrefab;
    public List<AntAI> aisToCompete;
    private List<Team> teams;

    [Header("Animations")]
    public float animationTime;
    public float rotationTime;
    private float currentAnimationTime;


    /*
     * HEXAGONAL TERRAIN REPRESENTATION
     * 
     * Hex form:
     *     A   B   C 
     *   D   E   F
     * G   H   I
     * 
     * Hex coordinates:
     *       (0|0) (1|0) (2|0)
     *    (0|1) (1|1) (2|1)
     * (0|2) (1|2) (2|2)
     * 
     * Terrain table form:
     * A D G
     * B E H
     * C F I
     * 
     * Terrain table coordinates :
     * (0|0) (0|1) (0|2)
     * (1|0) (1|1) (1|2)
     * (2|0) (2|1) (2|2)
    */

    // Start is called before the first frame update
    void Start()
    {
        // If the map too small
        if (terrainWidth < 2 || terrainHeight < 2)
            return;

        // Fills the terrain with tiles
        terrain = new TileContent[terrainWidth][];
        for (int i = 0; i < terrainWidth; i++)
        {
            terrain[i] = new TileContent[terrainHeight];
            for (int j = 0; j < terrainHeight; j++)
            {
                if (groundTilePrefab != null)
                {
                    Vector2 currentHexPosition = CoordConverter.HexToPos(new Vector2Int(i, terrainHeight - j - 1));
                    Tile newTile = Instantiate(groundTilePrefab, new Vector3(currentHexPosition.x, groundTilePrefab.transform.position.y, currentHexPosition.y), groundTilePrefab.transform.rotation);
                    terrain[i][j] = new TileContent(newTile, null);
                }
            }
        }

        // Instantiate the teams (number inferior or equal to 4)
        teams = new List<Team>();
        int index = 0;
        foreach (AntAI ai in aisToCompete)
        {
            Vector2Int queenPosition = new Vector2Int();
            switch (index)
            {
                case 0:
                    queenPosition = new Vector2Int(0, 0);
                    break;
                case 1:
                    queenPosition = new Vector2Int(terrainWidth - 1, terrainHeight - 1);
                    break;
                case 2:
                    queenPosition = new Vector2Int(terrainWidth - 1, 0);
                    break;
                case 3:
                    queenPosition = new Vector2Int(0, terrainHeight - 1);
                    break;
                default:
                    queenPosition = new Vector2Int(0, 0);
                    break;
            }

            Vector3 queenWorldPosition = CoordConverter.PlanToWorld(CoordConverter.HexToPos(queenPosition), queenPrefab.transform.position.y);
            Queen newQueen = Instantiate(queenPrefab, queenWorldPosition, queenPrefab.transform.rotation);
            newQueen.Init(queenPosition);

            teams.Add(new Team(index, newQueen, ai));

            index++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        List<Team> winningTeams = null;
        switch (status)
        {
            case GameStatus.ROTATING:

                currentAnimationTime -= Time.deltaTime;
                Rotate();

                if (currentAnimationTime <= 0)
                {
                    currentAnimationTime = animationTime;
                    status = GameStatus.ANIMATING;
                    break;
                }

                break;

            case GameStatus.ANIMATING:

                currentAnimationTime -= Time.deltaTime;
                Animate();

                if (currentAnimationTime <= 0)
                {
                    status = GameStatus.THINKING;
                    break;
                }

                break;

            case GameStatus.THINKING:

                FixAllAnimations();
                winningTeams = CheckForWin();

                if (winningTeams != null && winningTeams.Count > 0)
                    break;

                Think();
                
                status = GameStatus.ACTING;
                break;

            case GameStatus.ACTING:

                Act();

                currentAnimationTime = rotationTime;
                status = GameStatus.ROTATING;
                break;

            default:

                Debug.LogWarning("Illegal game status: " + status.ToString());
                break;
        }

        if (winningTeams != null && winningTeams.Count > 0)
        {
            // Announce victory / tie
        }
    }


    public void SetAIs(List<AntAI> ais)
    {
        aisToCompete = new List<AntAI>();
        foreach (AntAI ai in ais)
        {
            aisToCompete.Add(ai);
        }
    }

    private void FixAllAnimations()
    {
        foreach (Team team in teams)
        {
            team.queen.FixAnimation();
            
            foreach (Worker worker in team.workers)
            {
                worker.FixAnimation();
            }
        }
    }

    private List<Team> CheckForWin()
    {
        return null;
    }

    private void Think()
    {
        foreach (Team team in teams)
        {
            team.queen.decision = team.ai.OnQueenTurn(new TurnInformation(
                terrain[team.queen.gameCoordinates.x][team.queen.gameCoordinates.y].tile.Type,
                team.queen.pastTurn != null ? team.queen.pastTurn.DeepCopy() : null,
                team.queen.mindset,
                null,
                ValueConverter.Convert(team.queen.energy),
                ValueConverter.Convert(team.queen.hp),
                ValueConverter.Convert(team.queen.carriedFood),
                null,
                null,
                team.queen.GetInstanceID()
            ));

            Debug.Log(team.queen.decision.choice.type);

            foreach (Worker worker in team.workers)
            {
                worker.decision = team.ai.OnQueenTurn(new TurnInformation(
                   terrain[worker.gameCoordinates.x][worker.gameCoordinates.y].tile.Type,
                   worker.pastTurn != null ? worker.pastTurn.DeepCopy() : null,
                   worker.mindset,
                   null,
                   ValueConverter.Convert(worker.energy),
                   ValueConverter.Convert(worker.hp),
                   ValueConverter.Convert(worker.carriedFood),
                   null,
                   null,
                   worker.GetInstanceID()
               ));
            }
        }
    }

    private void Act()
    {
        foreach (Team team in teams)
        {
            ResolveDecision(team.queen);

            foreach (Worker worker in team.workers)
            {
                ResolveDecision(worker);
            }
        }
    }

    private void ResolveDecision(Ant ant)
    {
        TurnError error = TreatDecision(ant);
        ant.pastTurn = new PastTurnDigest(ant.decision, error);
    }

    private TurnError TreatDecision(Ant ant)
    {
        Decision decision = ant.decision;

        if (decision.choice == null)
            return TurnError.ILLEGAL;

        switch (decision.choice.type)
        {
            case ActionType.NONE:
                return TurnError.NONE;

            case ActionType.MOVE:
                ActMove(ant, decision.choice.direction);
                return TurnError.ILLEGAL;

            case ActionType.ATTACK:
                Debug.LogWarning("Not implemented yet");
                return TurnError.ILLEGAL;

            case ActionType.EAT:
                Debug.LogWarning("Not implemented yet");
                return TurnError.ILLEGAL;

            case ActionType.STOCK:
                Debug.LogWarning("Not implemented yet");
                return TurnError.ILLEGAL;

            case ActionType.GIVE:
                Debug.LogWarning("Not implemented yet");
                return TurnError.ILLEGAL;

            case ActionType.ANALYSE:
                Debug.LogWarning("Not implemented yet");
                return TurnError.ILLEGAL;

            case ActionType.COMMUNICATE:
                Debug.LogWarning("Not implemented yet");
                return TurnError.ILLEGAL;

            case ActionType.EGG:
                Debug.LogWarning("Not implemented yet");
                return TurnError.ILLEGAL;

            default:
                Debug.LogWarning("Unknwo ActionType: " + decision.choice.type);
                return TurnError.ILLEGAL;

        }
    }

    private void ActMove(Ant ant, HexDirection direction)
    {
        Vector2Int newCoord = CoordConverter.MoveHex(ant.gameCoordinates, direction);

        ant.displayDirection = direction;

        if (!CheckWalkability(newCoord))
            return; ;

        terrain[ant.gameCoordinates.x][ant.gameCoordinates.y].ant = null;
        terrain[newCoord.x][newCoord.y].ant = ant;
        ant.gameCoordinates = newCoord;
    }

    private bool CheckCoordinatesValidity(Vector2Int coord)
    {
        return coord.x >= 0 && coord.y >= 0 && coord.x < terrainWidth && coord.y < terrainHeight;
    }

    // Checks that a tile can be walked in
    private bool CheckWalkability(Vector2Int coord)
    {
        if (!CheckCoordinatesValidity(coord))
            return false;

        TileContent tileContent = terrain[coord.x][coord.y];
        if (tileContent == null)
        {
            Debug.Log("Tile content does not exist at coordinates " + coord.ToString());
            return false;
        }
        if (tileContent.ant != null)
            return false;
        if (tileContent.tile == null || tileContent.tile.Type != TerrainType.GROUND)
            return false;

        return true;
    }

    private void Rotate()
    {
        foreach (Team team in teams)
        {
            team.queen.RotateToTarget(rotationTime - currentAnimationTime, rotationTime);

            foreach (Worker worker in team.workers)
            {
                worker.RotateToTarget(rotationTime - currentAnimationTime, rotationTime);
            }
        }
    }

    private void Animate()
    {
        foreach (Team team in teams)
        {
            team.queen.MoveToTarget(animationTime - currentAnimationTime, animationTime);

            foreach (Worker worker in team.workers)
            {
                worker.MoveToTarget(animationTime - currentAnimationTime, animationTime);
            }
        }
    }
}
