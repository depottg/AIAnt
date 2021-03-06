﻿
public enum Value
{
    NONE,
    LOW,
    MEDIUM,
    HIGH
}

public enum HexDirection
{
    CENTER,
    UPLEFT,
    UPRIGHT,
    LEFT,
    RIGHT,
    DOWNLEFT,
    DOWNRIGHT
}

public enum AntType
{
    NONE,
    WORKER,
    QUEEN
}

public enum ActionType
{
    NONE,
    MOVE,
    ATTACK,
    EAT,
    STOCK,
    GIVE,
    ANALYSE,
    COMMUNICATE,
    EGG
}

public enum TurnError
{
    NONE,
    NO_ENERGY, // If the ant does not have enough energy to perform the action
    NO_FOOD, // If the ant does not have food to give or enough carried food to eat
    COLLISION_ANT, // If the action could not be done because the target contained an ant
    COLLISION_FOOD, // If the action could not be done because the target contained food
    COLLISION_EGG, // If the action could not be done because the target contained an egg
    COLLISION_WATER, // If the action could not be done because the target was a tile of water
    COLLISION_VOID, // If the action could not be done because the target did not contain a tile, or was out of bounds
    NOT_ENEMY, // If an ant tries to attack its ally
    NOT_ALLY, // If an ant tries to give to an enemy
    NO_TARGET, // If the target is missing (no ant when attack, no food when eat, etc.)
    NOT_QUEEN, // If a non-queen ant tries to EGG
    ILLEGAL
}

public enum PheromoneType
{
    PHER0,
    PHER1,
    PHER2,
    PHER3
}

public enum AntMindset
{
    AMS0,
    AMS1,
    AMS2,
    AMS3,
    AMS4,
    AMS5,
    AMS6,
    AMS7
}

public enum AntWord
{
    NONE,
    AW0,
    AW1,
    AW2,
    AW3,
    AW4,
    AW5,
    AW6,
    AW7,
}

public enum TerrainType
{
    NONE,
    GROUND,
    WATER
}