using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
  [SerializeField]
  Transform arrow = default;
  GameTile north, east, south, west, nextOnPath;
  int distance;
  public bool HasPath => distance != int.MaxValue;

  public static void MakeEastWestNeighbors (GameTile east, GameTile west) {
    Debug.Assert(west.east == null && east.west == null, "Redefined EastWest neighbors");
    west.east = east;
    east.west = west;
  }
  public static void MakeNorthSouthNeights (GameTile north, GameTile south) {
    Debug.Assert(north.south == null && south.north == null, "Redefined NorthSouth neighbors");
    north.south = south;
    south.north = north;
  }

  public void ClearPath () {
    distance = int.MaxValue;
    nextOnPath = null;
  }

  public void BecomeDestination () {
    distance = 0;
    nextOnPath = null;
  }
}
