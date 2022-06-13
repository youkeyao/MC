using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInfo
{
    public string preview;
    public string[] mats;
    public int[] texIDs;
    public int type;
    
    public BlockInfo(int type, string preview, string[] mats, int[] texIDs = null)
    {
        this.type = type;
        this.preview = preview;
        this.mats = mats;
        this.texIDs = texIDs;
    }
}

public class BlockType
{
    public List<Vector3> voxelVerts;
    public List<Vector2> uvs;
    public List<int> voxelTris;
    public List<Vector3> centerDistances = new List<Vector3> ();
    public Vector3 AA;
    public Vector3 BB;

    public BlockType(List<Vector3> voxelVerts, List<Vector2> uvs, List<int> voxelTris)
    {
        this.voxelVerts = voxelVerts;
        this.uvs = uvs;
        this.voxelTris = voxelTris;

        AA = BB = voxelVerts[0];
        for (int i = 1; i < voxelVerts.Count; i ++) {
            AA.x = Mathf.Min(AA.x, voxelVerts[i].x);
            AA.y = Mathf.Min(AA.y, voxelVerts[i].y);
            AA.z = Mathf.Min(AA.z, voxelVerts[i].z);
            BB.x = Mathf.Max(BB.x, voxelVerts[i].x);
            BB.y = Mathf.Max(BB.y, voxelVerts[i].y);
            BB.z = Mathf.Max(BB.z, voxelVerts[i].z);
        }
        for (int i = 0; i < voxelTris.Count; i += 3) {
            centerDistances.Add((voxelVerts[voxelTris[i]] + voxelVerts[voxelTris[i+1]] + voxelVerts[voxelTris[i+2]]) / 3);
        }
    }

    public static Vector3 GetAABBDistance(BlockType type, Vector3 normal, Quaternion rot)
    {
        normal = Matrix4x4.Rotate(rot).inverse * normal;
        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y) && Mathf.Abs(normal.x) > Mathf.Abs(normal.z)) {
            if (normal.x > 0) {
                return rot * new Vector3(type.BB.x, 0, 0);
            }
            else {
                return rot * new Vector3(type.AA.x, 0, 0);
            }
        }
        else if (Mathf.Abs(normal.y) > Mathf.Abs(normal.z)) {
            if (normal.y > 0) {
                return rot * new Vector3(0, type.BB.y, 0);
            }
            else {
                return rot * new Vector3(0, type.AA.y, 0);
            }
        }
        else {
            if (normal.z > 0) {
                return rot * new Vector3(0, 0, type.BB.z);
            }
            else {
                return rot * new Vector3(0, 0, type.AA.z);
            }
        }
    }
}

public static class BlockData
{
    public static List<BlockType> blockTypes = new List<BlockType> {
        new BlockType(
            new List<Vector3> {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),

                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),

                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),

                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),

                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),

                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
            },
            new List<Vector2> {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
            },
            new List<int> {
                0, 3, 1,
                2, 1, 3,
                5, 6, 4,
                7, 4, 6,

                8, 11, 9,
                10, 9, 11,
                13, 14, 12,
                15, 12, 14,
                
                16, 19, 17,
                18, 17, 19,
                21, 22, 20,
                23, 20, 22,
            }
        ),
        new BlockType(
            new List<Vector3> {
                new Vector3(-0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, 0.25f, -0.5f),
                new Vector3(-0.5f, 0.25f, -0.5f),

                new Vector3(-0.5f, -0.25f, 0.5f),
                new Vector3(0.5f, -0.25f, 0.5f),
                new Vector3(0.5f, 0.25f, 0.5f),
                new Vector3(-0.5f, 0.25f, 0.5f),

                new Vector3(-0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, 0.25f, 0.5f),
                new Vector3(-0.5f, 0.25f, 0.5f),

                new Vector3(-0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, -0.25f, 0.5f),
                new Vector3(-0.5f, -0.25f, 0.5f),

                new Vector3(-0.5f, -0.25f, 0.5f),
                new Vector3(-0.5f, -0.25f, -0.5f),
                new Vector3(-0.5f, 0.25f, -0.5f),
                new Vector3(-0.5f, 0.25f, 0.5f),

                new Vector3(0.5f, -0.25f, 0.5f),
                new Vector3(0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, 0.25f, 0.5f),
            },
            new List<Vector2> {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
            },
            new List<int> {
                0, 3, 1,
                2, 1, 3,
                5, 6, 4,
                7, 4, 6,

                8, 11, 9,
                10, 9, 11,
                13, 14, 12,
                15, 12, 14,
                
                16, 19, 17,
                18, 17, 19,
                21, 22, 20,
                23, 20, 22,
            }
        ),
        new BlockType(
            new List<Vector3> {
                new Vector3(-0.25f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.25f, 0.5f, -0.5f),

                new Vector3(-0.25f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.25f, 0.5f, 0.5f),

                new Vector3(-0.25f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.25f, 0.5f, 0.5f),

                new Vector3(-0.25f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.25f, -0.5f, 0.5f),

                new Vector3(-0.25f, -0.5f, 0.5f),
                new Vector3(-0.25f, -0.5f, -0.5f),
                new Vector3(-0.25f, 0.5f, -0.5f),
                new Vector3(-0.25f, 0.5f, 0.5f),

                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
            },
            new List<Vector2> {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),

                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),

                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
            },
            new List<int> {
                0, 3, 1,
                2, 1, 3,
                5, 6, 4,
                7, 4, 6,

                8, 11, 9,
                10, 9, 11,
                13, 14, 12,
                15, 12, 14,
                
                16, 19, 17,
                18, 17, 19,
                21, 22, 20,
                23, 20, 22,
            }
        ),
    };

    public static List<BlockInfo> allblocks = new List<BlockInfo> {
        new BlockInfo(0, "preview/preview_dirt", new string[] {
            "block/dirt"
        }),
        new BlockInfo(0, "preview/preview_grass_block", new string[] {
            "block/grass_block_side",
            "block/grass_block_top",
            "block/dirt",
        }, new int[] {
            0, 0, 0, 0, 1, 1, 2, 2, 0, 0, 0, 0
        }),
        new BlockInfo(0, "preview/preview_stone", new string[] {
            "block/stone"
        }),
        new BlockInfo(0, "preview/preview_andesite", new string[] {
            "block/andesite"
        }),
        new BlockInfo(0, "preview/preview_cobblestone", new string[] {
            "block/cobblestone"
        }),
        new BlockInfo(1, "preview/preview_half_cobblestone", new string[] {
            "block/cobblestone"
        }),
        new BlockInfo(0, "preview/preview_acacia_leaves", new string[] {
            "block/acacia_leaves"
        }),
        new BlockInfo(0, "preview/preview_birch_planks", new string[] {
            "block/birch_planks"
        }),
        new BlockInfo(0, "preview/preview_birch_log", new string[] {
            "block/birch_log",
            "block/birch_log_top",
        }, new int[] {
            0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0
        }),
        new BlockInfo(0, "preview/preview_pumpkin", new string[] {
            "block/pumpkin_side",
            "block/pumpkin_top",
        }, new int[] {
            0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0
        }),
        new BlockInfo(0, "preview/preview_gold_ore", new string[] {
            "block/gold_ore"
        }),
        new BlockInfo(0, "preview/preview_gold_block", new string[] {
            "block/gold_block_end"
        }),
        new BlockInfo(0, "preview/preview_furnace", new string[] {
            "block/furnace_front",
            "block/furnace_side",
            "block/furnace_top",
        }, new int[] {
            0, 0, 1, 1, 2, 2, 2, 2, 1, 1, 1, 1
        }),
        new BlockInfo(0, "preview/preview_furnace_on", new string[] {
            "block/furnace_front_on",
            "block/furnace_side",
            "block/furnace_top",
        }, new int[] {
            0, 0, 1, 1, 2, 2, 2, 2, 1, 1, 1, 1
        }),
    };

    // Material index: {front, back, top, bottom, left, right}
    public static readonly List<int[]> blockMat = new List<int[]> {
        new int[] {0, 0, 0, 0, 0, 0},
        new int[] {1, 1, 2, 0, 1, 1},
        new int[] {3, 3, 3, 3, 3, 3},
        new int[] {4, 4, 4, 4, 4, 4},
        new int[] {5, 5, 5, 5, 5, 5},
        new int[] {6, 6, 6, 6, 6, 6},
        new int[] {7, 7, 7, 7, 7, 7},
        new int[] {8, 8, 9, 9, 8, 8},
        new int[] {10, 10, 11, 11, 10, 10},
        new int[] {12, 12, 12, 12, 12, 12},
        new int[] {13, 13, 13, 13, 13, 13},
        new int[] {14, 16, 17, 17, 16, 16},
        new int[] {15, 16, 17, 17, 16, 16},
        new int[] {7, 7, 7, 7, 7, 7},
        new int[] {6, 6, 6, 6, 6, 6}
    };
}
