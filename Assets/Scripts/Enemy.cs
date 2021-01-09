using UnityEngine;

public class Enemy : MonoBehaviour {
  EnemyFactory originFactory;
  GameTile tileFrom, tileTo;
  Vector3 positionFrom, positionTo;
  float progress;

  Direction direction;
  DirectionChange directionChange;
  float directionAngleFrom, directionAngleTo;

  public EnemyFactory OriginFactory {
    get => originFactory;
    set {
      Debug.Assert(originFactory == null, "Redefined origin factory");
      originFactory = value;
    }
  }

  public void SpawnOn (GameTile tile) {
    Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go", this);
    tileFrom = tile;
    tileTo = tile.NextTileOnPath;
    progress = 0f;
    PrepareIntro();
  }
  void PrepareIntro () {
    positionFrom = tileFrom.transform.localPosition;
    positionTo = tileFrom.ExitPoint;
    direction = tileFrom.PathDirection;
    directionChange = DirectionChange.None;
    directionAngleFrom = directionAngleTo = direction.GetAngle();
    transform.localRotation = direction.GetRotation();
  }

  void PrepareNextState () {
    positionFrom = positionTo;
    positionTo = tileFrom.ExitPoint;
    directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
    direction = tileFrom.PathDirection;
    directionAngleFrom = directionAngleTo;

    switch (directionChange) {
      case DirectionChange.None: PrepareForward(); break;
      case DirectionChange.TurnRight: PrepareRight(); break;
      case DirectionChange.TurnLeft: PrepareLeft(); break;
      default: PrepareTurnAround(); break;
    }
  }

  public bool GameUpdate () {
    progress += Time.deltaTime;
    while (progress >= 1f) {
      tileFrom = tileTo;
      tileTo = tileTo.NextTileOnPath;
      
      // destination reached
      if (tileTo == null) {
        OriginFactory.Reclaim(this);
        return false;
      }
      progress -= 1f;
      PrepareNextState();
    }
    transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
    if (directionChange != DirectionChange.None) {
      float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
      transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }
    return true;
  }

  // movement and rotation
  void PrepareForward () {
    transform.localRotation = direction.GetRotation();
    directionAngleTo = direction.GetAngle();
  }
  void PrepareRight () {
    directionAngleTo = directionAngleFrom + 90f;
  }
  void PrepareLeft () {
    directionAngleTo = directionAngleFrom - 90f;
  }
  void PrepareTurnAround () {
    directionAngleTo = directionAngleFrom + 180f;
  }
}