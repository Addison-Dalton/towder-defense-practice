using UnityEngine;

public class Enemy : MonoBehaviour {
  EnemyFactory originFactory;
  GameTile tileFrom, tileTo;
  Vector3 positionFrom, positionTo;
  [SerializeField]
  Transform model = default;
  float progress, progressFactor;
  float pathOffset, speed;

  Direction direction;
  DirectionChange directionChange;
  float directionAngleFrom, directionAngleTo;
  [SerializeField] bool doesHop = true;
  [SerializeField] AnimationCurve hopHeightCurve;
  [SerializeField] AnimationCurve hopMovementCurve;
  [SerializeField] float hopHeightFloorPercent = .136f; // this is only way I could get the curve to use negative values
  [SerializeField, FloatRangeSlider(0f, 3f)] FloatRange hopDelayRange = new FloatRange(0f, 3f); 
  float hopDelay = 0f;
  [SerializeField, FloatRangeSlider(0f, 3f)] FloatRange hopHeightRange = new FloatRange(.25f, .5f); 
  float hopHeight = 1f;
  // random offset from next tile "to" position. does weird thing with turns
  [SerializeField, FloatRangeSlider(-1f, 1f)] FloatRange landingAccuracy = new FloatRange(-0.5f, 0.5f); 
  bool isHopping = true;
  float heightCurveOffset = 0; // from hopHeightFloorPercent, only needs to be calculated once in Initialize

  float hopRestProgress = 0f;
  float curY = 0f;

  public EnemyFactory OriginFactory {
    get => originFactory;
    set {
      Debug.Assert(originFactory == null, "Redefined origin factory");
      originFactory = value;
    }
  }

  public void Initialize (float scale, float speed, float pathOffset) {
    model.localScale = new Vector3(scale, scale, scale);
    this.speed = speed;
    this.pathOffset = pathOffset;
    heightCurveOffset = hopHeightFloorPercent * hopHeight;
    hopDelay = hopDelayRange.RandomValueInRange;
    hopHeight = hopHeightRange.RandomValueInRange;
  }

  public void SpawnOn (GameTile tile) {
    Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go", this);
    tileFrom = tile;
    tileTo = tile.NextTileOnPath;
    progress = 0f;
    PrepareIntro();
  }

  void PrepareNextState () {
    tileFrom = tileTo;
    tileTo = tileTo.NextTileOnPath;
    positionFrom = positionTo;

    if (tileTo == null) {
      PrepareOutro();
      return;
    }

    positionTo = getLandingZone(tileFrom.ExitPoint);
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
    if(doesHop && !isHopping){
      restingFromHop();
      return true;
    }
    progress += Time.deltaTime * progressFactor;
    curY = getCurY();
    while (progress >= 1f) {
      isHopping = false;
      // destination reached
      if (tileTo == null) {
        OriginFactory.Reclaim(this);
        return false;
      }
      progress = (progress - 1f) / progressFactor;
      PrepareNextState();
      progress *= progressFactor;
    }
    if (directionChange == DirectionChange.None) {
      if(doesHop){
        transform.localPosition = getHopPosition(positionFrom, positionTo, progress);
      }else{
        transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
      }
    } else {
      float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
      transform.localRotation = Quaternion.Euler(0f, angle, 0f);
      if(doesHop){
        transform.localPosition = new Vector3(transform.localPosition.x, curY, transform.localPosition.z);
      }
    }
    return true;
  }
  // movement and rotation
  void PrepareForward () {
    transform.localRotation = direction.GetRotation();
    directionAngleTo = direction.GetAngle();
    model.localPosition = new Vector3(pathOffset, 0f);
    progressFactor = speed;
  }
  void PrepareRight () {
    directionAngleTo = directionAngleFrom + 90f;
    model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
    transform.localPosition = positionFrom + direction.GetHalfVector();
    progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
  }
  void PrepareLeft () {
    directionAngleTo = directionAngleFrom - 90f;
    model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
    transform.localPosition = positionFrom + direction.GetHalfVector();
    progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
  }
  void PrepareTurnAround () {
    directionAngleTo = directionAngleFrom + (pathOffset < 0f ? 180f : -180f);
    model.localPosition = new Vector3(pathOffset, 0f);
    transform.localPosition = positionFrom;
    progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
  }

  void PrepareIntro () {
    positionFrom = tileFrom.transform.localPosition;
    positionTo = tileFrom.ExitPoint;
    direction = tileFrom.PathDirection;
    directionChange = DirectionChange.None;
    directionAngleFrom = directionAngleTo = direction.GetAngle();
    model.localPosition = new Vector3(pathOffset, 0f);
    transform.localRotation = direction.GetRotation();
    progressFactor = 2f * speed;
  }

  void PrepareOutro () {
    positionTo = tileFrom.transform.localPosition;
    directionChange = DirectionChange.None;
    directionAngleTo = direction.GetAngle();
    model.localPosition = new Vector3(pathOffset, 0f);
    transform.localRotation =  direction.GetRotation();
    progressFactor = 2f * speed;
  }

  // hop modifiers
  void restingFromHop(){
    if(hopRestProgress >= hopDelay){
      isHopping = true;
      hopRestProgress = 0;
    }else{
      hopRestProgress += Time.deltaTime * progressFactor;
    }
  }
  float getCurY() {
    return Mathf.Lerp (0f, hopHeight, hopHeightCurve.Evaluate (progress)) - heightCurveOffset;
  }
  Vector3 getHopPosition(Vector3 fromPos, Vector3 toPos, float progressVal) {
    float curveVal = hopMovementCurve.Evaluate (progressVal);
    float curveX = Mathf.Lerp (fromPos.x, toPos.x, curveVal);
    float curveZ = Mathf.Lerp (fromPos.z, toPos.z, curveVal);
    return new Vector3(curveX, getCurY(), curveZ);
  }

  Vector3 getLandingZone(Vector3 orig){
    if(!doesHop){
      return orig;
    }
    return new Vector3(orig.x + landingAccuracy.RandomValueInRange, orig.y, orig.z + landingAccuracy.RandomValueInRange);
  }
}