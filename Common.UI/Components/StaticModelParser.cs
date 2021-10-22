using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Urho;
using Urho.Urho2D;

namespace Common.UI.Components
{
    public static class StaticModelParser
    {
        /// <summary>
        /// Parses an OBJ file into a complete StaticModel component and attaches it to a parent node for rendering. <br/>
        /// This function should only be called from the Urho main thread to avoid memory access errors. (i.e. wrap in Urho.Application.InvokeOnMain())
        /// </summary>
        /// <param name="objFilePath">The full file path of the OBJ file to be parsed.</param>
        /// <param name="parent">The parent node the StaticModel component will be attached to. <br/>
        /// It is recommended that this be an emtpy, dedicated node as transformations may be applied.</param>
        /// <returns>True if successfully parsed, false if something went wrong.</returns>
        public static bool ParseAndAttachOBJ(string objFilePath, Node parent)
        {
            Mesh mesh = Mesh.GetMesh(objFilePath);

            if (!mesh.Vertices.Any()) return false; // Failed to load mesh.

            var basePath = Path.GetDirectoryName(objFilePath);

            List<Geometry> urhoGeometries = CreateGeometries(mesh);

            Model model = new Model();
            model.NumGeometries = (uint)mesh.Geometries.Count;
            model.BoundingBox = mesh.BoundingBox;

            List<Material> materials = new List<Material>();

            for (int i = 0; i < urhoGeometries.Count; i++)
            {
                var currentGeometry = mesh.Geometries[i];

                Material material = new Material();
                string textureFileName = null;

                if (currentGeometry.MaterialName == Mesh.Geometry.DefaultMaterialName)
                {
                    var mat = mesh.Materials.Values.FirstOrDefault();
                    textureFileName = mat?.TextureFileName;
                }
                else if (mesh.Materials.TryGetValue(mesh.Geometries[i].MaterialName, out var mat))
                {
                    textureFileName = mat.TextureFileName;
                }

                if (!string.IsNullOrWhiteSpace(textureFileName))
                {
                    string texturePath = Path.Combine(basePath, textureFileName);
                    Texture2D texture = Urho.Application.Current.ResourceCache.GetTexture2D(texturePath);
                    material.SetTexture(TextureUnit.Diffuse, texture);
                    material.SetTechnique(0, CoreAssets.Techniques.DiffUnlit, 0, 0);
                }
                else
                {
                    material.SetTechnique(0, CoreAssets.Techniques.NoTextureVCol, 1, 1);
                }

                model.SetGeometry((uint)i, 0, urhoGeometries[i]);
                materials.Add(material);
            }

            // You can't modify a StaticModel's Model after it has been attached, so it must be done after the model has been created and the materials have been gathered.
            StaticModel staticModel = parent.CreateComponent<StaticModel>();
            staticModel.Model = model;
            staticModel.CastShadows = true;

            for (int i = 0; i < materials.Count; i++)
            {
                staticModel.SetMaterial((uint)i, materials[i]);
            }

            parent.Position = staticModel.WorldBoundingBox.Center * -1;

            Debug.WriteLine($"Successfully parsed {objFilePath}.");
            return true;
        }

        private static List<Geometry> CreateGeometries(Mesh mesh)
        {
            List<Geometry> geometries = new List<Geometry>();

            for (int i = 0; i < mesh.Geometries.Count; i++)
            {
                var currentMeshGeometry = mesh.Geometries[i];

                VertexBuffer vb = new VertexBuffer(Urho.Application.CurrentContext, false);
                IndexBuffer ib = new IndexBuffer(Urho.Application.CurrentContext, false);

                vb.Shadowed = true;
                ib.Shadowed = true;

                uint indexCount = 0;
                if (mesh.HasTextureCoords)
                {
                    var vdata = GetTexturedVertextData(mesh.Vertices, mesh.UV, mesh.Normals, currentMeshGeometry.Triangles);
                    vb.SetSize((uint)vdata.Length, ElementMask.Position | ElementMask.Normal | ElementMask.Color | ElementMask.TexCoord1, false);
                    vb.SetData(vdata);

                    var idata = GetTexturedIndexData(vdata);
                    indexCount = (uint)idata.Length;
                    ib.SetSize(indexCount, true);
                    ib.SetData(idata);
                }
                else
                {
                    var vdata = GetVertextData(mesh.Vertices, mesh.Normals, mesh.Colors);
                    vb.SetSize((uint)vdata.Length, ElementMask.Position | ElementMask.Normal | ElementMask.Color, false);
                    vb.SetData(vdata);

                    var idata = GetIndexData(currentMeshGeometry.Triangles);
                    indexCount = (uint)idata.Length;
                    ib.SetSize(indexCount, true);
                    ib.SetData(idata);
                }

                Geometry newGeometry = new Geometry();
                newGeometry.SetVertexBuffer(0, vb);
                newGeometry.IndexBuffer = ib;
                newGeometry.SetDrawRange(PrimitiveType.TriangleList, 0, indexCount, true);

                geometries.Add(newGeometry);
            }

            return geometries;
        }

        private static Urho.VertexBuffer.PositionNormalColor[] GetVertextData(List<Vector3> vertices, List<Vector3> normals, List<Color> colors)
        {
            var data = new Urho.VertexBuffer.PositionNormalColor[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                var n = normals[i];

                Urho.Color clr = Urho.Color.Green;
                if (vertices.Count == colors.Count)
                {
                    clr = colors[i];
                }

                var d = new Urho.VertexBuffer.PositionNormalColor();

                d.Position = v;
                d.Normal = n;
                d.Color = clr.ToUInt();

                data[i] = d;
            }

            return data;
        }

        private static Urho.VertexBuffer.PositionNormalColorTexcoord[] GetTexturedVertextData(List<Vector3> vertices, List<Vector2> uv, List<Vector3> normals, List<Mesh.Triangle> triangles)
        {
            var data = new Urho.VertexBuffer.PositionNormalColorTexcoord[triangles.Count * 3];

            for (int i = 0; i < triangles.Count; i++)
            {
                var t = triangles[i];
                var idx = i * 3;
                var d = new Urho.VertexBuffer.PositionNormalColorTexcoord
                {
                    Position = vertices[(int)t.I1],
                    Normal = normals[(int)t.In1],
                    TexCoord = uv[(int)t.Iu1],
                    Color = 0
                };
                data[idx] = d;

                d = new Urho.VertexBuffer.PositionNormalColorTexcoord
                {
                    Position = vertices[(int)t.I2],
                    Normal = normals[(int)t.In2],
                    TexCoord = uv[(int)t.Iu2],
                    Color = 0
                };
                data[idx + 1] = d;

                d = new Urho.VertexBuffer.PositionNormalColorTexcoord
                {
                    Position = vertices[(int)t.I3],
                    Normal = normals[(int)t.In3],
                    TexCoord = uv[(int)t.Iu3],
                    Color = 0
                };
                data[idx + 2] = d;
            }

            return data;
        }

        private static uint[] GetIndexData(List<Mesh.Triangle> triangles)
        {
            var data = new uint[3 * triangles.Count];

            for (int i = 0; i < triangles.Count; i++)
            {
                int idx = 3 * i;

                data[idx + 0] = triangles[i].I1;
                data[idx + 1] = triangles[i].I2;
                data[idx + 2] = triangles[i].I3;
            }

            return data;
        }

        private static uint[] GetTexturedIndexData(Urho.VertexBuffer.PositionNormalColorTexcoord[] vb)
        {
            var data = new uint[vb.Length];

            for (int i = 0; i < vb.Length; i++)
            {
                data[i] = (uint)i;
            }

            return data;
        }
    }
}
