using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    int id;
    bool isFollow = false;
    float speed = 0;
    float maxSpeed = 6.0f;
    Transform player;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Character") {
            player = other.gameObject.transform;
            Destroy(this.GetComponent<SphereCollider>());
            this.GetComponent<Rigidbody>().useGravity = false;
            isFollow = true;
        }
    }

    void Update()
    {
        if (isFollow) {
            speed += 0.2f * Time.deltaTime;
            if (speed > maxSpeed) speed = maxSpeed;
            Vector3 distance = player.position - this.transform.position + Vector3.up;
            if (distance.magnitude < 0.5f) {
                Destroy(this.gameObject);
            }
            else {
                this.transform.position += speed * distance.normalized;
            }
        }
    }

    public void SetItem(int id, World world)
    {
        this.id = id;
        // change item mesh
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3> ();
        List<int> triangles = new List<int> ();
        List<Vector2> uvs = new List<Vector2> ();
        List<Vector2> infos = new List<Vector2> ();
        BlockType type = BlockData.blockTypes[BlockData.allBlocks[id].type];
        for (int i = 0; i < type.voxelTris.Count; i += 3) {
            int faceIndex = i / 3;
            // encode uv (-1, texID)
            int texID = BlockData.allBlocks[id].texIDs != null && BlockData.allBlocks[id].texIDs.Length > faceIndex ? BlockData.allBlocks[id].texIDs[faceIndex] : 0;
            texID = world.texturesID[BlockData.allBlocks[id].mats[texID]];
            Vector2 info = new Vector2(-1, texID);
            for (int j = 0; j < 3; j ++) {
                vertices.Add(type.voxelVerts[type.voxelTris[i+j]]);
                uvs.Add(type.uvs[type.voxelTris[i+j]]);
                infos.Add(info);
                triangles.Add(i+j);
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.uv2 = infos.ToArray();
        mesh.SetTriangles(triangles, 0);
        this.GetComponentsInChildren<MeshFilter>()[1].mesh = mesh;
        this.GetComponentsInChildren<Renderer>()[1].material = world.myMaterial;
    }
}
