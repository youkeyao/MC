using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class Chunk
{
    public Dictionary<string, Block> blocks = new Dictionary<string, Block> ();
    public GameObject chunkObject;
    public bool isDirty = false;
    public List<Vector3> vertices = new List<Vector3> ();
    public List<Vector2> uvs = new List<Vector2> ();
    
    int vertexIndex = 0;
    List<int> triangles = new List<int> ();
    List<Vector2> infos = new List<Vector2> ();
    
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    ObiCollider obiCollider;

    World world;

    public Chunk(World world)
    {
        this.world = world;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        meshRenderer.material = world.myMaterial;
    }

    bool CheckRender(Block b, BlockType type, int faceIndex) {
        Vector3 l1 = b.rot * BlockType.GetAABBDistance(type, type.centerDistances[faceIndex], Quaternion.Euler(0, 0, 0));
        Vector3 l2 = b.rot * BlockType.GetAABBDistance(type, -type.centerDistances[faceIndex], Quaternion.Euler(0, 0, 0));
        Vector3 pos = b.pos + l1 - l2;

        return !(blocks.ContainsKey(pos.ToString()) && blocks[pos.ToString()].rot == b.rot && BlockData.allBlocks[b.id].type == BlockData.allBlocks[blocks[pos.ToString()].id].type);
    }

    public void LoadBlockData()
    {
        foreach (Block b in blocks.Values) {
            BlockType type = BlockData.blockTypes[BlockData.allBlocks[b.id].type];
            for (int i = 0; i < type.voxelTris.Count; i += 3) {
                int faceIndex = i / 3;
                if (CheckRender(b, type, faceIndex)) {
                    // encode uv (type + faceIndex * typeNum, texID)
                    int texID = BlockData.allBlocks[b.id].texIDs != null && BlockData.allBlocks[b.id].texIDs.Length > faceIndex ? BlockData.allBlocks[b.id].texIDs[faceIndex] : 0;
                    texID = world.texturesID[BlockData.allBlocks[b.id].mats[texID]];
                    Vector2 info = new Vector2(BlockData.allBlocks[b.id].type + faceIndex * BlockData.blockTypes.Count, texID);
                    for (int j = 0; j < 3; j ++) {
                        vertices.Add(b.rot * type.voxelVerts[type.voxelTris[i+j]] + b.pos);
                        uvs.Add(type.uvs[type.voxelTris[i+j]]);
                        infos.Add(info);
                        triangles.Add(vertexIndex);

                        vertexIndex ++;
                    }
                }
            }
        }
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.uv2 = infos.ToArray();

        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = meshFilter.mesh;
        if (!obiCollider) obiCollider = chunkObject.AddComponent<ObiCollider>();
        obiCollider.transform.hasChanged = true;

        isDirty = false;
    }

    void ClearRender()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        infos.Clear();
    }

    public void Rerender() {
        ClearRender();
        new Thread(new ThreadStart(() => {
            LoadBlockData();
            isDirty = true;
        })).Start();
    }
}
