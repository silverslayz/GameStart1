using System.IO;
using UnityEngine;
using UnityEditor;

namespace GameStart.EditorTools
{
    /// <summary>
    /// Sculpts and paints the Haven terrain from a seed, so the landscape rebuilds from
    /// source rather than existing as hand-painted data nobody can reproduce.
    ///
    /// The terrain shipped as a real Unity Terrain but was effectively unused: 0.44m of
    /// relief across 150m (visually flat) and zero terrain layers, so it rendered untextured.
    ///
    /// Everything in Haven is placed assuming flat ground, so a protected pad around the
    /// settlement keeps its ORIGINAL heights exactly and hills only rise outside it. Objects
    /// inside the pad therefore cannot end up floating or buried.
    /// </summary>
    public static class TerrainSculptor
    {
        // Derived from the actual scene layout: 22 gameplay objects centred near
        // (-7.8, 8.0) with a maximum radius of 22.1m.
        private const float PadCenterX = -7.8f;
        private const float PadCenterZ = 8.0f;
        private const float PadRadius = 26f;
        private const float PadFalloff = 22f;

        private const float HillHeightMetres = 9f;
        private const int Seed = 1337;

        private const string TextureDir = "Assets/Textures/Terrain";
        private const string LayerDir = "Assets/Terrain";

        [MenuItem("Aetherfall/Terrain/Sculpt and Paint Haven")]
        public static void Generate()
        {
            Terrain terrain = Object.FindFirstObjectByType<Terrain>(FindObjectsInactive.Include);
            if (terrain == null || terrain.terrainData == null)
            {
                Debug.LogError("TerrainSculptor: no Terrain in the scene.");
                return;
            }

            TerrainData td = terrain.terrainData;
            Sculpt(terrain, td);
            Paint(td);

            // Terrain detail and trees are drawn far more cheaply instanced; off by default.
            terrain.drawInstanced = true;

            EditorUtility.SetDirty(td);
            EditorUtility.SetDirty(terrain);
            AssetDatabase.SaveAssets();
            Debug.Log("TerrainSculptor: done.");
        }

        // ------------------------------------------------------------------ sculpting

        private static void Sculpt(Terrain terrain, TerrainData td)
        {
            int res = td.heightmapResolution;
            float[,] heights = td.GetHeights(0, 0, res, res);

            Vector3 origin = terrain.transform.position;
            Vector3 size = td.size;
            float amplitude = HillHeightMetres / size.y;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    // Heightmap is indexed [z, x]; convert to world metres.
                    float wx = origin.x + (x / (float)(res - 1)) * size.x;
                    float wz = origin.z + (y / (float)(res - 1)) * size.z;

                    float dist = Mathf.Sqrt((wx - PadCenterX) * (wx - PadCenterX) +
                                            (wz - PadCenterZ) * (wz - PadCenterZ));

                    // 0 inside the settlement, easing to 1 beyond it. SmoothStep avoids a
                    // visible crease at the pad boundary.
                    float blend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(PadRadius, PadRadius + PadFalloff, dist));
                    if (blend <= 0f) continue;   // pad keeps its original height exactly

                    heights[y, x] += Fbm(wx, wz) * amplitude * blend;
                }
            }

            td.SetHeights(0, 0, heights);
        }

        /// <summary>Layered Perlin noise. Octaves at rising frequency give hills with texture
        /// rather than a single smooth swell.</summary>
        private static float Fbm(float wx, float wz)
        {
            float total = 0f;
            float amp = 1f;
            float freq = 1f / 55f;   // broadest features roughly every 55 metres
            float norm = 0f;

            for (int o = 0; o < 4; o++)
            {
                total += Mathf.PerlinNoise((wx + Seed) * freq, (wz + Seed) * freq) * amp;
                norm += amp;
                amp *= 0.45f;
                freq *= 2.1f;
            }

            return total / norm;
        }

        // ------------------------------------------------------------------ painting

        private static void Paint(TerrainData td)
        {
            TerrainLayer grass = BuildLayer("Grass", new Color(0.31f, 0.44f, 0.20f), new Color(0.24f, 0.35f, 0.15f));
            TerrainLayer dirt = BuildLayer("Dirt", new Color(0.42f, 0.31f, 0.20f), new Color(0.33f, 0.24f, 0.15f));

            td.terrainLayers = new[] { grass, dirt };

            int res = td.alphamapResolution;
            float[,,] map = new float[res, res, 2];

            // Height range drives the altitude term; sampled once rather than per pixel.
            int hres = td.heightmapResolution;
            float[,] heights = td.GetHeights(0, 0, hres, hres);
            float hMin = 1f, hMax = 0f;
            for (int y = 0; y < hres; y++)
                for (int x = 0; x < hres; x++)
                {
                    if (heights[y, x] < hMin) hMin = heights[y, x];
                    if (heights[y, x] > hMax) hMax = heights[y, x];
                }

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    // GetSteepness takes normalised coordinates and returns degrees.
                    float nx = x / (float)(res - 1);
                    float ny = y / (float)(res - 1);

                    float steep = td.GetSteepness(nx, ny);
                    float slopeTerm = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(5f, 17f, steep));

                    // Altitude term as well as slope: with gentle hills, slope alone never
                    // crosses the threshold and the whole map stays uniformly green.
                    float h = td.GetInterpolatedHeight(nx, ny) / td.size.y;
                    float altitude = Mathf.InverseLerp(hMin, hMax, h);
                    float altTerm = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 0.95f, altitude));

                    // Whichever is stronger wins, so hill flanks AND crowns show earth.
                    float dirtWeight = Mathf.Clamp01(Mathf.Max(slopeTerm, altTerm * 0.75f));

                    map[y, x, 0] = 1f - dirtWeight;
                    map[y, x, 1] = dirtWeight;
                }
            }

            td.SetAlphamaps(0, 0, map);
        }

        /// <summary>
        /// Builds a TerrainLayer backed by a generated PNG. The project has no texture assets
        /// at all, so these are produced rather than imported.
        /// </summary>
        private static TerrainLayer BuildLayer(string name, Color baseColour, Color darkColour)
        {
            Directory.CreateDirectory(TextureDir);
            Directory.CreateDirectory(LayerDir);

            string texPath = TextureDir + "/" + name + ".png";
            WriteNoiseTexture(texPath, baseColour, darkColour, name.GetHashCode());

            AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

            string layerPath = LayerDir + "/" + name + ".terrainlayer";
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
            if (layer == null)
            {
                layer = new TerrainLayer();
                AssetDatabase.CreateAsset(layer, layerPath);
            }

            layer.diffuseTexture = tex;
            // Small enough to read as ground detail, large enough not to visibly repeat.
            layer.tileSize = new Vector2(12f, 12f);
            EditorUtility.SetDirty(layer);

            return layer;
        }

        /// <summary>
        /// Two-octave value noise between a base and a darker tone. Flat colour reads as
        /// plastic under a directional light; the variation gives the ground grain.
        /// </summary>
        private static void WriteNoiseTexture(string path, Color a, Color b, int seed)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGB24, false);
            var rng = new System.Random(seed);
            float ox = (float)rng.NextDouble() * 100f;
            float oz = (float)rng.NextDouble() * 100f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float v = y / (float)size;

                    float n = Mathf.PerlinNoise(ox + u * 14f, oz + v * 14f) * 0.65f
                            + Mathf.PerlinNoise(ox + u * 43f, oz + v * 43f) * 0.35f;

                    tex.SetPixel(x, y, Color.Lerp(b, a, Mathf.Clamp01(n)));
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
    }
}
