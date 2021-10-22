using Common.UI.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Urho;

namespace Common.UI.Components
{
    public class Mesh : IDisposable
    {
        #region Internal Classes

        public class Triangle
        {
            public uint I1, In1, I2, In2, I3, In3;
            public uint Iu1, Iu2, Iu3;

            public bool IsNormalAligned => I1 == In1 && I2 == In2 && I3 == In3;

            public Triangle(uint i1, uint iu1, uint in1, uint i2, uint iu2, uint in2, uint i3, uint iu3, uint in3)
            {
                I1 = i1;
                Iu1 = iu1;
                In1 = in1;
                I2 = i2;
                Iu2 = iu2;
                In2 = in2;
                I3 = i3;
                Iu3 = iu3;
                In3 = in3;
            }
        }

        public class Geometry
        {
            public const string DefaultMaterialName = "DEFAULT";
            public string MaterialName { get; set; }
            public List<Triangle> Triangles { get; set; } = new List<Triangle>();

            public Geometry(string materialName = DefaultMaterialName)
            {
                MaterialName = materialName;
            }
        }

        public class Material
        {
            public string TextureFileName { get; set; }
        }

        #endregion Internal Classes

        private static LRUCache<string, Mesh> _cache = new LRUCache<string, Mesh>(5);

        public static Mesh GetMesh(string filename)
        {
            Mesh mesh = _cache[filename];

            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.Load(filename);

                if (mesh.Vertices.Any()) // We don't want to add the mesh to the cache if it is empty (failed to load)
                {
                    _cache[filename] = mesh;
                }
            }

            return mesh;
        }

        public List<Vector3> Vertices { get; set; }

        public List<Vector2> UV { get; set; }

        public List<Vector3> Normals { get; set; }

        public List<Color> Colors { get; set; }

        public List<Geometry> Geometries { get; set; }

        public Dictionary<string, Material> Materials { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public int TriangleCount { get; set; }

        public bool IsLoaded { get; set; }
        public string Filename { get; set; }
        public bool HasTextureCoords => UV != null && UV.Count > 0;

        public Mesh()
        {
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            Colors = new List<Color>();
            UV = new List<Vector2>();
            Materials = new Dictionary<string, Material>();
            Geometries = new List<Geometry>();
        }

        private void ClearAll()
        {
            Vertices.Clear();
            Normals.Clear();
            Colors.Clear();
            UV.Clear();
            Geometries.Clear();
            Materials.Clear();
            BoundingBox = new BoundingBox();
        }
        
        /// <summary>
        /// Loads an OBJ file and its related textures (if present) into this Mesh object.
        /// </summary>
        /// <param name="path">The full path of the OBJ file.</param>
        /// <returns>True if the OBJ file was successfully parsed, false if something went wrong.</returns>
        public bool Load(string path)
        {
            if (IsLoaded && Filename == path)
                return true;

            if (!File.Exists(path))
                return false;

            ClearAll();

            string objFileName = Path.GetFileName(path);
            string basePath = Path.GetDirectoryName(path);

            float minX, minY, minZ;
            minX = minY = minZ = float.MaxValue;

            float maxX, maxY, maxZ;
            maxX = maxY = maxZ = float.MinValue;

            using (var t = new ScopeTimer($"Mesh.Load: {objFileName}"))
            {
                using (var fileStream = File.OpenRead(Path.Combine(basePath, objFileName)))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096))
                    {
                        try
                        {
                            string line = null;
                            bool allIndicesAreAligned = true;

                            Geometry currentGeometry = new Geometry();

                            while ((line = streamReader.ReadLine()) != null)
                            {
                                if (string.IsNullOrWhiteSpace(line) || line.Length < 2)
                                    continue;

                                var tokens = line.Split(' ');

                                switch (tokens[0])
                                {
                                    // Mtl File Reference
                                    case "mtllib":
                                        if (tokens.Length > 2)
                                            throw new Exception("MTL file references must have only 2 tokens.");

                                        Materials = ParseMtlFile(basePath, tokens[1]);

                                        break;

                                    // Texture Reference
                                    case "usemtl":
                                        if (tokens.Length > 2)
                                            throw new Exception("Texture references must have only 2 tokens.");

                                        currentGeometry = new Geometry(tokens[1]);
                                        Geometries.Add(currentGeometry);

                                        break;

                                    // Vertex
                                    case "v":
                                        if (tokens.Length != 4 && tokens.Length != 7)
                                            throw new Exception("Vertices must have either 4 or 7 tokens.");

                                        Vector3 newVertex = new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
                                        newVertex = newVertex.ConvertBetweenCoordinateSystems();
                                        Vertices.Add(newVertex);

                                        minX = Math.Min(minX, newVertex.X);
                                        minY = Math.Min(minY, newVertex.Y);
                                        minZ = Math.Min(minZ, newVertex.Z);

                                        maxX = Math.Max(maxX, newVertex.X);
                                        maxY = Math.Max(maxY, newVertex.Y);
                                        maxZ = Math.Max(maxZ, newVertex.Z);

                                        if (tokens.Length == 7)
                                            Colors.Add(new Color(float.Parse(tokens[4]), float.Parse(tokens[5]),
                                                float.Parse(tokens[6])));

                                        break;

                                    // Normal
                                    case "vn":
                                        if (tokens.Length != 4)
                                            throw new Exception("Vertex normals must have exactly 4 tokens.");

                                        Vector3 newNormal = new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
                                        newNormal = newNormal.ConvertBetweenCoordinateSystems();
                                        Normals.Add(newNormal);

                                        break;

                                    // Texture Coordinates (UV)
                                    case "vt":
                                        if (tokens.Length != 3)
                                            throw new Exception("Texture coordinates must have exactly 3 tokens.");

                                        UV.Add(new Vector2(float.Parse(tokens[1]), 1.0f - float.Parse(tokens[2])));

                                        break;

                                    // Face
                                    case "f":
                                        if (tokens.Length != 4)
                                            throw new Exception("Every face must have exactly 4 tokens.");

                                        // [0] = Vertex Index, [1] = Texture Coordinate (UV), [2] = Normal Index
                                        var v1 = tokens[1].Split('/');
                                        var v2 = tokens[2].Split('/');
                                        var v3 = tokens[3].Split('/');

                                        // Each face token is a triplet, with exactly 3 sub-tokens
                                        if (v1.Length != 3 || v2.Length != 3 || v3.Length != 3)
                                            throw new Exception("Every face token must be a triplet with exactly 3 sub-tokens.");

                                        // OBJ indexing starts at 1, so subtract 1 for each to get correct list index
                                        uint i1 = uint.Parse(v1[0]) - 1;
                                        uint iu1 = uint.TryParse(v1[1], out uint uv1) ? uv1 - 1 : 0;
                                        uint in1 = uint.Parse(v1[2]) - 1;

                                        uint i2 = uint.Parse(v2[0]) - 1;
                                        uint iu2 = uint.TryParse(v2[1], out uint uv2) ? uv2 - 1 : 0;
                                        uint in2 = uint.Parse(v2[2]) - 1;

                                        uint i3 = uint.Parse(v3[0]) - 1;
                                        uint iu3 = uint.TryParse(v3[1], out uint uv3) ? uv3 - 1 : 0;
                                        uint in3 = uint.Parse(v3[2]) - 1;

                                        Triangle tri = new Triangle(i1, iu1, in1, i2, iu2, in2, i3, iu3, in3);

                                        if (!tri.IsNormalAligned)
                                            allIndicesAreAligned = false;

                                        tri.ConvertBetweenCoordinateSystems();
                                        currentGeometry.Triangles.Add(tri);
                                        TriangleCount++;

                                        break;

                                    default:
                                        break;
                                }
                            }
                            
                            // If <= 1 texture was used, it won't have been added while parsing (no "usemtl" defined in OBJ)
                            if (currentGeometry.MaterialName == Geometry.DefaultMaterialName)
                            {
                                Geometries.Add(currentGeometry);
                            }

                            if (!allIndicesAreAligned)
                            {
                                AlignNormals();
                            }

                            // Check to make sure the mesh is valid
                            if (Vertices.Count != Normals.Count)
                                throw new Exception("Vertex count doesn't not match normal count.");

                            if (Colors.Count > 0 && Colors.Count != Vertices.Count)
                                throw new Exception("Color count does not match vertex count.");

                            foreach(var g in Geometries)
                            {
                                foreach (var tri in g.Triangles)
                                {
                                    if (tri.I1 > Vertices.Count || tri.I2 > Vertices.Count || tri.I3 > Vertices.Count)
                                        throw new Exception("Triangle vertex index out of bounds.");

                                    if (tri.In1 > Normals.Count || tri.In2 > Normals.Count || tri.In3 > Normals.Count)
                                        throw new Exception("Triangle normal index out of bounds.");

                                    if (UV.Count > 0 && !(tri.Iu1 < UV.Count && tri.Iu2 < UV.Count && tri.Iu3 < UV.Count))
                                        throw new Exception("Triangle uv index out of bounds.");
                                }
                            }
                            
                        }
                        catch (Exception e)
                        {
                            ClearAll();

                            Debug.WriteLine($"Failed to parse {objFileName}. Exception: {e.Message}");
                            return false;
                        }
                    }
                }
            }

            BoundingBox = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));

            Debug.WriteLine($"Mesh loaded with {Vertices.Count} Vertices, {Normals.Count} Normals, {UV.Count} UV's, {Colors.Count} Colors, {TriangleCount} Triangles, {Geometries.Count} Geometries");

            IsLoaded = true;
            Filename = path;

            return true;
        }

        private static Dictionary<string, Material> ParseMtlFile(string basePath, string mtlFileName)
        {
            Dictionary<string, Material> materials = new Dictionary<string, Material>();

            using (var timer = new ScopeTimer($"Parse MTL File: {mtlFileName}"))
            {
                using (var fileStream = File.OpenRead(Path.Combine(basePath, mtlFileName)))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096))
                    {
                        try
                        {
                            string currentTextureName = null;
                            string line = null;

                            Material currentMaterial = null;

                            while ((line = streamReader.ReadLine()) != null)
                            {
                                string[] tokens = line.Split(' ');
                                switch (tokens[0])
                                {
                                    case "newmtl":
                                        if (tokens.Length != 2)
                                            throw new Exception("New material declarations must have exactly 2 tokens.");

                                        currentMaterial = new Material();

                                        currentTextureName = tokens[1];
                                        materials.Add(currentTextureName, currentMaterial);

                                        break;

                                    case "map_Kd":
                                        if (tokens.Length != 2)
                                            throw new Exception("Material path lines must have exactly 2 tokens.");

                                        if(currentMaterial == null)
                                            throw new Exception("No current material defined.");

                                        currentMaterial.TextureFileName = tokens[1];

                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine($"Failed to parse {mtlFileName}. Exception: {e.Message}");
                        }
                    }
                }
            }

            return materials;
        }

        private void AlignNormals()
        {
            using (var t = new ScopeTimer("Mesh.AlignNormals"))
            {
                var alignedNormals = new Vector3[Vertices.Count];

                foreach(var g in Geometries)
                {
                    foreach (var tri in g.Triangles)
                    {
                        UpdateNormal(ref alignedNormals, (int)tri.I1, (int)tri.In1);
                        UpdateNormal(ref alignedNormals, (int)tri.I2, (int)tri.In2);
                        UpdateNormal(ref alignedNormals, (int)tri.I3, (int)tri.In3);
                    }
                }                

                Normals = alignedNormals.ToList();
            }
        }

        private void UpdateNormal(ref Vector3[] alignedNormals, int vIndex, int nIndex)
        {
            var n = Normals[nIndex];

            var an = alignedNormals[vIndex];
            if (an != null)
            {
                // average the two
                an = (an + n) * 0.5f;
                an.NormalizeFast();
            }
            else
            {
                an = n;
            }

            alignedNormals[vIndex] = an;
        }

        //Nothing to dispose of that GC won't take care of. Simply implementing interface for LRUCache.
        public void Dispose() { }
    }
}
