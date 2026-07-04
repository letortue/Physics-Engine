using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine
{
    
    public class Material
    {
        public bool lambertianShading;
        public bool interpolateColor;
        public bool oneSided;
        public bool[] facingRatioShadingFaces;
        public Vec3[] faceColors;
        public bool isReflective;
        public bool isRefractive;
        public double refractionIndex;
        public Func<Vec3, Vec3> textureFunc;

        public Vec3[] vertexColors;
        public double[] vertexTextureT;
        public double[] vertexTextureV;



    }
    public class MeshProps
    {
        public Vec3[] vertices;
        public int[] indices;

        public Vec3[] vertexNormals;
        public Vec3[] faceNormals;
    }
    public abstract class Objectu
    {
        public Material material;
        public MeshProps meshProps;

    }
    public class Meshu : Objectu
    {
        public Meshu(Material material, MeshProps meshProps)
        {
            this.material = material;
            this.meshProps = meshProps;
        }
    }
    public class Ballu : Objectu
    {
        public double radius { get; set; }
        public Vec3 center { get; set; }
        public Ballu(Material material, double radius, Vec3 center)
        {
            this.material = material;
            this.radius = radius;
            this.center = center;
        }
    }

    public class Triangleu : Objectu
    {
        public Triangleu(Material material, MeshProps meshProps)
        {
            this.material = material;
            this.meshProps = meshProps;
        }
    }


}
