using CrsCommon.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Urho;
using Urho.Urho2D;

namespace CrsCommon.Components
{
    public class Mesh
    {
        private static Dictionary<string, Mesh> _cache = new Dictionary<string, Mesh>();

        public static Mesh GetMesh(string filename)
        {
            if (_cache.ContainsKey(filename))
                return _cache[filename];

            var mesh = new Mesh();
            mesh.Load(filename);
            _cache[filename] = mesh;

            return mesh;
        }

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

        public List<Vector3> Vertices { get; set; }

        public List<Vector2> UV { get; set; }

        public List<Vector3> Normals { get; set; }

        public List<Color> Colors { get; set; }

        public List<Triangle> Triangles { get; set; }

        public bool IsLoaded { get; set; }
        public string Filename { get; set; }
        public bool HasTextureCoords => UV != null && UV.Count > 0;

        public struct VertexData
        {
            public float vx;
            public float vy;
            public float vz;
            public float nx;
            public float ny;
            public float nz;
            public uint color;
        };

        public Mesh()
        {
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            Colors = new List<Color>();
            Triangles = new List<Triangle>();
            UV = new List<Vector2>();
        }

        public bool Load(string filename)
        {
            if (IsLoaded && Filename == filename)
                return true;

            Vertices.Clear();
            Normals.Clear();
            Colors.Clear();
            Triangles.Clear();
            UV.Clear();

            if (!File.Exists(filename))
                return false;

            var alignedIndexes = true;
            var scan = new ScanFormatted();

            using (var t = new ScopeTimer("Mesh.Load"))
            {
                using (var fileStream = File.OpenRead(filename))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096))
                    {
                        try
                        {
                            string line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                if (string.IsNullOrWhiteSpace(line) || line.Length < 2)
                                    continue;

                                var tokens = line.Split(' ');
                                if (tokens[0] == "vn")
                                {
                                    // vertex normals must have exactly 4 tokens
                                    if (tokens.Length != 4)
                                        return false;

                                    Normals.Add(new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]),
                                        float.Parse(tokens[3])));
                                }
                                else if (tokens[0] == "v")
                                {
                                    // vertices must have either 4 tokens or 7
                                    if (tokens.Length != 4 && tokens.Length != 7)
                                        return false;

                                    Vertices.Add(new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]),
                                        float.Parse(tokens[3])));
                                    if (tokens.Length == 7)
                                        Colors.Add(new Color(float.Parse(tokens[4]), float.Parse(tokens[5]),
                                            float.Parse(tokens[6])));
                                }
                                else if (tokens[0] == "f")
                                {
                                    // faces must have exactly 4 tokens 
                                    if (tokens.Length != 4)
                                        return false;

                                    var v1 = tokens[1].Split('/');
                                    var v2 = tokens[2].Split('/');
                                    var v3 = tokens[3].Split('/');

                                    // each face token is a triplet, with exactly 3 sub-tokens
                                    if (v1.Length != 3 || v2.Length != 3 || v3.Length != 3)
                                        return false;

                                    var tri = new Triangle(
                                        uint.Parse(v1[0]) - 1,
                                        uint.TryParse(v1[1], out uint uv1) ? uv1 - 1 : 0,
                                        uint.Parse(v1[2]) - 1,
                                        uint.Parse(v2[0]) - 1,
                                        uint.TryParse(v2[1], out uint uv2) ? uv2 - 1 : 0,
                                        uint.Parse(v2[2]) - 1,
                                        uint.Parse(v3[0]) - 1,
                                        uint.TryParse(v3[1], out uint uv3) ? uv3 - 1 : 0,
                                        uint.Parse(v3[2]) - 1);

                                    if (!tri.IsNormalAligned)
                                        alignedIndexes = false;

                                    Triangles.Add(tri);
                                }
                                else if (tokens[0] == "vt")
                                {
                                    // texture coordinates must have exactly 3 tokens
                                    if (tokens.Length != 3)
                                        return false;

                                    UV.Add(new Vector2(float.Parse(tokens[1]), 1.0f - float.Parse(tokens[2])));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // failed to parse OBJ
                            return false;
                        }
                    }
                }
            }

            ConvertBetweenCoordinateSystems();

            if (!alignedIndexes)
            {
                AlignNormals();
            }

            if (Vertices.Count != Normals.Count)
                return false;

            if (Colors.Count > 0 && Colors.Count != Vertices.Count)
                return false;

            if (UV.Count > 0 && UV.Count != 3 * Triangles.Count)
                return false;

            foreach (var tri in Triangles)
            {
                if (tri.I1 > Vertices.Count || tri.I2 > Vertices.Count || tri.I3 > Vertices.Count)
                    return false;

                if (tri.In1 > Normals.Count || tri.In2 > Normals.Count || tri.In3 > Normals.Count)
                    return false;

                if (UV.Count > 0 && !(tri.Iu1 < UV.Count && tri.Iu2 < UV.Count && tri.Iu3 < UV.Count))
                    return false;
            }

            Debug.WriteLine($"Loaded with {Vertices.Count} Vertices, {Normals.Count} Normals, {Colors.Count} Colors, {Triangles.Count} Triangles");

            IsLoaded = true;
            Filename = filename;

            return true;
        }

        private void AlignNormals()
        {
            using (var t = new ScopeTimer("Mesh.AlignNormals"))
            {
                var alignedNormals = new Vector3[Vertices.Count];

                foreach (var tri in Triangles)
                {
                    UpdateNormal(ref alignedNormals, (int)tri.I1, (int)tri.In1);
                    UpdateNormal(ref alignedNormals, (int)tri.I2, (int)tri.In2);
                    UpdateNormal(ref alignedNormals, (int)tri.I3, (int)tri.In3);
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

        private void ConvertBetweenCoordinateSystems()
        {
            var multiple = new Vector3(-1, 1, 1);
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = Vertices[i].ConvertBetweenCoordinateSystems();
            }

            for (int i = 0; i < Normals.Count; i++)
            {
                Normals[i] = Normals[i].ConvertBetweenCoordinateSystems();
            }

            Triangles?.ForEach(_ => _.ConvertBetweenCoordinateSystems());
        }

        public Urho.VertexBuffer.PositionNormalColor[] GetVertextData()
        {
            var data = new Urho.VertexBuffer.PositionNormalColor[Vertices.Count];

            for (int i = 0; i < Vertices.Count; i++)
            {
                var v = Vertices[i];
                var n = Normals[i];

                Urho.Color clr = Urho.Color.Green;
                if (Vertices.Count == Colors.Count)
                {
                    clr = Colors[i];
                }

                var d = new Urho.VertexBuffer.PositionNormalColor();

                d.Position = v;
                d.Normal = n;
                d.Color = clr.ToUInt();

                data[i] = d;
            }

            return data;
        }

        public Urho.VertexBuffer.PositionNormalColorTexcoord[] GetTexturedVertextData()
        {
            var data = new Urho.VertexBuffer.PositionNormalColorTexcoord[Triangles.Count * 3];

            for (int i = 0; i < Triangles.Count; i++)
            {
                var t = Triangles[i];
                var idx = i * 3;
                var d = new Urho.VertexBuffer.PositionNormalColorTexcoord
                {
                    Position = Vertices[(int)t.I1],
                    Normal = Normals[(int)t.In1],
                    TexCoord = UV[(int)t.Iu1],
                    Color = 0
                };
                data[idx] = d;

                d = new Urho.VertexBuffer.PositionNormalColorTexcoord
                {
                    Position = Vertices[(int)t.I2],
                    Normal = Normals[(int)t.In2],
                    TexCoord = UV[(int)t.Iu2],
                    Color = 0
                };
                data[idx + 1] = d;

                d = new Urho.VertexBuffer.PositionNormalColorTexcoord
                {
                    Position = Vertices[(int)t.I3],
                    Normal = Normals[(int)t.In3],
                    TexCoord = UV[(int)t.Iu3],
                    Color = 0
                };
                data[idx + 2] = d;
            }

            return data;
        }

        public uint[] GetIndexData()
        {
            var data = new uint[3 * Triangles.Count];

            for (int i = 0; i < Triangles.Count; i++)
            {
                int idx = 3 * i;

                data[idx + 0] = Triangles[i].I1;
                data[idx + 1] = Triangles[i].I2;
                data[idx + 2] = Triangles[i].I3;
            }

            return data;
        }

        public uint[] GetTexturedIndexData(Urho.VertexBuffer.PositionNormalColorTexcoord[] vb)
        {
            var data = new uint[vb.Length];

            for (int i = 0; i < vb.Length; i++)
            {
                data[i] = (uint)i;
            }

            return data;
        }

        public Urho.BoundingBox GetBoundingBox()
        {
            float minx, miny, minz, maxx, maxy, maxz;

            minx = miny = minz = float.MaxValue;
            maxx = maxy = maxz = float.MinValue;

            foreach (var v in Vertices)
            {
                minx = Math.Min(minx, v.X);
                miny = Math.Min(miny, v.Y);
                minz = Math.Min(minz, v.Z);
                maxx = Math.Max(maxx, v.X);
                maxy = Math.Max(maxy, v.Y);
                maxz = Math.Max(maxz, v.Z);
            }

            return new Urho.BoundingBox(
                new Urho.Vector3(minx, miny, minz),
                new Urho.Vector3(maxx, maxy, maxz));
        }
    }
}
