using Urho;
using static Common.Components.Mesh;

namespace Common.Common
{
    public static class UrhoExtensions
    {
        public static Vector3 ConvertBetweenCoordinateSystems(this Vector3 vector) => new Vector3(-vector.X, vector.Y, vector.Z);

        public static Quaternion ConvertBetweenCoordinateSystems(this Quaternion quaternion) =>  new Quaternion(-quaternion.X, quaternion.Y, quaternion.Z, -quaternion.W);

        public static void ConvertBetweenCoordinateSystems(this Triangle triangle)
        {
            //Change the order of the face's vertices so it shows on the opposite side
            // of the triangle since we mirrored the model
            var i1 = triangle.I1;
            var in1 = triangle.In1;
            var iu1 = triangle.Iu1;
            triangle.I1 = triangle.I3;
            triangle.In1 = triangle.In3;
            triangle.Iu1 = triangle.Iu3;
            triangle.I3 = i1;
            triangle.In3 = in1;
            triangle.Iu3 = iu1;
        }
    }
}
