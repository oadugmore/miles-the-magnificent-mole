using UnityEngine;
using System.Collections;

public class RandomTerrainGenerator : MonoBehaviour
{
    private const int ChunkTypeCount = 6;
    enum ChunkType
    {
        ChooseForPowerup, GetToOne, ChooseToAvoidLava, GeyserTwoColumns, GeyserThreeColumns, GeyserAndLava
    }

    //private const int RowTypeCount = 8;
    //enum RowType
    //{
    //    Clear, OneDigSpot, TwoDigSpots, ThreeDigSpots, Lava, GeyserTwoSolid, GeyserOneSolid, TwoGeysers, GeyserAndLava
    //}

    //private const int BlockTypeCount = 5;
    enum BlockType
    {
        SolidGround, SoftGround, Air, Lava, Geyser
    }

    #region Properties

    public int colliderDisableDistance;
    public ObjectPooler softGroundPool;
    public ObjectPooler solidGroundPool;
    public ObjectPooler geyserPool;
    public ObjectPooler pizzaPool;
    public ObjectPooler jetpackPool;
    public ObjectPooler lavaPool;
    //public GameObject solidGround;
    //public GameObject softGround;
    //public GameObject lava;
    //public GameObject geyser;
    //public GameObject powerup;
    public float generationBeginX;
    public float generationBeginY;
    public float generationTriggerOffset;
    [HideInInspector]
    public float generationNextTrigger;

    private ChunkType _lastChunkType = ChunkType.GetToOne;
    private Transform player;

    private bool _generatingChunk = false;
    public bool generatingChunk
    {
        get { return _generatingChunk; }
        private set { _generatingChunk = value; }
    }

    private float _generationNextX;
    public float generationNextX
    {
        get { return _generationNextX; }
        private set { _generationNextX = value; }
    }

    private float _generationNextY;
    public float generationNextY
    {
        get { return _generationNextY; }
        private set { _generationNextY = value; }
    }

    private int _chunksGenerated;
    public int chunksGenerated
    {
        get { return _chunksGenerated; }
        private set { _chunksGenerated = value; }
    }
    #endregion

    // Use this for initialization
    void Start()
    {
        generationNextX = generationBeginX;
        generationNextY = generationBeginY;
        generationNextTrigger = generationBeginY + generationTriggerOffset;
        player = FindObjectOfType<PlayerController>().GetComponent<Transform>();
        //StartCoroutine(CheckEnableDisableColliders());
    }

    //private IEnumerator CheckEnableDisableColliders()
    //{
    //    foreach (GameObject chunk in softGroundPool.pooledObjects)
    //    {
    //        if (chunk.activeInHierarchy)
    //        {
    //            var children = chunk.GetComponentsInChildren<Collider2D>();
    //            if (children.Length < 2) continue;
    //            if (children[1].isActiveAndEnabled && Mathf.Abs(player.position.y - chunk.transform.position.y) > colliderDisableDistance)
    //            {
    //                //Debug.Log("Disabling colliders...");
    //                yield return null;
    //                for (int i = 1; i < children.Length; i++)
    //                {
    //                    children[i].enabled = false;
    //                }
    //            }
    //            else if (!children[1].isActiveAndEnabled && Mathf.Abs(player.position.y - chunk.transform.position.y) <= colliderDisableDistance)
    //            {
    //                //Debug.Log("Enabling colliders...");
    //                yield return null;
    //                for (int i = 1; i < children.Length; i++)
    //                {
    //                    children[i].enabled = true;
    //                }
    //            }
    //        }
    //    }
    //    yield return new WaitForSeconds(0.5f);
    //}

    private IEnumerator WaitToGenerateChunk()
    {
        while (_generatingChunk)
            yield return null;
        GenerateChunk();
    }

    public void GenerateChunk()
    {
        if (_generatingChunk)
        {
            StartCoroutine(WaitToGenerateChunk());
            return;
        }

        chunksGenerated++;
        ChunkType chunkType = (ChunkType)Random.Range(0, ChunkTypeCount);
        if (chunkType == _lastChunkType && (Util.CoinFlip() || Util.CoinFlip())) chunkType = (ChunkType)Random.Range(0, ChunkTypeCount);
        if (chunksGenerated < 6 && (chunkType == ChunkType.ChooseToAvoidLava || chunkType == ChunkType.GeyserTwoColumns || chunkType == ChunkType.GeyserThreeColumns)) chunkType = Util.CoinFlip() ? ChunkType.ChooseForPowerup : ChunkType.GetToOne;
        if (chunkType == ChunkType.GeyserTwoColumns && Random.Range(0, 3) == 0) chunkType = ChunkType.GeyserThreeColumns;
        _lastChunkType = chunkType;
        _generatingChunk = true;

        switch (chunkType)
        {
            case ChunkType.ChooseForPowerup:
                {
                    StartCoroutine(GeneratePowerupChunkAsync(1));
                    break;
                }
            case ChunkType.GetToOne:
                {
                    StartCoroutine(GenerateGet2OneChunkAsync());
                    break;
                }
            case ChunkType.ChooseToAvoidLava:
                {
                    StartCoroutine(GenerateLavaChunkAsync());
                    break;
                }
            case ChunkType.GeyserTwoColumns:
                {
                    StartCoroutine(GenerateGeyserTwoColumnChunkAsync());
                    break;
                }
            case ChunkType.GeyserThreeColumns:
                {
                    StartCoroutine(GenerateGeyserThreeColumnChunkAsync());
                    break;
                }
            case ChunkType.GeyserAndLava:
                {
                    StartCoroutine(GenerateGeyserAndLavaChunkAsync());
                    break;
                }
            default:
                break;
        }
    }

    #region Chunks

    /// <summary>
    /// 
    /// </summary>
    /// <param name="powerupType">1 = Pizza, 2 = Jetpack</param>
    /// <returns></returns>
    private IEnumerator GeneratePowerupChunkAsync(int powerupType)
    {
        //args[0] (arg1) is the row with the powerup
        yield return null;
        int arg1 = Random.Range(0, 4);
        int arg2 = arg1;
        int delta = Random.Range(2, 4);
        if (arg1 < 2) arg2 += delta;
        else arg2 -= delta;
        arg2 = Mathf.Clamp(arg2, 0, 3);
        //int[] args = { arg1, arg2 };
        yield return null;
        GenerateTwoDigSpotsRow(arg1, arg2);
        yield return null;
        GenerateTwoDigSpotsRow(arg1, arg2);
        yield return null;
        GenerateTwoDigSpotsRow(arg1, arg2, arg1, powerupType);
        yield return null;

        for (int i = 0; i < 2; i++)
        {
            GenerateClearRow();
            yield return null;
        }
        _generatingChunk = false;
    }

    private IEnumerator GenerateGet2OneChunkAsync()
    {
        yield return null;
        int softGround = Random.Range(0, 4);
        yield return null;
        GenerateOneDigSpotRow(softGround);
        yield return null;
        GenerateOneDigSpotRow(softGround);
        yield return null;

        for (int i = 0; i < 2; i++)
        {
            GenerateClearRow();
            yield return null;
        }
        _generatingChunk = false;
    }

    private IEnumerator GenerateLavaChunkAsync()
    {
        //args[1] is the column with the lava
        yield return null;
        int softGround = Random.Range(0, 4);
        int lava = softGround;
        int delta = Random.Range(2, 4);
        if (softGround < 2) lava += delta;
        else lava -= delta;
        lava = Mathf.Clamp(lava, 0, 3);
        //int[] args = { arg1, arg2 };
        yield return null;
        for (int i = 0; i < 2; i++)
        {
            GenerateTwoDigSpotsRow(softGround, lava);
            yield return null;
        }
        GenerateLavaRowTwoSolid(lava, softGround);
        yield return null;
        GenerateOneDigSpotRow(softGround);
        yield return null;

        for (int i = 0; i < 2; i++)
        {
            GenerateClearRow();
            yield return null;
        }
        _generatingChunk = false;
    }

    private IEnumerator GenerateGeyserTwoColumnChunkAsync()
    {
        //args[1] is the column with the geyser
        yield return null;
        int arg1 = Random.Range(0, 4);
        int arg2 = arg1 + 1;
        if (arg2 == 4) arg2 = 2;
        //int[] args = { arg1, arg2 };
        if (Util.CoinFlip())
        {
            int c = arg2;
            arg2 = arg1;
            arg1 = c;
        }

        int count = Random.Range(6, 9);
        yield return null;
        for (int i = 0; i < count; i++)
        {
            GenerateTwoDigSpotsRow(arg1, arg2);
            yield return null;
        }
        GenerateGeyserTwoSolidRow(arg2, arg1);
        yield return null;
        GenerateOneDigSpotRow(arg1);
        yield return null;

        for (int i = 0; i < 2; i++)
        {
            GenerateClearRow();
            yield return null;
        }
        _generatingChunk = false;
    }

    private IEnumerator GenerateGeyserThreeColumnChunkAsync()
    {
        //powerupIndex is used to determine if and where a geyser should spawn in RowType.GeyserOneSolid
        yield return null;
        int solidGround = 0;
        int softGround = 0;
        if (Util.CoinFlip())
        {
            solidGround = 0;
            softGround = Random.Range(1, 4);//solidGround;
        }
        else
        {
            solidGround = 3;
            softGround = Random.Range(0, 3);
        }

        //while (softGround == solidGround) softGround = Random.Range(1, 3);
        //if (Mathf.Abs(solidGround - softGround) == 2 && Random.Range(0, 2) == 1)
        //{
        //    while (softGround == solidGround) softGround = Random.Range(1, 3);
        //}

        int geyser1 = 0;
        int geyser2 = geyser1;
        while (geyser1 == solidGround || geyser1 == softGround) geyser1++;
        while (geyser2 == solidGround || geyser2 == softGround || geyser2 == geyser1) geyser2++;
        if (Mathf.Abs(solidGround - geyser1) == 2)
        {
            int c = geyser1;
            geyser1 = geyser2;
            geyser2 = c;
        }
        //int[] args = { solidGround, softGround, geyser1, geyser2 };

        int count = Random.Range(5, 8);
        bool extra = Util.CoinFlip();
        yield return null;
        for (int i = 0; i < count; i++)
        {
            GenerateThreeDigSpotsRow(solidGround);
            yield return null;
        }
        if (!extra)
        {
            GenerateTwoGeysersRow(geyser1, geyser2, softGround);
        }
        else
        {
            GenerateGeyserOneSolidRow(solidGround, geyser1, softGround);
            yield return null;
            if (Util.CoinFlip()) GenerateTwoDigSpotsRow(softGround, geyser2);
            yield return null;
            GenerateGeyserTwoSolidRow(geyser2, softGround);
        }
        yield return null;

        for (int i = 0; i < 2; i++)
        {
            GenerateClearRow();
            yield return null;
        }
        _generatingChunk = false;
    }

    private IEnumerator GenerateGeyserAndLavaChunkAsync()
    {
        yield return null;
        int solidGround = 0;
        int geyser = 0;
        int lava = 0;
        int softGround = 0;
        if (Util.CoinFlip())
        {
            solidGround = 0;
            geyser = 2;
            lava = Util.CoinFlip() ? 1 : 3;
        }
        else
        {
            solidGround = 3;
            geyser = 1;
            lava = Util.CoinFlip() ? 0 : 2;
        }
        //softGround = 6 - solidGround + geyser + lava;
        while (softGround == solidGround || softGround == geyser || softGround == lava) softGround++;
        yield return null;

        int count = Random.Range(6, 9);

        for (int i = 0; i < count; i++)
        {
            GenerateThreeDigSpotsRow(solidGround);
            yield return null;
        }

        //GenerateLavaRowOneSolid(lava, solidGround);
        yield return null;
        //GenerateGeyserTwoSolidRow(geyser, softGround, true);
        GenerateGeyserAndLavaRow(geyser, lava, solidGround);
        GenerateOneDigSpotRow(softGround);
        yield return null;

        for (int i = 0; i < 2; i++)
        {
            GenerateClearRow();
            yield return null;
        }
        _generatingChunk = false;
    }

    #endregion

    #region Rows

    private void AddRowPadding()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnBlock(BlockType.SolidGround);
            generationNextX += 3;
        }
    }

    private void RepositionForNewRow()
    {
        generationNextX = generationBeginX;
        generationNextY -= 3;
        generationNextTrigger = generationNextY + generationTriggerOffset;
    }

    private void GenerateClearRow()
    {
        AddRowPadding();
        generationNextX += 12;
        AddRowPadding();
        RepositionForNewRow();
    }

    private void GenerateOneDigSpotRow(int softGround)
    {
        AddRowPadding();
        for (int i = 0; i < 4; i++)
        {
            BlockType type = (i == softGround) ? BlockType.SoftGround : BlockType.SolidGround;
            SpawnBlock(type);
            generationNextX += 3;
        }
        AddRowPadding();
        RepositionForNewRow();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="soft1"></param>
    /// <param name="soft2"></param>
    /// <param name="powerupColumn"></param>
    /// <param name="powerupType">1 = Pizza, 2 = Jetpack</param>
    private void GenerateTwoDigSpotsRow(int soft1, int soft2, int powerupColumn = -1, int powerupType = -1)
    {
        AddRowPadding();
        for (int i = 0; i < 4; i++)
        {
            BlockType type = (i == soft1 || i == soft2) ? BlockType.SoftGround : BlockType.SolidGround;
            SpawnBlock(type);
            if (powerupColumn == i)
            {
                SpawnPizza();
            }
            generationNextX += 3;
        }
        AddRowPadding();
        RepositionForNewRow();
    }

    private void GenerateThreeDigSpotsRow(int solidGround)
    {
        AddRowPadding();
        for (int i = 0; i < 4; i++)
        {
            BlockType type = (i == solidGround) ? BlockType.SolidGround : BlockType.SoftGround;
            SpawnBlock(type);
            //if (powerupIndex == i)
            //{
            //    SpawnPowerup();
            //}
            generationNextX += 3;
        }
        AddRowPadding();
        RepositionForNewRow();
    }

    private void GenerateLavaRowOneSolid(int lava, int solidGround)
    {
        AddRowPadding();
        for (int i = 0; i < 4; i++)
        {
            BlockType type = BlockType.SoftGround;
            if (i == lava) type = BlockType.Lava;
            else if (i == solidGround) type = BlockType.SolidGround;
            SpawnBlock(type);
            generationNextX += 3;
        }
        AddRowPadding();
        RepositionForNewRow();
    }

    private void GenerateLavaRowTwoSolid(int lava, int softGround)
    {
        AddRowPadding();
        for (int i = 0; i < 4; i++)
        {
            BlockType type = BlockType.SolidGround;
            if (i == lava) type = BlockType.Lava;
            else if (i == softGround) type = BlockType.SoftGround;
            SpawnBlock(type);
            generationNextX += 3;
        }
        AddRowPadding();
        RepositionForNewRow();
    }

    private void GenerateGeyserTwoSolidRow(int geyser, int softGround, bool shortGeyserTriggerDistance = false)
    {
        AddRowPadding();
        for (int i = 0; i < 4; i++)
        {
            BlockType type = BlockType.SolidGround;
            if (i == geyser) type = BlockType.Geyser;
            else if (i == softGround) type = BlockType.SoftGround;
            SpawnBlock(type, shortGeyserTriggerDistance);
            generationNextX += 3;
        }
        AddRowPadding();
        RepositionForNewRow();
    }

    private void GenerateGeyserOneSolidRow(int solidGround, int geyser, int powerup = -1, bool shortGeyserTriggerDistance = false)
    {
        AddRowPadding();
        for (int i = 0; i < 4; i++)
        {
            BlockType type = BlockType.SoftGround;
            if (i == solidGround) type = BlockType.SolidGround;
            else if (i == geyser) type = BlockType.Geyser;
            if (i == powerup) SpawnPizza();
            SpawnBlock(type, shortGeyserTriggerDistance);
            generationNextX += 3;
        }
        AddRowPadding();
        RepositionForNewRow();
    }

    private void GenerateTwoGeysersRow(int geyser1, int geyser2, int softGround)
    {
        AddRowPadding();
        for (int i = 0; i < 4; i++)
        {
            BlockType type = BlockType.SolidGround;
            if (i == geyser1 || i == geyser2) type = BlockType.Geyser;
            else if (i == softGround) type = BlockType.SoftGround;
            SpawnBlock(type);
            generationNextX += 3;
        }
        AddRowPadding();
        RepositionForNewRow();
    }

    private void GenerateGeyserAndLavaRow(int geyser, int lava, int solidGround)
    {
        AddRowPadding();
        for (int i = 0; i < 4; i++)
        {
            BlockType type = BlockType.SoftGround;
            if (i == geyser) type = BlockType.Geyser;
            else if (i == solidGround) type = BlockType.SolidGround;
            else if (i == lava) type = BlockType.Lava;
            SpawnBlock(type);
            generationNextX += 3;
        }
        AddRowPadding();
        RepositionForNewRow();
    }

    #endregion

    private void SpawnPizza()
    {
        var powerup = pizzaPool.GetPooledObject();
        powerup.transform.position = new Vector3(generationNextX, generationNextY);
        powerup.SetActive(true);
        //powerup.GetComponent<PowerupController>().Reset();
    }

    private IEnumerator ActivateChildren(GameObject obj)
    {
        yield return null;
        var children = obj.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            children[i].gameObject.SetActive(true);
            if (i % 5 == 0) yield return null;
        }

        //foreach (Transform t in children)
        //{
        //    t.gameObject.SetActive(true);
        //}
    }

    private void SpawnBlock(BlockType type, bool shortGeyserTriggerDistance = false)
    {
        GameObject block;

        switch (type)
        {
            case BlockType.SolidGround:
                block = solidGroundPool.GetPooledObject();
                break;
            case BlockType.SoftGround:
                block = softGroundPool.GetPooledObject();
                //block.BroadcastMessage("SetActive", true);
                block.SetActive(true);
                StartCoroutine(ActivateChildren(block));
                //var children = block.GetComponentsInChildren<Transform>(true);
                //foreach (Transform t in children)
                //{
                //    t.gameObject.SetActive(true);
                //}
                break;
            case BlockType.Air:
                return;
            case BlockType.Lava:
                block = lavaPool.GetPooledObject();
                break;
            case BlockType.Geyser:
                block = geyserPool.GetPooledObject();
                block.SetActive(true);
                StartCoroutine(ActivateChildren(block));
                break;
            default:
                block = new GameObject();
                Debug.LogError("Spawning default block. This shouldn't happen!");
                break;
        }

        //Instantiate(block, position, Quaternion.identity);
        block.transform.position = new Vector3(generationNextX, generationNextY);
        block.SetActive(true);
        if (type == BlockType.Geyser) block.GetComponent<GeyserController>().CalculateTriggerNextFrame(shortGeyserTriggerDistance);
    }

}
