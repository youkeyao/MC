using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class World : MonoBehaviour
{
    public List<Sprite> previews;
    public Dictionary<string, int> texturesID = new Dictionary<string, int> ();
    public int chunkWidth = 16;
    public int chunkHeight = 16;
    public int renderRadius = 2;
    public int generateRadius = 3;
    public Dictionary<Vector3, Chunk> chunks = new Dictionary<Vector3, Chunk> ();
    public Transform player;
    public GameObject inventory;
    public GameObject helpPanel;
    public GameObject exitGUI;
    public GameObject startGUI;
    public GameObject gameGUI;
    public GameObject progressbar;
    public bool isPause = true;
    public bool isOpenInventory = false;
    public List<Mesh> meshes = new List<Mesh> ();
    public Material myMaterial;

    int seed; // random seed
    bool isThread = false;
    bool pressInventory = false;
    bool pressEscape = false;
    int nowRenderRadius = 0;
    int countProgress = 0;
    int[] nowRenderPos;
    Queue<Vector3> initList = new Queue<Vector3> ();
    Vector3[,,] renderList;
    List<Texture2D> textures = new List<Texture2D> ();

    void Start()
    {
        gameGUI.SetActive(false);
        startGUI.SetActive(true);
        exitGUI.SetActive(false);

        // load meshes
        for (int i = 0; i < BlockData.blockTypes.Count; i ++) {
            Mesh mesh = new Mesh();
            mesh.vertices = BlockData.blockTypes[i].voxelVerts.ToArray();
            mesh.SetTriangles(BlockData.blockTypes[i].voxelTris.ToArray(), 0);
            meshes.Add(mesh);
        }

        // load textures
        foreach (BlockInfo b in BlockData.allblocks) {
            previews.Add(Resources.Load<Sprite>(b.preview));
            foreach (string p in b.mats) {
                if (!texturesID.ContainsKey(p)) {
                    texturesID.Add(p, textures.Count);
                    textures.Add(Resources.Load<Texture2D>(p));
                }
            }
        }

        // load into texture array
        Texture2DArray textureArray = new Texture2DArray(textures[0].width, textures[0].height, textures.Count, textures[0].format, false, true);
        for (int i = 0; i < textures.Count; i ++) {
            Graphics.CopyTexture(textures[i], 0, 0, textureArray, i, 0);
        }
        textureArray.Apply();

        // create material
        myMaterial = new Material(Shader.Find("Custom/MyShader"));
        myMaterial.SetTexture("_TextureArray", textureArray);
    }

    public void GameStart()
    {
        while (seed < 100000) seed = System.BitConverter.ToInt32(System.Guid.NewGuid().ToByteArray(), 0);

        nowRenderPos = new int[3] {0, 0, 0};

        inventory.GetComponent<HandleInventory>().Init();
        inventory.SetActive(false);
        progressbar.SetActive(true);

        renderList = new Vector3[2*generateRadius + 1, 2*generateRadius + 1, 2*generateRadius + 1];
        CheckChunkRender(new Vector3(0, 0, 0));

        // thread to init chunk
        isThread = true;
        for (int i = 0; i < 3; i ++) {
            new Thread(new ThreadStart(() => {
                while (isThread) {
                    int count;
                    Vector3 id = Vector3.zero;
                    lock (initList) { 
                        count = initList.Count;
                        if (count > 0) {
                            id = initList.Dequeue();
                        }
                    }
                    if (count > 0) {
                        SetTerrain(chunks[id], id);
                    }
                }
            })).Start();
        }
    }

    public void GameExit()
    {
        isThread = false;
        initList.Clear();
        foreach (Chunk chunk in chunks.Values) {
            Destroy(chunk.chunkObject);
        }
        chunks.Clear();
        gameGUI.SetActive(false);
        startGUI.SetActive(true);
        exitGUI.SetActive(false);
        countProgress = 0;
        player.position = new Vector3(0, 15, 0);
        player.rotation = Quaternion.Euler(0, 0, 0);
        Camera.main.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public void OnEscape()
    {
        if (isPause) {
            isPause = false;
            exitGUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else {
            isPause = true;
            helpPanel.SetActive(false);
            exitGUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void OnHelp()
    {
        if (helpPanel.activeSelf) {
            helpPanel.SetActive(false);
        }
        else {
            helpPanel.SetActive(true);
        }
    }

    void Update()
    {
        if (!startGUI.activeSelf) {
            Vector3 nowPlayerPos = GetChunkID(player.position);

            CheckChunkRender(nowPlayerPos);

            if (Input.GetAxis("Inventory") != 0.0f) {
                if (!pressInventory) {
                    pressInventory = true;
                    if (isOpenInventory) {
                        isOpenInventory = false;
                        inventory.SetActive(false);
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                    else {
                        isOpenInventory = true;
                        inventory.SetActive(true);
                        Cursor.lockState = CursorLockMode.None;
                    }
                }
            }
            else pressInventory = false;

            if (Input.GetAxis("Pause") != 0.0f) {
                if (!pressEscape) {
                    pressEscape = true;
                    OnEscape();
                }
            }
            else pressEscape = false;
        }
        else {
            progressbar.GetComponent<Slider>().value = (float)countProgress / (2*renderRadius + 1) / (2*renderRadius + 1) / (2*renderRadius + 1);
            if (countProgress >= (2*renderRadius + 1) * (2*renderRadius + 1) * (2*renderRadius + 1)) {
                countProgress = 0;
                startGUI.SetActive(false);
                gameGUI.SetActive(true);
                progressbar.SetActive(false);
                isPause = false;
                player.position = new Vector3(0, 15, 0);
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    void OnDestroy()
    {
        isThread = false;
    }

    public Vector3 GetChunkID(Vector3 pos)
    {
        return new Vector3(Mathf.Floor(pos.x / chunkWidth), Mathf.Floor(pos.y / chunkHeight), Mathf.Floor(pos.z / chunkWidth));
    }

    bool isNear(Vector3 chunkid, Vector3 now)
    {
        return -renderRadius <= chunkid.x - now.x && chunkid.x - now.x <= renderRadius && -renderRadius <= chunkid.y - now.y && chunkid.y - now.y <= renderRadius && -renderRadius <= chunkid.z - now.z && chunkid.z - now.z <= renderRadius;
    }

    void CheckChunkRender(Vector3 now)
    {
        for (int i = 0; i < (2*generateRadius + 1) * (2*generateRadius + 1) * (2*generateRadius + 1); i ++) {
            // out of range
            if (chunks.ContainsKey(renderList[nowRenderPos[0] + generateRadius, nowRenderPos[1] + generateRadius, nowRenderPos[2] + generateRadius])) {
                Vector3 chunkid = renderList[nowRenderPos[0] + generateRadius, nowRenderPos[1] + generateRadius, nowRenderPos[2] + generateRadius];
                if (!isNear(chunkid, now) && chunks[chunkid].chunkObject.GetComponent<Renderer>().enabled == true) {
                    chunks[chunkid].chunkObject.GetComponent<Renderer>().enabled = false;
                }
            }
            Vector3 id = now + new Vector3(nowRenderPos[0], nowRenderPos[1], nowRenderPos[2]);
            // new chunk
            if (!chunks.ContainsKey(id)) {
                chunks.Add(id, new Chunk(this));
                lock (initList) {
                    initList.Enqueue(id);
                }
            }
            // render
            if (chunks[id].isDirty) {
                chunks[id].CreateMesh();
            }
            if (isNear(id, now) && chunks[id].chunkObject.GetComponent<Renderer>().enabled == false) {
                chunks[id].chunkObject.GetComponent<Renderer>().enabled = true;
            }
            renderList[nowRenderPos[0] + generateRadius, nowRenderPos[1] + generateRadius, nowRenderPos[2] + generateRadius] = id;

            // next chunk pos
            if (nowRenderPos[0] == -nowRenderRadius || nowRenderPos[0] == nowRenderRadius || nowRenderPos[1] == -nowRenderRadius || nowRenderPos[1] == nowRenderRadius) {
                nowRenderPos[2] += 1;
            }
            else {
                if (nowRenderPos[2] == -nowRenderRadius) {
                    nowRenderPos[2] = nowRenderRadius;
                }
                else {
                    nowRenderPos[2] = -nowRenderRadius;
                    nowRenderPos[1] += 1;
                }
            }
            if (nowRenderPos[2] > nowRenderRadius) {
                nowRenderPos[2] = -nowRenderRadius;
                nowRenderPos[1] += 1;
            }
            if (nowRenderPos[1] > nowRenderRadius) {
                nowRenderPos[1] = -nowRenderRadius;
                nowRenderPos[0] += 1;
            }
            if (nowRenderPos[0] > nowRenderRadius) {
                nowRenderRadius += 1;
                if (nowRenderRadius > generateRadius) {
                    nowRenderRadius = 0;
                }
                nowRenderPos[0] = -nowRenderRadius;
                nowRenderPos[1] = -nowRenderRadius;
                nowRenderPos[2] = -nowRenderRadius;
            }
        }
    }

    void SetTerrain(Chunk chunk, Vector3 id)
    {
        if (id.y == 0) {
            int[,] mat = GenerateHeightMap(id);
            for (int x = 0; x < chunkWidth; x ++) {
                for (int z = 0; z < chunkWidth; z ++) {
                    Vector3 pos = new Vector3(id.x * chunkWidth + x, 0, id.z * chunkWidth + z) + new Vector3(0.5f, 0.5f, 0.5f);
                    for (int y = 0; y < mat[x, z]; y ++) {
                        chunk.blocks.Add(pos.ToString(), new Block(pos, Quaternion.Euler(0, 0, 0), 2));
                        pos.y += 1;
                    }
                    chunk.blocks.Add(pos.ToString(), new Block(pos, Quaternion.Euler(0, 0, 0), 0));
                    pos.y += 1;
                    chunk.blocks.Add(pos.ToString(), new Block(pos, Quaternion.Euler(0, 0, 0), 0));
                    pos.y += 1;
                    chunk.blocks.Add(pos.ToString(), new Block(pos, Quaternion.Euler(0, 0, 0), 1));
                }
            }
            GenerateTrees(mat, chunk, id);
            chunk.LoadBlockData();
            chunk.isDirty = true;
        }
        else if (id.y < 0) {
            for (int x = 0; x < chunkWidth; x ++) {
                for (int y = 0; y < chunkHeight; y ++) {
                    for (int z = 0; z < chunkWidth; z ++) {
                        Vector3 pos = new Vector3(id.x * chunkWidth + x, id.y * chunkHeight + y, id.z * chunkWidth + z) + new Vector3(0.5f, 0.5f, 0.5f);
                        chunk.blocks.Add(pos.ToString(), new Block(pos, Quaternion.Euler(0, 0, 0), 2));
                    }
                }
            }
            chunk.LoadBlockData();
            chunk.isDirty = true;
        }
        countProgress += 1;
    }

    int[,] GenerateHeightMap(Vector3 id)
    {
        int[,] mat = new int[chunkWidth, chunkWidth];
        float lefttop = Random((id.x - 1) * chunkWidth + chunkWidth / 2, (id.z + 1) * chunkWidth + chunkWidth / 2);
        float left = Random((id.x - 1) * chunkWidth + chunkWidth / 2, id.z * chunkWidth + chunkWidth / 2);
        float leftbottom = Random((id.x - 1) * chunkWidth + chunkWidth / 2, (id.z - 1) * chunkWidth + chunkWidth / 2);
        float top = Random(id.x * chunkWidth + chunkWidth / 2, (id.z + 1) * chunkWidth + chunkWidth / 2);
        float center = Random(id.x * chunkWidth + chunkWidth / 2, id.z * chunkWidth + chunkWidth / 2);
        float bottom = Random(id.x * chunkWidth + chunkWidth / 2, (id.z - 1) * chunkWidth + chunkWidth / 2);
        float righttop = Random((id.x + 1) * chunkWidth + chunkWidth / 2, (id.z + 1) * chunkWidth + chunkWidth / 2);
        float right = Random((id.x + 1) * chunkWidth + chunkWidth / 2, id.z * chunkWidth + chunkWidth / 2);
        float rightbottom = Random((id.x + 1) * chunkWidth + chunkWidth / 2, (id.z - 1) * chunkWidth + chunkWidth / 2);
        for (int i = 0; i < chunkWidth; i ++) {
            for (int j = 0; j < chunkWidth; j ++) {
                float tmp = 0;
                if (i < chunkWidth / 2) {
                    if (j < chunkWidth / 2) {
                        tmp += left * (chunkWidth / 2 - i) * (j + chunkWidth / 2);
                        tmp += bottom * (i + chunkWidth / 2) * (chunkWidth / 2 - j);
                        tmp += leftbottom * (chunkWidth / 2 - i) * (chunkWidth / 2 - j);
                        tmp += center * (i + chunkWidth / 2) * (j + chunkWidth / 2);
                        tmp  = tmp / chunkWidth / chunkWidth;
                    }
                    else {
                        tmp += left * (chunkWidth / 2 - i) * (chunkWidth - 1 - j + chunkWidth / 2);
                        tmp += top * (i + chunkWidth / 2) * (j - chunkWidth / 2);
                        tmp += lefttop * (chunkWidth / 2 - i) * (j - chunkWidth / 2);
                        tmp += center * (i + chunkWidth / 2) * (chunkWidth - 1 - j + chunkWidth / 2);
                        tmp  = tmp / chunkWidth / chunkWidth;
                    }
                }
                else {
                    if (j < chunkWidth / 2) {
                        tmp += right * (i - chunkWidth / 2) * (j + chunkWidth / 2);
                        tmp += bottom * (chunkWidth - 1 - i + chunkWidth / 2) * (chunkWidth / 2 - j);
                        tmp += rightbottom * (i - chunkWidth / 2) * (chunkWidth / 2 - j);
                        tmp += center * (chunkWidth - 1 - i + chunkWidth / 2) * (j + chunkWidth / 2);
                        tmp  = tmp / chunkWidth / chunkWidth;
                    }
                    else {
                        tmp += right * (i - chunkWidth / 2) * (chunkWidth - 1 - j + chunkWidth / 2);
                        tmp += top * (chunkWidth - 1 - i + chunkWidth / 2) * (j - chunkWidth / 2);
                        tmp += righttop * (i - chunkWidth / 2) * (j - chunkWidth / 2);
                        tmp += center * (chunkWidth - 1 - i + chunkWidth / 2) * (chunkWidth - 1 - j + chunkWidth / 2);
                        tmp  = tmp / chunkWidth / chunkWidth;
                    }
                }
                mat[i, j] = (int)(tmp * (chunkHeight - 8));
            }
        }
        return mat;
    }

    void GenerateTrees(int[,] heightmap, Chunk chunk, Vector3 id)
    {
        for(int i = 0; i < 16; i ++) {
            System.Random r = new System.Random(System.BitConverter.ToInt32(System.Guid.NewGuid().ToByteArray(), 0));
            int x = r.Next(3, chunkWidth - 3);
            int z = r.Next(3, chunkWidth - 3);
            bool flag = true;
            for (int j = x - 3; j <= x + 3 && flag; j ++) {
                for (int k = z - 3; k <= z + 3; k ++) {
                    if (heightmap[j, k] != heightmap[x, z]) {
                        flag = false;
                        break;
                    }
                }
            }
            Vector3 pos = new Vector3(id.x * chunkWidth + x, id.y * chunkHeight + heightmap[x, z] + 3, id.z * chunkWidth + z) + new Vector3(0.5f, 0.5f, 0.5f);
            if (flag && !chunk.blocks.ContainsKey(pos.ToString())) {
                Quaternion rot = Quaternion.Euler(0, r.Next(-45, 45), 0);
                chunk.blocks.Add(pos.ToString(), new Block(pos, rot, 8));
                pos.y += 1;
                chunk.blocks.Add(pos.ToString(), new Block(pos, rot, 8));
                pos.y += 1;
                chunk.blocks.Add(pos.ToString(), new Block(pos, rot, 8));
                chunk.blocks.Add((pos + rot * Vector3.forward).ToString(), new Block(pos + rot * Vector3.forward, rot, 6));
                chunk.blocks.Add((pos - rot * Vector3.forward).ToString(), new Block(pos - rot * Vector3.forward, rot, 6));
                chunk.blocks.Add((pos + rot * Vector3.right).ToString(), new Block(pos + rot * Vector3.right, rot, 6));
                chunk.blocks.Add((pos - rot * Vector3.right).ToString(), new Block(pos - rot * Vector3.right, rot, 6));
                pos.y += 1;
                chunk.blocks.Add(pos.ToString(), new Block(pos, rot, 6));
                heightmap[x, z] += 4;
            }
        }
    }

    float Random(float x, float z)
    {
        uint n = 22695477 * (uint)x + 1664525 * (uint)z;
        n = (n << 13) ^ n;
        return (n * 1013904223 + seed) / 4294967296.0f;
    }
}