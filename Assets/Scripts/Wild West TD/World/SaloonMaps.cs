using UnityEngine;
using System.Collections.Generic;

// Defines board layouts and builds their previews. Coordinates are tile positions, not world metres.
namespace WildWestTD
{
    public class SaloonMaps : MonoBehaviour
    {
        public Material bridgeMaterial;
        public Transform[] tables;
        public Material[] palette;
        public int activeMap;
        public static readonly string[] Names =
        {
            "Dusty Crossing",
            "Twin Mesa",
            "Canyon Pass",
            "Dry River Ford",
            "Iron Spur"
        };
        public static readonly string[] Descriptions =
        {
            "A winding frontier trail with high ground in the west.",
            "Two northern trails converge between twin mesas.",
            "A long switchback through a narrow red-rock canyon.",
            "Two routes meet beside a river with wooden bridge crossings.",
            "Two mining tracks join, then curve around the high ridge."
        };
        public static int SpawnCount(int id)
        {
            return id == 1 || id == 3 || id == 4 ? 2 : 1;
        }

        public static Vector2Int[][] Routes(int id)
        {
            if (id == 1)
            {
                return new[]
                {
                    new[]
                    {
                        new Vector2Int(3, 19),
                        new Vector2Int(3, 13),
                        new Vector2Int(10, 13),
                        new Vector2Int(10, 5),
                        new Vector2Int(7, 5),
                        new Vector2Int(7, 0)
                    },
                    new[]
                    {
                        new Vector2Int(17, 19),
                        new Vector2Int(17, 13),
                        new Vector2Int(10, 13)
                    }
                };
            }

            if (id == 2)
            {
                return new[]
                {
                    new[]
                    {
                        new Vector2Int(16, 19),
                        new Vector2Int(16, 16),
                        new Vector2Int(3, 16),
                        new Vector2Int(3, 10),
                        new Vector2Int(16, 10),
                        new Vector2Int(16, 4),
                        new Vector2Int(8, 4),
                        new Vector2Int(8, 0)
                    }
                };
            }

            if (id == 3)
            {
                return new[]
                {
                    new[]
                    {
                        new Vector2Int(0, 14),
                        new Vector2Int(10, 14),
                        new Vector2Int(10, 4),
                        new Vector2Int(6, 4),
                        new Vector2Int(6, 0)
                    },
                    new[]
                    {
                        new Vector2Int(19, 14),
                        new Vector2Int(10, 14)
                    }
                };
            }

            if (id == 4)
            {
                return new[]
                {
                    new[]
                    {
                        new Vector2Int(2, 19),
                        new Vector2Int(2, 9),
                        new Vector2Int(7, 9),
                        new Vector2Int(7, 4),
                        new Vector2Int(14, 4),
                        new Vector2Int(14, 0)
                    },
                    new[]
                    {
                        new Vector2Int(16, 19),
                        new Vector2Int(16, 13),
                        new Vector2Int(7, 13),
                        new Vector2Int(7, 9)
                    }
                };
            }

            return new[]
            {
                new[]
                {
                    new Vector2Int(9, 19),
                    new Vector2Int(9, 11),
                    new Vector2Int(13, 11),
                    new Vector2Int(13, 5),
                    new Vector2Int(6, 5),
                    new Vector2Int(6, 0)
                }
            };
        }

        public static bool[, ] Path(int id)
        {
            var p = new bool[20, 20];
            foreach (var branch in Routes(id))
            {
                for (int i = 1; i < branch.Length; i++)
                {
                    var a = branch[i - 1];
                    var b = branch[i];
                    int dx = System.Math.Sign(b.x - a.x), dy = System.Math.Sign(b.y - a.y);
                    p[a.x, a.y] = true;
                    while (a != b)
                    {
                        a += new Vector2Int(dx, dy);
                        p[a.x, a.y] = true;
                    }
                }
            }

            return p;
        }

        public static int[, ] Heights(int id)
        {
            var p = Path(id);
            var h = new int[20, 20];
            for (int x = 0; x < 20; x++)
            {
                for (int z = 0; z < 20; z++)
                {
                    int d = 30;
                    if (id == 0)
                    {
                        d = Mathf.Abs(x - 2) + Mathf.Abs(z - 15);
                    }

                    if (id == 1)
                    {
                        d = Mathf.Min(Mathf.Abs(x - 5) + Mathf.Abs(z - 7), Mathf.Abs(x - 15) + Mathf.Abs(z - 7));
                    }

                    if (id == 2)
                    {
                        d = Mathf.Min(Mathf.Abs(x - 1), Mathf.Abs(x - 19)) * 2 + Mathf.Abs(z - 10) / 3;
                    }

                    if (id == 3)
                    {
                        d = Mathf.Min(Mathf.Abs(x - 3) + Mathf.Abs(z - 5), Mathf.Abs(x - 16) + Mathf.Abs(z - 5));
                    }

                    if (id == 4)
                    {
                        d = Mathf.Min(Mathf.Abs(x - 11) + Mathf.Abs(z - 9), Mathf.Abs(x - 11) + Mathf.Abs(z - 16));
                    }

                    h[x, z] = (p[x, z] || IsRiver(id, x, z)) ? 0 : Mathf.Clamp(4 - d / 2, 0, 2);
                }
            }

            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int x = 0; x < 20; x++)
                {
                    for (int z = 0; z < 20; z++)
                    {
                        int a = h[x, z];
                        if (x > 0)
                        {
                            a = Mathf.Min(a, h[x - 1, z] + 1);
                        }

                        if (x < 19)
                        {
                            a = Mathf.Min(a, h[x + 1, z] + 1);
                        }

                        if (z > 0)
                        {
                            a = Mathf.Min(a, h[x, z - 1] + 1);
                        }

                        if (z < 19)
                        {
                            a = Mathf.Min(a, h[x, z + 1] + 1);
                        }

                        if (a != h[x, z])
                        {
                            h[x, z] = a;
                            changed = true;
                        }
                    }
                }
            }

            return h;
        }

        public static bool IsRiver(int id, int x, int z)
        {
            return id == 3 && x >= 8 && x <= 9 && z >= 0 && z < 20;
        }

        public static bool IsBridge(int id, int x, int z)
        {
            return IsRiver(id, x, z) && Path(id)[x, z];
        }

        public static bool CanPlaceTower(int id, int x, int z)
        {
            return id >= 0 && id < 5 && x >= 0 && x < 20 && z >= 0 && z < 20 && !IsRiver(id, x, z) && !Path(id)[x, z];
        }

        public static int HeightLevel(int id, int x, int z)
        {
            return Heights(id)[x, z] + 1;
        }

        public void ChangeMap(int delta)
        {
            ApplyMap((activeMap + delta + 5) % 5);
        }

        public void ApplyMap(int id)
        {
            activeMap = Mathf.Clamp(id, 0, 4);
            if (tables == null || palette == null || palette.Length < 25)
            {
                return;
            }

            var path = Path(activeMap);
            var height = Heights(activeMap);
            foreach (var table in tables)
            {
                if (!table)
                {
                    continue;
                }

                foreach (var t in table.GetComponentsInChildren<Transform>())
                {
                    if (t.name != "Terrain tile" && t.name != "Trail")
                    {
                        continue;
                    }

                    int x = Mathf.Clamp(Mathf.RoundToInt(t.localPosition.x / .079f + 9.5f), 0, 19), z = Mathf.Clamp(Mathf.RoundToInt(t.localPosition.z / .079f + 9.5f), 0, 19);
                    int h = height[x, z];
                    var pos = t.localPosition;
                    pos.y = 1.325f + h * .035f;
                    t.localPosition = pos;
                    t.localScale = new Vector3(.076f, .025f + h * .07f, .076f);
                    int color = path[x, z] ? 3 : h;
                    if (IsRiver(activeMap, x, z) && !path[x, z])
                    {
                        color = 4;
                    }

                    t.GetComponent<Renderer>().sharedMaterial = IsRiver(activeMap, x, z) && path[x, z] && bridgeMaterial ? bridgeMaterial : palette[activeMap * 5 + color];
                    t.name = path[x, z] ? "Trail" : "Terrain tile";
                }
            }
        }

        void Start()
        {
            ApplyMap(activeMap);
        }
    }
}

