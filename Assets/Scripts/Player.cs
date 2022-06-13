using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float speed = 6.0f;
    public float turnSpeed = 1.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float putTurnSpeed = 5.0f;
    public GameObject selectBlock;
    public GameObject putBlock;
    public GameObject dropItem;
    public World world;
    public GameObject shortcut;

    float xRotation = 0.0f;
    CharacterController controller;
    Vector3 moveDirection = Vector3.zero;
    bool isRemove = false;
    bool isPut = false;
    bool canPut = true;
    bool canRemove = true;
    RaycastHit hit;
    int blockIndex;
    int putStatus = 0;
    int rotateAxis = 0;
    Block selectedBlock;
 
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        controller = GetComponent<CharacterController>();
        blockIndex = -1;
        shortcut.SetActive(false);
    }

    void Update()
    {
        if (!world.isOpenInventory && !world.isPause) {
            // view
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            transform.Rotate(Vector3.up, mouseX * turnSpeed);
            xRotation -= mouseY * turnSpeed;
            xRotation = Mathf.Clamp(xRotation, -85, 85f);
            Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

            // move
            if (controller.isGrounded) {
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection *= speed;
                if (Input.GetButton("Jump"))
                    moveDirection.y = jumpSpeed;
            }
            moveDirection.y -= gravity * Time.deltaTime;
            controller.Move(moveDirection * Time.deltaTime);

            // select
            SelectHint();

            // action
            // put block
            if (Input.GetAxis("Put") != 0.0f) {
                if (!isPut) {
                    isPut = true;
                    if (canPut) {
                        // put rotx
                        if (putStatus == 0) {
                            putStatus = 1;
                        }
                        // put roty
                        else if (putStatus == 1) {
                            putStatus = 2;
                        }
                        else {
                            putStatus = 0;
                            PutBlock();
                            rotateAxis = 0;
                        }
                    }
                }
            }
            else isPut = false;
            // remove block
            if (Input.GetAxis("Remove") != 0.0f) {
                if (!isRemove) {
                    isRemove = true;
                    // remove
                    if (blockIndex == -1 && canRemove) {
                        RemoveBlock();
                    }
                    // cancel put
                    else {
                        shortcut.SetActive(false);
                        blockIndex = -1;
                        putStatus = 0;
                        rotateAxis = 0;
                    }
                }
            }
            else isRemove = false;
            // choose block shortcut
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (putStatus == 0 && scroll != 0.0f) {
                if (scroll > 0) {
                    blockIndex = (blockIndex + 8) % 9;
                }
                else if (scroll < 0) {
                    blockIndex = (blockIndex + 1) % 9;
                }
                UpdateShortCutHighlight();
            }
            // translate putBlock
            else if (putStatus == 1) {
                putBlock.transform.position += scroll * hit.normal;
            }
            // rotate putBlock
            else if (putStatus == 2) {
                if (scroll > 0) {
                    rotateAxis = (rotateAxis + 2) % 3;
                }
                else if (scroll < 0) {
                    rotateAxis = (rotateAxis + 1) % 3;
                }
                if (Input.GetAxis("OrderPut") != 0.0f) {
                    putBlock.transform.rotation = selectBlock.transform.rotation;
                }
                else {
                    if (rotateAxis == 0) {
                        putBlock.transform.Rotate(-mouseX * putTurnSpeed, 0, 0);
                    }
                    else if (rotateAxis == 1) {
                        putBlock.transform.Rotate(0, -mouseX * putTurnSpeed, 0);
                    }
                    else if (rotateAxis == 2) {
                        putBlock.transform.Rotate(0, 0, -mouseX * putTurnSpeed);
                    }
                }
            }
        }
    }

    void UpdateShortCutHighlight()
    {
        shortcut.SetActive(true);
        Vector3 tmp = shortcut.GetComponent<RectTransform>().anchoredPosition;
        tmp.x = -321 + blockIndex * 80;
        shortcut.GetComponent<RectTransform>().anchoredPosition = tmp;
    }

    void PutBlock()
    {
        Vector3 pos = putBlock.transform.position;
        Quaternion rot = putBlock.transform.rotation;
        Chunk now = world.chunks[world.GetChunkID(pos)];
        now.blocks.Add(pos.ToString(), new Block(pos, rot, world.inventory.GetComponent<HandleInventory>().shortcutList[blockIndex]));
        now.Rerender();
        blockIndex = -1;
        shortcut.SetActive(false);
    }

    void RemoveBlock()
    {
        Vector3 selectBlockPos = selectBlock.transform.position;
        Chunk now = world.chunks[world.GetChunkID(selectBlockPos)];
        now.blocks.Remove(selectBlockPos.ToString());
        now.Rerender();

        GameObject drop = GameObject.Instantiate(dropItem, selectBlock.transform.position, selectBlock.transform.rotation);
        drop.GetComponent<DropItem>().SetItem(selectedBlock.id, world);
        drop.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(0, 10), 20, Random.Range(0, 10)).normalized);
    }

    // put selectBlock and putBlock
    void SelectHint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (putStatus == 0) {
            if (Physics.Raycast(ray, out hit, 10f)) {
                // hit triangles
                MeshCollider collider = hit.collider as MeshCollider;
                Mesh mesh = collider.sharedMesh;
                Vector3 p0 = mesh.vertices[mesh.triangles[hit.triangleIndex * 3]];
                Vector3 p1 = mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 1]];
                Vector3 p2 = mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 2]];

                // get info
                int selectIndex = (int)((Mathf.Round(hit.textureCoord2.x) / BlockData.blockTypes.Count));
                int typeIndex = (int)Mathf.Round(hit.textureCoord2.x - selectIndex * BlockData.blockTypes.Count);
                BlockType selectType = BlockData.blockTypes[typeIndex];
                Vector3 origin1 = selectType.voxelVerts[selectType.voxelTris[selectIndex * 3]];
                Vector3 origin2 = selectType.voxelVerts[selectType.voxelTris[selectIndex * 3 + 1]];
                Vector3 origin3 = selectType.voxelVerts[selectType.voxelTris[selectIndex * 3 + 2]];
                Vector3 center = (p0 + p1 + p2) / 3 - CalcTriRotation(origin1, origin2, origin3, p0, p1, p2) * selectType.centerDistances[selectIndex];
                Chunk now = world.chunks[world.GetChunkID(center)];

                // find block
                if (now.blocks.ContainsKey(center.ToString())) {
                    selectedBlock = now.blocks[center.ToString()];
                    selectBlock.transform.position = selectedBlock.pos;
                    selectBlock.transform.rotation = selectedBlock.rot;
                    selectBlock.GetComponent<MeshFilter>().mesh = world.meshes[typeIndex];
                    selectBlock.SetActive(true);
                    canRemove = true;
                }
                else {
                    canRemove = false;
                }

                // choose block
                if (blockIndex != -1) {
                    int putId = world.inventory.GetComponent<HandleInventory>().shortcutList[blockIndex];
                    BlockType putType = BlockData.blockTypes[BlockData.allblocks[putId].type];
                    Vector3 faceDistance = BlockType.GetAABBDistance(putType, -hit.normal, selectBlock.transform.rotation);
                    if (Input.GetAxis("OrderPut") != 0.0f) {
                        putBlock.transform.position = selectBlock.transform.position - faceDistance;
                        faceDistance = BlockType.GetAABBDistance(selectType, hit.normal, selectBlock.transform.rotation);
                        putBlock.transform.position += faceDistance;
                    }
                    else {
                        putBlock.transform.position = hit.point - faceDistance;
                    }
                    putBlock.transform.rotation = selectBlock.transform.rotation;
                    if (putBlock.GetComponent<TriggerStatus>().isTrigger) {
                        canPut = false;
                    }
                    else {
                        canPut = true;
                    }
                    putBlock.GetComponent<MeshFilter>().mesh = world.meshes[BlockData.allblocks[putId].type];
                    putBlock.GetComponent<MeshCollider>().sharedMesh = world.meshes[BlockData.allblocks[putId].type];
                    putBlock.SetActive(true);
                }
                else {
                    canPut = false;
                    putBlock.SetActive(false);
                }
            }
            else {
                canPut = false;
                canRemove = false;
                selectBlock.SetActive(false);
                putBlock.SetActive(false);
            }
        }
        else {
            canRemove = false;
            selectBlock.SetActive(false);
            if (putBlock.GetComponent<TriggerStatus>().isTrigger) {
                canPut = false;
            }
            else {
                canPut = true;
            }
        }
    }

    // calculate rotation from three points
    Quaternion CalcTriRotation(Vector3 o1, Vector3 o2, Vector3 o3, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Quaternion q1 = Quaternion.FromToRotation(o2 - o1, p2 - p1);
        Vector3 o4 = (o3 - o1) - Vector3.Project(o3 - o1, o2 - o1);
        Vector3 p4 = (p3 - p1) - Vector3.Project(p3 - p1, p2 - p1);
        Quaternion q2 = Quaternion.FromToRotation(q1 * o4, p4);
        return q2 * q1;
    }
}