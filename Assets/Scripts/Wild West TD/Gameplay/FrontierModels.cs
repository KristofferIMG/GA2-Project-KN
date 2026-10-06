using UnityEngine;
using System.Collections.Generic;

// Source shapes for enemy models and one-time tower authoring. Live towers use saved mesh prefabs.
namespace WildWestTD
{
    public static class FrontierModels
    {
        static readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
        public static Material Mat(Color c)
        {
            Material m;
            if (materials.TryGetValue(c, out m) && m)
            {
                return m;
            }

            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color = c;
            m.SetFloat("_Smoothness", .12f);
            if (c.a < .99f)
            {
                m.SetFloat("_Surface", 1);
                m.SetFloat("_SrcBlend", 5);
                m.SetFloat("_DstBlend", 10);
                m.SetFloat("_ZWrite", 0);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = 3000;
            }

            materials[c] = m;
            return m;
        }

        static Color brown = new Color(.28f, .13f, .065f);
        static Color skin = new Color(.73f, .45f, .26f);
        static Color iron = new Color(.13f, .18f, .2f);
        static Color gold = new Color(.94f, .65f, .19f);
        static GameObject Part(Transform p, string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color c)
        {
            var g = GameObject.CreatePrimitive(type);
            g.name = name;
            g.transform.SetParent(p, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            if (Application.isPlaying)
            {
                Object.Destroy(g.GetComponent<Collider>());
            }
            else
            {
                Object.DestroyImmediate(g.GetComponent<Collider>());
            }

            g.GetComponent<Renderer>().sharedMaterial = Mat(c);
            return g;
        }

        static void Box(Transform p, string n, Vector3 a, Vector3 b, Color c)
        {
            Part(p, n, PrimitiveType.Cube, a, b, c);
        }

        static void Limb(Transform p, string n, Vector3 a, Vector3 b, float width, Color c)
        {
            var g = Part(p, n, PrimitiveType.Cylinder, (a + b) / 2, new Vector3(width, Vector3.Distance(a, b) / 2, width), c);
            g.transform.up = p.TransformDirection(b - a);
        }

        public static Transform Make(Transform parent, int kind, bool tower, bool wanted = false)
        {
            var root = new GameObject(tower ? FrontierRules.TowerNames[kind] : kind == 0 ? "Bandit" : kind == 1 ? "Roadrunner" : "Ironhide").transform;
            root.SetParent(parent, false);
            if (tower && kind == 4)
            {
                MakeBank(root);
                return root;
            }

            Color coat = tower ? (kind == 0 ? new Color(.20f, .49f, .67f) : kind == 1 ? new Color(.27f, .40f, .23f) : kind == 2 ? new Color(.78f, .35f, .12f) : new Color(.36f, .29f, .61f)) : (kind == 0 ? new Color(.50f, .23f, .15f) : kind == 1 ? new Color(.16f, .55f, .48f) : new Color(.32f, .36f, .40f));
            if (tower && kind >= 5)
            {
                coat = kind == 5 ? new Color(.32f, .38f, .43f) : kind == 6 ? new Color(.72f, .58f, .30f) : new Color(.18f, .20f, .24f);
            }

            if (tower)
            {
                Part(root, "Territory plinth", PrimitiveType.Cylinder, new Vector3(0, .045f, 0), new Vector3(.67f, .045f, .67f), coat);
                Box(root, "Front brass marker", new Vector3(0, .10f, -.27f), new Vector3(.21f, .04f, .05f), gold);
            }

            var body = new GameObject("Body").transform;
            body.SetParent(root, false);
            float bulk = !tower && kind == 2 ? 1.27f : 1;
            body.localScale = Vector3.one * bulk;
            for (int side = -1; side <= 1; side += 2)
            {
                var pivot = new GameObject(side < 0 ? "Left leg" : "Right leg").transform;
                pivot.SetParent(body, false);
                pivot.localPosition = new Vector3(side * .105f, .39f, 0);
                Limb(pivot, "Trouser", Vector3.zero, new Vector3(0, -.22f, 0), .13f, iron);
                Box(pivot, "Knee", new Vector3(0, -.17f, -.01f), new Vector3(.13f, .12f, .14f), iron);
                Box(pivot, "Boot", new Vector3(0, -.27f, -.045f), new Vector3(.145f, .16f, .24f), brown);
            }

            Part(body, "Waistcoat", PrimitiveType.Capsule, new Vector3(0, .52f, 0), new Vector3(.40f, .235f, .27f), coat);
            Box(body, "Leather belt", new Vector3(0, .39f, 0), new Vector3(.40f, .055f, .28f), brown);
            Box(body, "Buckle", new Vector3(0, .39f, -.146f), new Vector3(.075f, .055f, .026f), gold);
            Part(body, "Head", PrimitiveType.Sphere, new Vector3(0, .82f, 0), new Vector3(.29f, .32f, .27f), skin);
            Part(body, "Neck", PrimitiveType.Cylinder, new Vector3(0, .685f, 0), new Vector3(.13f, .055f, .13f), skin);
            Box(body, "Nose", new Vector3(0, .82f, -.14f), new Vector3(.06f, .075f, .065f), skin);
            for (int s = -1; s <= 1; s += 2)
            {
                Box(body, "Eye", new Vector3(s * .065f, .855f, -.125f), new Vector3(.03f, .03f, .025f), iron);
            }

            Color hat = tower && kind == 3 ? new Color(.79f, .69f, .45f) : brown;
            Part(body, "Hat brim", PrimitiveType.Cylinder, new Vector3(0, .955f, 0), new Vector3(.49f, .026f, .42f), hat);
            Part(body, "Hat crown", PrimitiveType.Cylinder, new Vector3(0, 1.035f, 0), new Vector3(.28f, .066f, .25f), hat);
            Part(body, "Hat band", PrimitiveType.Cylinder, new Vector3(0, .997f, 0), new Vector3(.29f, .018f, .26f), wanted ? Color.red : iron);
            Box(body, "Bandana", new Vector3(0, .727f, -.095f), new Vector3(.27f, .06f, .13f), wanted ? new Color(.86f, .07f, .07f) : tower ? gold : new Color(.45f, .08f, .065f));
            for (int s = -1; s <= 1; s += 2)
            {
                Limb(body, "Upper sleeve", new Vector3(s * .18f, .64f, 0), new Vector3(s * .25f, .49f, -.045f), .13f, coat);
                Limb(body, "Forearm", new Vector3(s * .25f, .49f, -.045f), new Vector3(s * .19f, .52f, -.22f), .11f, coat);
                Part(body, "Hand", PrimitiveType.Sphere, new Vector3(s * .19f, .52f, -.23f), Vector3.one * .115f, skin);
            }

            if (tower)
            {
                if (kind < 2)
                {
                    float length = kind == 1 ? .57f : .24f;
                    Box(body, "Gun stock", new Vector3(.17f, .53f, -.25f), new Vector3(.075f, .11f, .14f), brown);
                    Box(body, "Barrel", new Vector3(.17f, .56f, -.30f - length / 2), new Vector3(.055f, .06f, length), iron);
                    if (kind == 1)
                    {
                        Box(body, "Rifle scope", new Vector3(.17f, .625f, -.44f), new Vector3(.07f, .055f, .21f), iron);
                    }
                }
                else if (kind == 2)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Part(body, "Dynamite stick", PrimitiveType.Cylinder, new Vector3(.16f + (i - 1) * .055f, .54f, -.29f), new Vector3(.047f, .105f, .047f), new Color(.82f, .13f, .07f));
                    }

                    Box(body, "Supply satchel", new Vector3(-.18f, .45f, .1f), new Vector3(.19f, .23f, .15f), brown);
                }
                else if (kind == 3)
                {
                    Box(body, "Sheriff badge", new Vector3(-.10f, .61f, -.14f), new Vector3(.09f, .09f, .02f), gold);
                    var rope = new GameObject("Coiled lasso").AddComponent<LineRenderer>();
                    rope.transform.SetParent(body, false);
                    rope.useWorldSpace = false;
                    rope.sharedMaterial = Mat(gold);
                    rope.widthMultiplier = .022f;
                    rope.positionCount = 33;
                    for (int i = 0; i <= 32; i++)
                    {
                        float a = i * Mathf.PI / 16;
                        rope.SetPosition(i, new Vector3(.22f + Mathf.Cos(a) * .13f, .57f + Mathf.Sin(a) * .13f, -.28f));
                    }
                }
            }
            else if (kind == 2)
            {
                Box(body, "Iron breastplate", new Vector3(0, .54f, -.14f), new Vector3(.34f, .29f, .055f), new Color(.48f, .51f, .49f));
                for (int s = -1; s <= 1; s += 2)
                {
                    Box(body, "Shoulder armour", new Vector3(s * .225f, .65f, 0), new Vector3(.18f, .13f, .29f), iron);
                }
            }
            else if (kind == 1)
            {
                Box(body, "Runner scarf", new Vector3(.05f, .71f, .20f), new Vector3(.08f, .06f, .34f), new Color(.93f, .68f, .18f));
                body.localScale = new Vector3(.85f, 1, .85f);
            }

            if (tower && kind == 5)
            {
                Box(body, "Gatling receiver", new Vector3(0, .54f, -.34f), new Vector3(.43f, .19f, .29f), iron);
                var rotor = new GameObject("Barrel rotor").transform;
                rotor.SetParent(body, false);
                rotor.localPosition = new Vector3(0, .57f, -.48f);
                for (int i = 0; i < 6; i++)
                {
                    float a = i * Mathf.PI / 3;
                    Limb(rotor, "Rotating barrel", new Vector3(Mathf.Cos(a) * .075f, Mathf.Sin(a) * .075f, 0), new Vector3(Mathf.Cos(a) * .075f, Mathf.Sin(a) * .075f, -.36f), .044f, iron);
                }

                for (int j = 0; j < 2; j++)
                {
                    var band = Part(rotor, "Barrel collar", PrimitiveType.Cylinder, new Vector3(0, 0, -.07f - j * .22f), new Vector3(.21f, .025f, .21f), gold);
                    band.transform.localRotation = Quaternion.Euler(90, 0, 0);
                }

                Part(body, "Brass ammunition drum", PrimitiveType.Cylinder, new Vector3(.29f, .56f, -.32f), new Vector3(.22f, .14f, .22f), gold);
                Limb(body, "Gun support", new Vector3(0, .12f, -.38f), new Vector3(0, .48f, -.38f), .10f, iron);
            }

            if (tower && kind == 6)
            {
                Box(body, "Sheriff star", new Vector3(-.10f, .61f, -.14f), new Vector3(.10f, .10f, .025f), gold);
                Box(body, "Shotgun stock", new Vector3(.13f, .52f, -.26f), new Vector3(.13f, .12f, .21f), brown);
                for (int i = 0; i < 2; i++)
                {
                    Limb(body, "Double shotgun barrel", new Vector3(.09f + i * .07f, .55f, -.30f), new Vector3(.09f + i * .07f, .55f, -.66f), .06f, iron);
                }

                Box(body, "Shell belt", new Vector3(-.05f, .46f, -.145f), new Vector3(.16f, .08f, .04f), gold);
            }

            if (tower && kind == 7)
            {
                Box(body, "Duster back", new Vector3(0, .38f, .12f), new Vector3(.40f, .32f, .065f), coat);
                Box(body, "Silver badge", new Vector3(-.10f, .61f, -.15f), new Vector3(.085f, .10f, .025f), new Color(.72f, .78f, .77f));
                Box(body, "Bounty rifle stock", new Vector3(.17f, .53f, -.26f), new Vector3(.09f, .11f, .20f), brown);
                Limb(body, "Bounty rifle barrel", new Vector3(.17f, .56f, -.31f), new Vector3(.17f, .56f, -.77f), .055f, iron);
                Box(body, "Leather bounty ledger", new Vector3(-.22f, .43f, .05f), new Vector3(.09f, .23f, .20f), brown);
            }

            return root;
        }

        public static void MakeBank(Transform root)
        {
            Color wall = new Color(.68f, .48f, .24f);
            Box(root, "Bank foundation", new Vector3(0, .055f, 0), new Vector3(.78f, .11f, .72f), brown);
            Box(root, "Bank timber body", new Vector3(0, .33f, 0), new Vector3(.62f, .46f, .57f), wall);
            Box(root, "False front", new Vector3(0, .58f, -.27f), new Vector3(.72f, .19f, .065f), brown);
            Box(root, "Roof cornice", new Vector3(0, .70f, -.25f), new Vector3(.80f, .06f, .15f), gold);
            Box(root, "Roof", new Vector3(0, .57f, .03f), new Vector3(.70f, .06f, .63f), brown);
            Box(root, "Vault door", new Vector3(0, .28f, -.295f), new Vector3(.21f, .33f, .035f), iron);
            for (int side = -1; side <= 1; side += 2)
            {
                Box(root, "Window frame", new Vector3(side * .215f, .35f, -.298f), new Vector3(.15f, .22f, .04f), brown);
                Box(root, "Blue glass", new Vector3(side * .215f, .35f, -.323f), new Vector3(.105f, .17f, .012f), new Color(.23f, .48f, .49f));
                Box(root, "Window bar", new Vector3(side * .215f, .35f, -.334f), new Vector3(.012f, .17f, .014f), gold);
            }

            var coin = Part(root, "Gold bank seal", PrimitiveType.Cylinder, new Vector3(0, .59f, -.314f), new Vector3(.12f, .015f, .12f), gold);
            coin.transform.localRotation = Quaternion.Euler(90, 0, 0);
            for (int i = 0; i < 3; i++)
            {
                Part(root, "Coin stack", PrimitiveType.Cylinder, new Vector3(.25f, .13f + i * .03f, -.40f), new Vector3(.10f, .018f, .10f), gold);
            }
        }

        public static void AnimateTower(Transform root, float spin, float dt)
        {
            var rotor = root.Find("Body/Barrel rotor");
            if (rotor)
            {
                rotor.Rotate(0, 0, spin * 900 * dt, Space.Self);
            }
        }

        public static void Tint(Transform root, Color c)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>())
            {
                var slots = r.sharedMaterials;
                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = Mat(c);
                }

                r.sharedMaterials = slots;
            }
        }

        public static void Animate(Transform root, float clock, float speed)
        {
            var body = root.Find("Body");
            if (!body)
            {
                return;
            }

            var left = body.Find("Left leg");
            var right = body.Find("Right leg");
            float a = Mathf.Sin(clock * speed * 9) * 23;
            left.localRotation = Quaternion.Euler(a, 0, 0);
            right.localRotation = Quaternion.Euler(-a, 0, 0);
        }
    }
}

