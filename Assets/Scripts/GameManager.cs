using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Action<Tile> mouseDown;

    [SerializeField] private int matchLength = 3;
    [SerializeField] private Vector2Int gameboardLength = new Vector2Int(10, 10);
    [SerializeField] private Tile[] tilePrefabs;

    public static readonly float durationAnimation = .5f;

    private static readonly Vector2 gameboardSize = new Vector2(10f, 10f);
    private static readonly float tileDimensionHalf = .5f;
    private static readonly float distanceGameboard = .5f;

    private Vector2 originPosition;
    private Vector2 stepSize;
    private float scale;
    private List<Tile>[] gameboard;
    private DateTime animationStart = DateTime.MinValue;

    private void OnEnable()
    {
        mouseDown += OnMouseDown;
    }

    private void OnDisable()
    {
        mouseDown -= OnMouseDown;
    }

    private void Awake()
    {
        originPosition = -gameboardSize * .5f;
        stepSize = gameboardSize / gameboardLength;
        scale = Mathf.Min(stepSize.x, stepSize.y);
    }

    void Start()
    {
        gameboard = new List<Tile>[gameboardLength.x];

        for (var column = 0; column < gameboardLength.x; column++)
        {
            gameboard[column] = new List<Tile>();
            for (var row = 0; row < gameboardLength.y; row++)
            {
                InstantiateTile(column, row);
            }
        }
    }

    private void InstantiateTile(int column, int row)
    {
        Tile instance;
        if (column < matchLength - 1)
        {
            instance = Instantiate(GetTilePrefabRandom(), CalculatePosition(column, row), Quaternion.identity);
        }
        else
        {
            Tile tilePrefab;
            bool condition;
            do
            {
                tilePrefab = GetTilePrefabRandom();
                condition = false;
                for (var i = 1; i < matchLength; i++)
                {
                    if (!gameboard[column - i][row].name.Contains(tilePrefab.name))
                    {
                        condition = true;
                        break;
                    }
                }
            } while(!condition);

            instance = Instantiate(tilePrefab, CalculatePosition(column, row), Quaternion.identity);
        }

        instance.transform.localScale = new Vector3(scale, scale, scale);
        gameboard[column].Add(instance);
    }

    private Tile GetTilePrefabRandom()
    {
        return tilePrefabs[UnityEngine.Random.Range(0, tilePrefabs.Length)];
    }

    private Vector3 CalculatePosition(int column, int row)
    {
        return new Vector3(originPosition.x + stepSize.x * column + stepSize.x * tileDimensionHalf,
            originPosition.y + stepSize.y * row + stepSize.y * tileDimensionHalf,
            -scale * tileDimensionHalf + distanceGameboard);
    }

    private void OnMouseDown(Tile tile)
    {
        if (animationStart.AddSeconds(durationAnimation) > DateTime.Now)
        {
            return;
        }

        for (var column = 0; column < gameboardLength.x; column++)
        {
            if (gameboard[column].Remove(tile))
            {
                Destroy(tile.gameObject);

                RepositionAll();
            }
        }
    }

    private void FindMatch()
    {
        for (var row = 0; row < gameboardLength.y; row++)
        {
            string nameLast = string.Empty;
            int columnFirst = 0;
            var count = 1;

            for (var column = 0; column < gameboardLength.x; column++)
            {
                if (gameboard[column].Count <= row)
                {
                    nameLast = string.Empty;
                }
                else
                {
                    var name = gameboard[column][row].name;
                    if (name == nameLast)
                    {
                        count++;
                    }
                    else
                    {
                        nameLast = name;
                        columnFirst = column;
                        count = 1;
                    }
                }

                if (count == matchLength)
                {
                    ClearMatch(columnFirst, row);
                }
            }
        }
    }

    private void ClearMatch(int column, int row)
    {
        string name = gameboard[column][row].name;
        for (; column < gameboardLength.x; column++)
        {
            if (gameboard[column].Count <= row)
            {
                break;
            }

            var tile = gameboard[column][row];

            if (tile.name == name)
            {
                gameboard[column].Remove(tile);
                Destroy(tile.gameObject);
            }
            else
            {
                break;
            }
        }

        RepositionAll();
    }

    private void RepositionAll()
    {
        for (var column = 0; column < gameboardLength.x; column++)
        {
            for (var row = 0; row < gameboard[column].Count; row++)
            {
                gameboard[column][row].SetPosition(CalculatePosition(column, row));
            }
        }

        StartCoroutine(LockInput());
    }

    private IEnumerator LockInput()
    {
        animationStart = DateTime.Now;
        yield return new WaitForSeconds(durationAnimation);
        FindMatch();
    }

    private void OnValidate()
    {
        Debug.Assert(matchLength > 1);
        Debug.Assert(gameboardLength.x > matchLength);
        Debug.Assert(gameboardLength.y > 0);
        Debug.Assert(tilePrefabs.Length > 1);
    }
}
