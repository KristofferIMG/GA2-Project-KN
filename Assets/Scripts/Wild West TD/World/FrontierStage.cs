using UnityEngine;

// Builds the tabletop battle stage and its desert backdrop.
namespace WildWestTD
{
    // A temporary battlefield diorama, separated from the explorable saloon.
    public static class FrontierStage
    {
        static Transform Piece(Transform root, string name, PrimitiveType kind, Vector3 pos, Vector3 scale, Material material)
        {
            var g = GameObject.CreatePrimitive(kind);
            g.name = name;
            string category = name == "Trail" || name == "Terrain tile" ? "Board tiles" : name.Contains("board") || name.Contains("foundation") ? "Board frame" : "Desert scenery";
            var group = root.Find(category);
            if (!group)
            {
                group = new GameObject(category).transform;
                group.SetParent(root, false);
            }

            g.transform.SetParent(group, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = material;
            Object.Destroy(g.GetComponent<Collider>());
            return g.transform;
        }

        public static Transform Create(SaloonMaps maps)
        {
            int id = maps.activeMap;
            var root = new GameObject("Western battlefield diorama").transform;
            root.position = new Vector3(150, 0, 0);
            var sand = FrontierModels.Mat(new Color(.61f, .37f, .20f));
            var rock = FrontierModels.Mat(new Color(.42f, .19f, .105f));
            var cap = FrontierModels.Mat(new Color(.69f, .40f, .21f));
            var wood = FrontierModels.Mat(new Color(.20f, .095f, .045f));
            var green = FrontierModels.Mat(new Color(.24f, .37f, .16f));
            Piece(root, "Desert surround", PrimitiveType.Cube, new Vector3(0, 1.23f, 0), new Vector3(12, .12f, 12), sand);
            Piece(root, "Board foundation", PrimitiveType.Cube, new Vector3(0, 1.28f, 0), new Vector3(1.70f, .085f, 1.70f), wood);
            for (int s = -1; s <= 1; s += 2)
            {
                Piece(root, "Timber board frame", PrimitiveType.Cube, new Vector3(s * .84f, 1.345f, 0), new Vector3(.042f, .07f, 1.72f), wood);
                Piece(root, "Timber board frame", PrimitiveType.Cube, new Vector3(0, 1.345f, s * .84f), new Vector3(1.72f, .07f, .042f), wood);
            }

            var path = SaloonMaps.Path(id);
            var heights = SaloonMaps.Heights(id);
            for (int x = 0; x < 20; x++)
            {
                for (int z = 0; z < 20; z++)
                {
                    int h = heights[x, z];
                    int color = path[x, z] ? 3 : h;
                    if (SaloonMaps.IsRiver(id, x, z) && !path[x, z])
                    {
                        color = 4;
                    }

                    var material = SaloonMaps.IsBridge(id, x, z) && maps.bridgeMaterial ? maps.bridgeMaterial : maps.palette[id * 5 + color];
                    Piece(root, path[x, z] ? "Trail" : "Terrain tile", PrimitiveType.Cube, new Vector3((x - 9.5f) * .079f, 1.325f + h * .035f, (z - 9.5f) * .079f), new Vector3(.076f, .025f + h * .07f, .076f), material);
                }
            }

            // Sparse scenery outside the playable grid keeps placements and routes readable.
            for (int i = 0; i < 7; i++)
            {
                float x = (i - 3) * .62f;
                float depth = 2.2f + (i % 2) * .2f;
                float height = .42f + (i % 3) * .19f;
                Piece(root, "Distant sandstone mesa", PrimitiveType.Cube, new Vector3(x, 1.29f + height / 2, depth), new Vector3(.66f, height, .64f), rock);
                Piece(root, "Mesa cap", PrimitiveType.Cube, new Vector3(x, 1.29f + height, depth), new Vector3(.70f, .07f, .68f), cap);
            }

            for (int i = 0; i < 6; i++)
            {
                float side = i % 2 == 0 ? -1 : 1;
                float x = side * (1.15f + (i % 3) * .17f), z = -.8f + i * .40f;
                Piece(root, "Saguaro trunk", PrimitiveType.Cylinder, new Vector3(x, 1.42f, z), new Vector3(.05f, .13f, .05f), green);
                Piece(root, "Saguaro arm", PrimitiveType.Cube, new Vector3(x + .055f, 1.44f, z), new Vector3(.10f, .035f, .04f), green);
                Piece(root, "Saguaro tip", PrimitiveType.Cylinder, new Vector3(x + .10f, 1.485f, z), new Vector3(.037f, .058f, .037f), green);
                for (int j = 0; j < 2; j++)
                {
                    Piece(root, "Desert stones", PrimitiveType.Sphere, new Vector3(x - .1f - j * .07f, 1.31f, z + .15f), new Vector3(.1f, .06f, .075f), cap);
                }
            }

            for (int i = 0; i < 15; i++)
            {
                Piece(root, "Fence post", PrimitiveType.Cube, new Vector3((i - 7) * .18f, 1.40f, 1.30f), new Vector3(.025f, .22f, .025f), wood);
            }

            for (int j = 0; j < 2; j++)
            {
                Piece(root, "Fence rail", PrimitiveType.Cube, new Vector3(0, 1.38f + j * .08f, 1.30f), new Vector3(2.6f, .018f, .02f), wood);
            }

            var light = new GameObject("Battlefield sunlight").AddComponent<Light>();
            light.transform.SetParent(root, false);
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1, .89f, .72f);
            light.transform.rotation = Quaternion.Euler(55, -35, 0);
            light.shadows = LightShadows.Soft;
            return root;
        }
    }
}

