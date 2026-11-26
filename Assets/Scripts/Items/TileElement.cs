using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lofelt.NiceVibrations;

public class TileElement : MonoBehaviour
{
    public SpriteRenderer tileSprite;

    public int currentDirection;

    public TileHolder tileHolder;

    private int targetDistance;

    private int awayDistance;

    private BoxCollider2D collider;

    public TrailRenderer trailRenderer;

    public Color[] trailColorArr;

    public ParticleSystem dustVfx;

    public GameObject explosionVfx;

    public SawElement nextSaw;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Bounce()
    {
        if (currentDirection == 0 || currentDirection == 2)
            tileSprite.transform.transform.DOShakePosition(0.25f, strength: new Vector3(0, 0.1f, 0), vibrato: 20, randomness: 1, snapping: false, fadeOut: true);
        else
            tileSprite.transform.transform.DOShakePosition(0.25f, strength: new Vector3(0.1f, 0, 0), vibrato: 20, randomness: 1, snapping: false, fadeOut: true);
    }

    IEnumerator PlayDustVfx()
    {
        dustVfx.transform.SetParent(null);
        dustVfx.Play();
        yield return new WaitForSeconds(2.0f);
        dustVfx.transform.SetParent(transform);
        dustVfx.transform.localPosition = Vector3.zero;
    }

    private void DamageToNeighborCell()
    {
        TileElement neighborCell = GetNeighborCell().tile;

        if (neighborCell == null)
            return;

        neighborCell.tileSprite.DOColor(Color.red, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
        {

            neighborCell.tileSprite.DOColor(Color.white, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {


            });

        });
    }

    private TileHolder GetNeighborCell()
    {
        TileHolder neighborCell = null;

        if (currentDirection == 0)
            neighborCell = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth];
        else if (currentDirection == 2)
            neighborCell = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth];
        else if (currentDirection == 3)
            neighborCell = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex + 1];
        else if (currentDirection == 1)
            neighborCell = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex - 1];

        return neighborCell;
    }

    public void InitTile(int direction)
    {
        collider = GetComponent<BoxCollider2D>();
        currentDirection = direction;
        tileSprite.sprite = GameManager.instance.dataManager.blockConfigList[GameManager.instance.blockSkinIndex].blockSetList[direction].directionSprite[direction];

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(trailColorArr[direction], 0.0f), new GradientColorKey(trailColorArr[direction], 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.5f, 1.0f) }
        );
        trailRenderer.colorGradient = gradient;
    }

    public void MoveTile()
    {
        collider.enabled = false;
        FindNearestCell();
        // Debug.Log("Move Tile " + currentDirection + " Distance " + targetDistance);

        //if (nextSaw != null)
        //    return;

        if (targetDistance > 0)
        {
            if (rotatorParent != null)
            {
                //Debug.Log("RELEASE TILE IN ROTATOR");
                rotatorParent.rootHandList[handIndexInRotator].ReleaseTileNode(nodeIndexInRotator);
                rotatorParent = null;
            }
        }
        if (tileHolder != null)
        {
            StartCoroutine(PlayDustVfx());
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * targetDistance).SetEase(Ease.Linear).OnComplete(() =>
            {
                collider.enabled = true;
                Bounce();
                DamageToNeighborCell();
                AudioManager.instance.bounceSound.Play();
            });
        }

    }

    public void FindNearestCell()
    {
        switch (currentDirection)
        {
            case 0:
                DetectCellUp();
                break;
            case 2:
                DetectCellDown();
                break;
            case 1:
                DetectCellLeft();
                break;
            case 3:
                DetectCellRight();
                break;

        }
    }

    private void DetectCellUp()
    {
        if (tileHolder.posY == 0)
        {
            TargetAway();
            return;
        }

        int distanceCell = 0;

        for (int i = 1; i <= tileHolder.posY; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex
                - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile == null)
                distanceCell++;
            else
                break;
        }

        int distanceToSaw = 0;

        for (int i = 1; i <= tileHolder.posY; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex
                - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw != null)
            {
                distanceToSaw = i;
                nextSaw = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex
                - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw;
                break;
            }

            else
                distanceToSaw = 0;
        }

        int distanceToRot = 0;

        for (int i = 1; i <= tileHolder.posY; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex
                - i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].rotator != null)
            {
                distanceToRot = i;
                break;
            }

            else
                distanceToRot = 0;
        }

        if (distanceToRot > 0 && distanceToRot <= distanceCell)
        {
            distanceCell = distanceToRot - 1;
            AudioManager.instance.bounceSound.Play();
        }


        if (distanceToSaw > 0 && distanceToSaw <= distanceCell)
        {
            tileHolder.tile = null;
            awayDistance = distanceToSaw;
            KillTile();
            return;
        }


        if (distanceCell == tileHolder.posY)
        {
            TargetAway();
            return;
        }

        else if (distanceCell == 0)
        {
            Debug.Log("NO MOVE");
        }

        else
        {
            int lastHolderIndex = tileHolder.cellIndex;
            tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[lastHolderIndex
                - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * distanceCell];
            tileHolder.tile = this;
            Debug.Log("Find Cell Up Distance : " + distanceCell);
            targetDistance = distanceCell;
        }

    }

    private void DetectCellDown()
    {
        //Debug.Log("Find Cell Down");

        if (tileHolder.posY == (GameManager.instance.boardGenerator.currentLevelData.sizeHeight - 1))
        {
            TargetAway();
            return;
        }

        int distanceCell = 0;

        for (int i = 1; i <= GameManager.instance.boardGenerator.currentLevelData.sizeHeight - tileHolder.posY - 1; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex +
                i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile == null)
                distanceCell++;
            else
                break;
        }

        int distanceToSaw = 0;

        for (int i = 1; i <= GameManager.instance.boardGenerator.currentLevelData.sizeHeight - tileHolder.posY - 1; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex +
               i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw != null)
            {
                distanceToSaw = i;
                nextSaw = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex +
                i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].saw;
                break;
            }

            else
                distanceToSaw = 0;
        }

        int distanceToRot = 0;

        for (int i = 1; i <= GameManager.instance.boardGenerator.currentLevelData.sizeHeight - tileHolder.posY - 1; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex +
                i * GameManager.instance.boardGenerator.currentLevelData.sizeWidth].rotator != null)
            {
                distanceToRot = i;
                break;
            }

            else
                distanceToRot = 0;
        }

        if (distanceToRot > 0 && distanceToRot <= distanceCell)
        {
            distanceCell = distanceToRot - 1;
            AudioManager.instance.bounceSound.Play();
        }

        if (distanceToSaw > 0 && distanceToSaw <= distanceCell)
        {
            tileHolder.tile = null;
            awayDistance = distanceToSaw;
            KillTile();
            return;
        }

        if (distanceCell == (GameManager.instance.boardGenerator.currentLevelData.sizeHeight - 1 - tileHolder.posY))
        {
            TargetAway();
            return;
        }
        else if (distanceCell == 0)
        {

        }

        else
        {
            int lastHolderIndex = tileHolder.cellIndex;
            tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[lastHolderIndex
                + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * distanceCell];
            tileHolder.tile = this;
            Debug.Log("Find Cell Down Distance : " + distanceCell);
            targetDistance = distanceCell;
        }
    }

    private void DetectCellLeft()
    {
        //Debug.Log("Find Cell Left");

        if (tileHolder.posX == 0)
        {
            TargetAway();
            return;
        }

        int distanceCell = 0;

        for (int i = 1; i <= tileHolder.posX; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex - i].tile == null)
                distanceCell++;
            else
                break;
        }

        int distanceToSaw = 0;

        for (int i = 1; i <= tileHolder.posX; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex - i].saw != null)
            {
                distanceToSaw = i;
                nextSaw = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex - i].saw;
                break;
            }

            else
                distanceToSaw = 0;
        }

        int distanceToRot = 0;

        for (int i = 1; i <= tileHolder.posX; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex - i].rotator != null)
            {
                distanceToRot = i;
                break;
            }

            else
                distanceToRot = 0;
        }

        if (distanceToRot > 0 && distanceToRot <= distanceCell)
        {
            distanceCell = distanceToRot - 1;
            AudioManager.instance.bounceSound.Play();
        }

        if (distanceToSaw > 0 && distanceToSaw <= distanceCell)
        {
            tileHolder.tile = null;
            awayDistance = distanceToSaw;
            KillTile();
            return;
        }

        if (distanceCell == tileHolder.posX)
        {
            TargetAway();
            return;
        }

        else if (distanceCell == 0)
        {

        }

        else
        {
            int lastHolderIndex = tileHolder.cellIndex;
            tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[lastHolderIndex
                - distanceCell];
            tileHolder.tile = this;
            // Debug.Log("Find Cell Up Distance : " + distanceCell);
            targetDistance = distanceCell;
        }
    }

    private void DetectCellRight()
    {
        //Debug.Log("Find Cell Right");

        if (tileHolder.posX == (GameManager.instance.boardGenerator.currentLevelData.sizeWidth - 1))
        {
            TargetAway();
            return;
        }

        int distanceCell = 0;

        for (int i = 1; i <= GameManager.instance.boardGenerator.currentLevelData.sizeWidth - tileHolder.posX - 1; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex + i].tile == null)
                distanceCell++;
            else
                break;
        }

        int distanceToSaw = 0;

        for (int i = 1; i <= GameManager.instance.boardGenerator.currentLevelData.sizeWidth - tileHolder.posX - 1; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex + i].saw != null)
            {
                distanceToSaw = i;
                nextSaw = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex + i].saw;
                break;
            }

            else
                distanceToSaw = 0;
        }

        int distanceToRot = 0;

        for (int i = 1; i <= GameManager.instance.boardGenerator.currentLevelData.sizeWidth - tileHolder.posX - 1; i++)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex + i].rotator != null)
            {
                distanceToRot = i;
                break;
            }

            else
                distanceToRot = 0;
        }

        if (distanceToRot > 0 && distanceToRot <= distanceCell)
        {
            distanceCell = distanceToRot - 1;
            AudioManager.instance.bounceSound.Play();
        }

        if (distanceToSaw > 0 && distanceToSaw <= distanceCell)
        {
            tileHolder.tile = null;
            awayDistance = distanceToSaw;
            KillTile();
            return;
        }

        if (distanceCell == (GameManager.instance.boardGenerator.currentLevelData.sizeWidth - 1 - tileHolder.posX))
        {
            //Debug.Log("Away");
            TargetAway();
            return;
        }
        else if (distanceCell == 0)
        {
            //Debug.Log("No move");
        }

        else
        {
            int lastHolderIndex = tileHolder.cellIndex;
            tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[lastHolderIndex
                + distanceCell];
            tileHolder.tile = this;
            //Debug.Log("Find Cell Down Distance : " + distanceCell);
            targetDistance = distanceCell;
        }
    }

    private void TargetAway()
    {
        StartCoroutine(PlayDustVfx());
        switch (currentDirection)
        {
            case 0:
               // Debug.Log("Away Up : ");
                awayDistance = tileHolder.posY + 8;
                targetDistance = awayDistance;

                transform.DOLocalMoveY(awayDistance * GameManager.instance.boardGenerator.tileSize,
                    0.1f * awayDistance).SetEase(Ease.Linear).OnComplete(() =>
                {
                    GameManager.instance.CheckRemainBlocks();
                    Destroy(dustVfx.gameObject);
                    Destroy(gameObject);
                });

                break;

            case 3:

               // Debug.Log("Away Right : ");
                awayDistance = GameManager.instance.boardGenerator.currentLevelData.sizeWidth - 1 - tileHolder.posX + 8;
                targetDistance = awayDistance;
                transform.DOLocalMoveX(awayDistance * GameManager.instance.boardGenerator.tileSize,
                    0.1f * awayDistance).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        GameManager.instance.CheckRemainBlocks();
                        Destroy(dustVfx.gameObject);
                        Destroy(gameObject);
                    });

                break;

            case 2:
                //Debug.Log("Away Down : ");
                awayDistance = GameManager.instance.boardGenerator.currentLevelData.sizeHeight - 1 - tileHolder.posY + 8;
                targetDistance = awayDistance;
                transform.DOLocalMoveY(-awayDistance * GameManager.instance.boardGenerator.tileSize,
                   0.1f * awayDistance).SetEase(Ease.Linear).OnComplete(() =>
                   {
                       GameManager.instance.CheckRemainBlocks();
                       Destroy(dustVfx.gameObject);
                       Destroy(gameObject);
                   });
                break;

            case 1:

               // Debug.Log("Away Left : ");
                awayDistance = tileHolder.posX + 8;
                targetDistance = awayDistance;
                transform.DOLocalMoveX(-awayDistance * GameManager.instance.boardGenerator.tileSize,
                   0.1f * awayDistance).SetEase(Ease.Linear).OnComplete(() =>
                   {
                       GameManager.instance.CheckRemainBlocks();
                       Destroy(dustVfx.gameObject);
                       Destroy(gameObject);
                   });
                break;

        }

        tileHolder.tile = null;
        tileHolder = null;
        collider.enabled = false;

    }

    public void KillTile()
    {
        StartCoroutine(PlayDustVfx());
        AudioManager.instance.sawSound.Play();
        if (rotatorParent)
        {
            rotatorParent.rootHandList[handIndexInRotator].ReleaseTileNode(nodeIndexInRotator);
            rotatorParent = null;
        }

        switch (currentDirection)
        {
            case 0:
                //Debug.Log("Kill Up : " + awayDistance);

                transform.DOLocalMoveY(awayDistance * GameManager.instance.boardGenerator.tileSize,
                    0.1f * awayDistance).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        nextSaw.bloodVfx.Play();
                        GameManager.instance.CheckRemainBlocks();
                        if (AudioManager.instance.hapticState == 1)
                            HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);                      
                         Destroy(dustVfx.gameObject);
                        Destroy(gameObject);
                    });

                break;

            case 3:

               // Debug.Log("Away Right : ");

                transform.DOLocalMoveX(awayDistance * GameManager.instance.boardGenerator.tileSize,
                    0.1f * awayDistance).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        nextSaw.bloodVfx.Play();
                        GameManager.instance.CheckRemainBlocks();
                        if (AudioManager.instance.hapticState == 1)
                            HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
                        Destroy(dustVfx.gameObject);
                        Destroy(gameObject);
                    });

                break;

            case 2:
               // Debug.Log("Kill Down : ");

                transform.DOLocalMoveY(-awayDistance * GameManager.instance.boardGenerator.tileSize,
                   0.1f * awayDistance).SetEase(Ease.Linear).OnComplete(() =>
                   {
                       nextSaw.bloodVfx.Play();
                       GameManager.instance.CheckRemainBlocks();
                       if (AudioManager.instance.hapticState == 1)
                           HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
                       Destroy(dustVfx.gameObject);
                       Destroy(gameObject);
                   });
                break;

            case 1:

                //Debug.Log("Away Left : ");

                transform.DOLocalMoveX(-awayDistance * GameManager.instance.boardGenerator.tileSize,
                   0.1f * awayDistance).SetEase(Ease.Linear).OnComplete(() =>
                   {
                       nextSaw.bloodVfx.Play();
                       GameManager.instance.CheckRemainBlocks();
                       if (AudioManager.instance.hapticState == 1)
                           HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
                       Destroy(dustVfx.gameObject);
                       Destroy(gameObject);
                   });
                break;

        }

    }

    public void BreakTile()
    {
        if (tileHolder != null)
        {
            tileHolder.tile = null;
        }
        if (rotatorParent != null)
        {
            rotatorParent.rootHandList[handIndexInRotator].ReleaseTileNode(nodeIndexInRotator);
            rotatorParent = null;
        }
        GameObject exObj = Instantiate(explosionVfx);
        exObj.transform.position = transform.position;
        exObj.transform.localScale = 0.5f * Vector3.one;

        if (AudioManager.instance.hapticState == 1)
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);

        Destroy(exObj, 2.0f);
        GameManager.instance.CheckRemainBlocks();
        Destroy(dustVfx.gameObject);
        Destroy(gameObject);
    }

    public void BreakComboTiles()
    {
        //has near to up tile
        if (tileHolder.cellIndex >= GameManager.instance.boardGenerator.currentLevelData.sizeWidth)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile != null)
            {
                TileElement upTile = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile;
                upTile.BreakTile();
            }
        }

        //has near to down tile
        if (tileHolder.cellIndex <= GameManager.instance.boardController.tileHolderList.Count - GameManager.instance.boardGenerator.currentLevelData.sizeWidth - 1)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile != null)
            {
                TileElement downTile = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth].tile;
                downTile.BreakTile();
            }
        }

        //has near to left tile
        if (tileHolder.cellIndex % GameManager.instance.boardGenerator.currentLevelData.sizeWidth >= 1)
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex - 1].tile != null)
            {
                TileElement leftTile = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex - 1].tile;
                leftTile.BreakTile();
            }
        }

        //has near to right tile
        if (tileHolder.cellIndex % GameManager.instance.boardGenerator.currentLevelData.sizeWidth <= (GameManager.instance.boardGenerator.currentLevelData.sizeWidth - 1))
        {
            if (GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex + 1].tile != null)
            {
                TileElement rightTile = GameManager.instance.boardController.tileHolderList[tileHolder.cellIndex + 1].tile;
                rightTile.BreakTile();
            }
        }
        BreakTile();
    }

    public int directionInRotator;

    public int handIndexInRotator;

    public int nodeIndexInRotator;

    public RotatorElement rotatorParent;

    public void RotateTile(int step, int nodeIndex)
    {
        switch (directionInRotator)
        {
            case 0:
                RotateTileUp(step, nodeIndex);
                break;
            case 1:
                RotateTileLeft(step, nodeIndex);
                break;
            case 2:
                RotateTileDown(step, nodeIndex);
                break;
            case 3:
                RotateTileRight(step, nodeIndex);
                break;
        }
    }

    public bool CanRotate(int step, int nodeIndex)
    {
        switch (directionInRotator)
        {
            case 0:

                if (CanRotateTileUp(step, nodeIndex))
                    return true;
                else
                    return false;
                break;

            case 1:

                if (CanRotateTileLeft(step, nodeIndex))
                    return true;
                else
                    return false;
                break;

            case 2:


                if (CanRotateTileDown(step, nodeIndex))
                    return true;
                else
                    return false;

                break;
            case 3:
                if (CanRotateTileRight(step, nodeIndex))
                    return true;
                else
                    return false;
                break;

            default:
                return true;
        }
    }

    public void RotateTileUp(int step, int nodeIndex)
    {
        collider.enabled = false;
        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;
        int nextCellIndex1 = 0;
        int nextCellIndex2 = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex].saw;

            tileHolder.tile = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                collider.enabled = true;
                directionInRotator = 3;

                if (nextSaw != null)
                {
                    nextSaw.bloodVfx.Play();
                    KillTile();
                }

            });

        }

        else if (step == 2)
        {
            nextCellIndex1 = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
            nextCellIndex2 = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex * 2;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex2];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex2].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex2].saw;


            tileHolder.tile = this;
            transform.SetParent(GameManager.instance.boardController.tileHolderList[nextCellIndex1].transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.SetParent(tileHolder.transform);
                transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    collider.enabled = true;
                    directionInRotator = 2;
                    if (nextSaw != null)
                    {
                        nextSaw.bloodVfx.Play();
                        KillTile();
                    }
                });

            });
        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex].saw;

            tileHolder.tile = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                collider.enabled = true;
                directionInRotator = 1;
                if (nextSaw != null)
                {
                    nextSaw.bloodVfx.Play();
                    KillTile();
                }
            });
        }

    }

    public bool CanRotateTileUp(int step, int nodeIndex)
    {

        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;


        }

        else if (step == 2)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex * 2;

        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
        }

        if (nextCellIndex < 0 || nextCellIndex >= GameManager.instance.boardController.tileHolderList.Count)
            return false;

        if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile != null)
        {
            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile.rotatorParent == null)
                return false;

            else
            {
                if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile.rotatorParent != this.rotatorParent)
                    return false;
                else
                    return true;
            }
        }

        else
            return true;

    }

    public void RotateTileLeft(int step, int nodeIndex)
    {
        collider.enabled = false;
        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;
        int nextCellIndex1 = 0;
        int nextCellIndex2 = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex].saw;

            tileHolder.tile = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                collider.enabled = true;
                directionInRotator = 0;

                if (nextSaw != null)
                {
                    nextSaw.bloodVfx.Play();
                    KillTile();
                }

            });
        }

        else if (step == 2)
        {
            nextCellIndex1 = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
            nextCellIndex2 = cellIndex + nodeIndex * 2;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex2];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex2].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex2].saw;

            tileHolder.tile = this;
            transform.SetParent(GameManager.instance.boardController.tileHolderList[nextCellIndex1].transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.SetParent(tileHolder.transform);
                transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    collider.enabled = true;
                    directionInRotator = 3;

                    if (nextSaw != null)
                    {
                        nextSaw.bloodVfx.Play();
                        KillTile();
                    }
                });

            });
        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex].saw;

            tileHolder.tile = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                collider.enabled = true;
                directionInRotator = 2;
                if (nextSaw != null)
                {
                    nextSaw.bloodVfx.Play();
                    KillTile();
                }
            });
        }

    }

    public bool CanRotateTileLeft(int step, int nodeIndex)
    {

        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
        }

        else if (step == 2)
        {
            nextCellIndex = cellIndex + nodeIndex * 2;

        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
        }

        if (nextCellIndex < 0 || nextCellIndex >= GameManager.instance.boardController.tileHolderList.Count)
            return false;

        if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile != null)
        {
            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile.rotatorParent == null)
                return false;
            else
            {
                if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile.rotatorParent != this.rotatorParent)
                    return false;
                else
                    return true;
            }
        }

        else
            return true;

    }

    public void RotateTileDown(int step, int nodeIndex)
    {
        collider.enabled = false;
        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;
        int nextCellIndex1 = 0;
        int nextCellIndex2 = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex].saw;

            tileHolder.tile = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                collider.enabled = true;
                directionInRotator = 1;

                if (nextSaw != null)
                {
                    nextSaw.bloodVfx.Play();
                    KillTile();
                }
            });
        }

        else if (step == 2)
        {
            nextCellIndex1 = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
            nextCellIndex2 = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex * 2;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex2];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex2].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex2].saw;

            tileHolder.tile = this;
            transform.SetParent(GameManager.instance.boardController.tileHolderList[nextCellIndex1].transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.SetParent(tileHolder.transform);
                transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    collider.enabled = true;
                    directionInRotator = 0;
                    if (nextSaw != null)
                    {
                        nextSaw.bloodVfx.Play();
                        KillTile();
                    }

                });

            });
        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex].saw;

            tileHolder.tile = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                collider.enabled = true;
                directionInRotator = 3;

                if (nextSaw != null)
                {
                    nextSaw.bloodVfx.Play();
                    KillTile();
                }

            });
        }

    }

    public bool CanRotateTileDown(int step, int nodeIndex)
    {

        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
        }

        else if (step == 2)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex * 2;

        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex + nodeIndex;
        }

        if (nextCellIndex < 0 || nextCellIndex >= GameManager.instance.boardController.tileHolderList.Count)
            return false;

        if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile != null)
        {
            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile.rotatorParent == null)
                return false;
            else
            {
                if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile.rotatorParent != this.rotatorParent)
                    return false;
                else
                    return true;
            }
        }

        else
            return true;

    }

    public void RotateTileRight(int step, int nodeIndex)
    {
        collider.enabled = false;
        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;
        int nextCellIndex1 = 0;
        int nextCellIndex2 = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex].saw;

            tileHolder.tile = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                collider.enabled = true;
                directionInRotator = 2;

                if (nextSaw != null)
                {
                    nextSaw.bloodVfx.Play();
                    KillTile();
                }

            });
        }

        else if (step == 2)
        {
            nextCellIndex1 = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
            nextCellIndex2 = cellIndex - nodeIndex * 2;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex2];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex2].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex2].saw;

            tileHolder.tile = this;
            transform.SetParent(GameManager.instance.boardController.tileHolderList[nextCellIndex1].transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.SetParent(tileHolder.transform);
                transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    collider.enabled = true;
                    directionInRotator = 1;

                    if (nextSaw != null)
                    {
                        nextSaw.bloodVfx.Play();
                        KillTile();
                    }

                });

            });
        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
            if (tileHolder.tile == this)
                tileHolder.tile = null;
            tileHolder = GameManager.instance.boardController.tileHolderList[nextCellIndex];

            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].saw != null)
                nextSaw = GameManager.instance.boardController.tileHolderList[nextCellIndex].saw;

            tileHolder.tile = this;
            transform.SetParent(tileHolder.transform);

            transform.DOLocalMove(Vector3.zero, 0.1f * 1.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                collider.enabled = true;
                directionInRotator = 0;
                if (nextSaw != null)
                {
                    nextSaw.bloodVfx.Play();
                    KillTile();
                }
            });
        }

    }

    public bool CanRotateTileRight(int step, int nodeIndex)
    {

        int cellIndex = tileHolder.cellIndex;
        int nextCellIndex = 0;

        if (step == 1)
        {
            nextCellIndex = cellIndex + GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
        }

        else if (step == 2)
        {
            nextCellIndex = cellIndex - nodeIndex * 2;

        }

        else if (step == 3)
        {
            nextCellIndex = cellIndex - GameManager.instance.boardGenerator.currentLevelData.sizeWidth * nodeIndex - nodeIndex;
        }

        if (nextCellIndex < 0 || nextCellIndex >= GameManager.instance.boardController.tileHolderList.Count)
            return false;

        if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile != null)
        {
            if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile.rotatorParent == null)
                return false;
            else
            {
                if (GameManager.instance.boardController.tileHolderList[nextCellIndex].tile.rotatorParent != this.rotatorParent)
                    return false;
                else
                    return true;
            }
        }

        else
            return true;

    }

}
