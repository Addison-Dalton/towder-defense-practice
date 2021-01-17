using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class GameTileContent : MonoBehaviour
{
  [SerializeField]
  GameTileContentType type = default;
  GameTileContentFactory originFactory;

  public GameTileContentType Type => type;
  public bool BlockPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

  public GameTileContentFactory OriginFactory {
    get => originFactory;
    set {
      Debug.Assert(originFactory == null, "Redefined origin factory");
      originFactory = value;
    }
  }

  public virtual void GameUpdate() {}
  public void Recycle () {
    originFactory.Reclaim(this);
  }
}
