using UnityEngine;
using System.Collections.Generic;

// Shows wave income and soft dynamite dust. Feedback never changes the amount of money awarded.
namespace WildWestTD
{
    public partial class FrontierGame
    {
        public Material dynamiteDustMaterial;
        class IncomePopup
        {
            public Vector3 position;
            public int amount;
            public float age;
            public UnityEngine.UI.Text label;
        }

        readonly List<IncomePopup> incomePopups = new List<IncomePopup>();
        public int ActiveIncomePopups
        {
            get
            {
                return incomePopups.Count;
            }
        }

        void ShowBankIncome()
        {
            foreach (var t in battle.towers)
            {
                if (t.type == 4)
                {
                    incomePopups.Add(new IncomePopup { position = World(t.position, .10f), amount = t.income, label = GetComponentInChildren<SaloonInterface>(true).CreateIncomeLabel(t.income) });
                }
            }
        }

        void ClearIncomePopups()
        {
            foreach (var popup in incomePopups)
            {
                if (popup.label)
                {
                    Destroy(popup.label.gameObject);
                }
            }

            incomePopups.Clear();
        }

        void UpdateIncomePopups(float dt)
        {
            if (paused || confirmExit)
            {
                return;
            }

            for (int index = incomePopups.Count - 1; index >= 0; index--)
            {
                var popup = incomePopups[index];
                popup.age += dt;
                if (popup.age >= 2.2f)
                {
                    if (popup.label)
                    {
                        Destroy(popup.label.gameObject);
                    }

                    incomePopups.RemoveAt(index);
                    continue;
                }

                if (!popup.label)
                {
                    continue;
                }

                Vector3 screen = cam.WorldToScreenPoint(popup.position + Vector3.up * popup.age * .045f);
                popup.label.gameObject.SetActive(screen.z > 0);
                popup.label.transform.position = screen;
                popup.label.color = new Color(.75f, 1, .5f, 1 - Mathf.Clamp01((popup.age - .55f) / 1.65f));
            }
        }

        void DynamiteBurst(Vector3 point)
        {
            SaloonAudio.Play("explosion");
            var g = new GameObject("Dynamite impact - dust and smoke");
            g.transform.SetParent(feedbackRoot);
            g.transform.position = point;
            var ps = g.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1.1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(.45f, .9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.035f, .12f);
            main.startSize = new ParticleSystem.MinMaxCurve(.015f, .045f);
            main.startColor = new Color(.65f, .48f, .29f, .7f);
            main.gravityModifier = -.015f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 24;
            main.stopAction = ParticleSystemStopAction.Destroy;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0, 16) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = .022f;
            var colors = ps.colorOverLifetime;
            colors.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(new Color(.85f, .63f, .32f), 0), new GradientColorKey(new Color(.35f, .30f, .25f), 1) }, new[] { new GradientAlphaKey(.7f, 0), new GradientAlphaKey(.4f, .4f), new GradientAlphaKey(0, 1) });
            colors.color = gradient;
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .35f), new Keyframe(.45f, 1), new Keyframe(1, 1.25f)));
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = dynamiteDustMaterial ? dynamiteDustMaterial : DustMaterial();
            ps.Play();
        }

        static Material dustMaterial;
        static Material DustMaterial()
        {
            if (dustMaterial)
            {
                return dustMaterial;
            }

            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texture.name = "Soft dust sprite";
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float radius = new Vector2((x - 15.5f) / 15.5f, (y - 15.5f) / 15.5f).magnitude;
                    texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - radius), 2)));
                }
            }

            texture.Apply();
            dustMaterial = new Material(Shader.Find("WildWestTD/SoftDust"));
            dustMaterial.name = "Dynamite soft dust";
            dustMaterial.SetFloat("_Surface", 1);
            dustMaterial.SetFloat("_Blend", 0);
            dustMaterial.SetFloat("_SrcBlend", 5);
            dustMaterial.SetFloat("_DstBlend", 10);
            dustMaterial.SetFloat("_ZWrite", 0);
            dustMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            dustMaterial.SetTexture("_MainTex", texture);
            dustMaterial.renderQueue = 3000;
            return dustMaterial;
        }
    }
}

