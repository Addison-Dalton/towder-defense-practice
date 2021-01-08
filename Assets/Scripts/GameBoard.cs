using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {
  [SerializeField]
  Transform ground = default;

  [SerializeField]
  GameTile tilePrefab = default;
  GameTile[] tiles;

  Vector2Int size;
  Queue<GameTile> searchFrontier = new Queue<GameTile>();

  public void Initialize(Vector2Int size) {
    this.size = size;
    ground.localScale = new Vector3(size.x, size.y, 1f);

    Vector2 offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);
    tiles = new GameTile[size.x * size.y];
    for (int i = 0, y = 0; y < size.y; y++) {
      for (int x = 0; x < size.x; x++, i++) {
        GameTile tile = tiles[i] = Instantiate(tilePrefab);
        tile.transform.SetParent(transform, false);
        tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);

        // assign East/West and North/South Tile relationships
        if (x > 0) GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
        if (y > 0) GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);

        // assign alternate tile if x is even, but negate this assignment if y is even
        tile.IsAlternative = (x & 1) == 0;
        if ((y & 1) == 0) {
          tile.IsAlternative = !tile.IsAlternative;
        }
      }
    }
    FindPaths();
  }

  private void FindPaths () {
    // clear all paths
    foreach (GameTile tile in tiles) {
      tile.ClearPath();
    }
    // assign destination tile and add it to the pathing frontier
    tiles[tiles.Length / 2].BecomeDestination();
    searchFrontier.Enqueue(tiles[tiles.Length / 2]);

    // assign paths for every tile
    while (searchFrontier.Count > 0) {
      GameTile frontierTile = searchFrontier.Dequeue();
      if (frontierTile != null) {
        if (frontierTile.IsAlternative) {
          searchFrontier.Enqueue(frontierTile.GrowPathNorth());
          searchFrontier.Enqueue(frontierTile.GrowPathSouth());
          searchFrontier.Enqueue(frontierTile.GrowPathEast());
          searchFrontier.Enqueue(frontierTile.GrowPathWest());
        } else {
          searchFrontier.Enqueue(frontierTile.GrowPathWest());
          searchFrontier.Enqueue(frontierTile.GrowPathEast());
          searchFrontier.Enqueue(frontierTile.GrowPathSouth());
          searchFrontier.Enqueue(frontierTile.GrowPathNorth());
        }
      }
    }

    // rotate tile arrows according to thier paths
    foreach (GameTile tile in tiles) {
      tile.ShowPath();
    }
  }

  public GameTile GetTile (Ray ray) {
    if (Physics.Raycast(ray, out RaycastHit hit)) {
      int x = (int) (hit.point.x + size.x * 0.5f);
      int y = (int) (hit.point.z + size.y * 0.5f);
      if (x >= 0 && x < size.x && y >= 0 && y < size.y) {
        return tiles[x + y * size.x];
      }
      return null;
    };
    return null;
  }
}
