namespace Physics_Engine
{

    public static class FileReader
    {
        public static Mesh ReadOBJ(string filepath, VertexAttributes attributes, ShadingAttributes shading)
        {
            List<Vec3> vertices = new List<Vec3>();
            List<int> indices = new List<int>();
            List<Vec3> faceNormals = new List<Vec3>();
            List<int> faces = new List<int>();
            List<int> faceNormalIndices = new List<int>();

            string[] lines = File.ReadAllLines(filepath);
            
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                switch (parts[0])
                {
                    case "v":
                        vertices.Add(new Vec3(double.Parse(parts[1]), double.Parse(parts[2]), double.Parse(parts[3])));
                        break;
                    case "vn":
                        faceNormals.Add(new Vec3(double.Parse(parts[1]), double.Parse(parts[2]), double.Parse(parts[3])));
                        break;
                    case "f":
                        int faceNormalIdx = 0;
                        for (int j = 1; j < parts.Length; j++)
                        {
                            string[] indexParts = parts[j].Split('/');
                            int vIndex = int.Parse(indexParts[0]) - 1;
                            indices.Add(vIndex);
                            if (j == 1 && indexParts.Length > 2 && indexParts[2] != "")
                                faceNormalIdx = int.Parse(indexParts[2]) - 1;
                        }
                        faces.Add(parts.Length - 1);
                        faceNormalIndices.Add(faceNormalIdx);
                        break;


                }


            }
            
            Vec3[] colors = new Vec3[vertices.Count];
            Vec3[] velocity = new Vec3[vertices.Count];
            Vec3[] acceleration = new Vec3[vertices.Count];
            double[] opacity = new double[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                colors[i] = attributes.colors[0];
                velocity[i] = attributes.velocity[0];
                acceleration[i] = attributes.acceleration[0];
                opacity[i] = attributes.opacity[0];

            }
            attributes.colors = colors;
            attributes.velocity = velocity;
            attributes.acceleration = acceleration;
            attributes.opacity = opacity;

            shading.normal = faceNormals.ToArray();
            
            Mesh mesh = new Mesh(vertices.ToArray(), attributes, shading, faceNormals.ToArray(), faces.ToArray(), indices.ToArray());
            
            return mesh;
            
        }
    }
}