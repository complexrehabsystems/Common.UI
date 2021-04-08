using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Urho;
using Urho.Shapes;
using Common.Controls;

namespace Common.Components
{
    public class RotationInput : Component
    {
        protected UrhoApp App => Application.Current as UrhoApp;

        private const int DIVISIONS = 100;
        private const float RADIUS = 1.5f;


        Component xAxis;
        Component yAxis;
        Component zAxis;

        Color colorUnselected = new Color(0f, 0f, 0f, 0.0f);
        Color colorSelected = new Color(0f, 0f, 0f, 0.1f);
        Color colorWedge = new Color(1f, 1f, 1f, 0.8f);

        List<Vector3> circlePoints = new List<Vector3>();

        public RotationInput()
        {
            ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            CreateCirclePoints();

            yAxis = GetCircleComponent("yAxis", Color.Green);
            yAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, 90));

            zAxis = GetCircleComponent("zAxis", Color.Red);          

            xAxis = GetCircleComponent("xAxis", Color.Blue);
            xAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, 90));

        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            var input = Application.Input;

            if (input.GetMouseButtonDown(MouseButton.Left) || input.NumTouches == 1 && App.TouchedNode != null)
            {
                TouchState state = input.GetTouch(0);
                if (state.Pressure != 1.0)
                {
                    return;
                }

                int sign_x = state.Delta.X < 0 ? -1 : 1;
                int sign_y = state.Delta.Y < 0 ? -1 : 1;

                var delta = (sign_x * state.Delta.X * state.Delta.X + sign_y * state.Delta.Y * state.Delta.Y) / 4;

                if (App.TouchedNode.IsChildOf(yAxis.Node))
                {
                    float touchedX = state.Position.X;
                    float touchedY = state.Position.Y;
                    Debug.WriteLine(touchedX);
                    Debug.WriteLine(touchedY);

                    Vector2 originPoint = App.Camera.WorldToScreenPoint(App.RootNode.Position);

                    Debug.WriteLine(originPoint.X * App.Graphics.Width);
                    Debug.WriteLine(originPoint.Y * App.Graphics.Height);
                    float pixelOriginPointX = originPoint.X * App.Graphics.Width;
                    float pixelOriginPointY = originPoint.Y * App.Graphics.Height;

                    Vector3 mouseMovement = new Vector3(state.Delta.X, state.Delta.Y, 0);
                    Vector3 toOrigin = new Vector3(pixelOriginPointX - touchedX, pixelOriginPointY - touchedY, 0);

                    Vector3 crossProduct = Vector3.Cross(mouseMovement, toOrigin);

                    float deltaZ = crossProduct.Z / 1000;

                    App.RootNode.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, deltaZ), TransformSpace.World);
                    yAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, -deltaZ), TransformSpace.World);
                    xAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, -deltaZ), TransformSpace.World);
                    zAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, -deltaZ), TransformSpace.World);
                }
                else if (App.TouchedNode.IsChildOf(xAxis.Node))
                {
                    delta = state.Delta.Y;
                    App.RootNode.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, delta), TransformSpace.World);
                    yAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, -delta), TransformSpace.World);
                    xAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, -delta), TransformSpace.World);
                    zAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, -delta), TransformSpace.World);
                }
                else if (App.TouchedNode.IsChildOf(zAxis.Node))
                {
                    delta = -state.Delta.X;
                    App.RootNode.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, delta), TransformSpace.World);
                    yAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, -delta), TransformSpace.World);
                    xAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, -delta), TransformSpace.World);
                    zAxis.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, -delta), TransformSpace.World);
                }
            }
        }


        void CreateCirclePoints()
        {
            circlePoints.Clear();

            for (int i = 0; i <= DIVISIONS; i++)
            {
                double angle = i * 2 * Math.PI / DIVISIONS;
                float x = (float)Math.Cos(angle);
                float y = 0;
                float z = (float)Math.Sin(angle);

                circlePoints.Add(RADIUS * new Vector3(x, y, z));
            }
        }

        Component GetCircleComponent(string label, Color clr)
        {

            List<VertexBuffer.PositionNormal> circleVertices = new List<VertexBuffer.PositionNormal>();

            for (int i = 0; i <= DIVISIONS; i++)
            {
                circleVertices.Add(new VertexBuffer.PositionNormal
                {
                    Position = circlePoints[i]
                });
            }

            var circleBuffer = new VertexBuffer(Application.CurrentContext, false);
            circleBuffer.SetSize((uint)circleVertices.Count, ElementMask.Position | ElementMask.Normal, false);
            circleBuffer.SetData(circleVertices.ToArray());

            var circleGeometry = new Geometry();
            circleGeometry.SetVertexBuffer(0, circleBuffer);
            circleGeometry.SetDrawRange(PrimitiveType.LineStrip, 0, 0, 0, (uint)circleVertices.Count, true);

            Model circleModel = new Model();
            circleModel.NumGeometries = 1;
            circleModel.SetGeometry(0, 0, circleGeometry);
            circleModel.BoundingBox = new BoundingBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));

            Node circleNode = Node.CreateChild(label);
            StaticModel circle = circleNode.CreateComponent<StaticModel>();
            circle.Model = circleModel;

            Material lineMaterial = Material.FromColor(clr);
            lineMaterial.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlit, 1, 1);
            lineMaterial.LineAntiAlias = true;
            circle.SetMaterial(lineMaterial);

            Node ballNode = circle.Node.CreateChild();
            var ball = ballNode.CreateComponent<Sphere>();
            ball.Color = clr;
            ballNode.Scale = new Vector3(0.15f, 0.15f, 0.15f);
            ballNode.Position = circlePoints[12];

            return circle;
        }
    }
}
