using UnityEngine;

public class Enemy : MonoBehaviour {
  EnemyFactory originFactory;

  public EnemyFactory OriginFactory {
    get => originFactory;
    set {
      Debug.Assert(originFactory == null, "Redefined origin factory");
      originFactory = value;
    }
  }

  public void SpawnOn (GameTile tile) {
    transform.localPosition = tile.transform.localPosition;
  }
}