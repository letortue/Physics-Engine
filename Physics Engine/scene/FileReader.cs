namespace Physics_Engine
{

    public static class FileReader
    {
        public static Mesh ReadOBJ(string filepath)
        {
            List<Vec3> vertices = new List<Vec3>();
            List<int> indices = new List<int>();
            List<Vec3> faceNormals = new List<Vec3>();
            List<int> faces = new List<int>();

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
                        for (int j = 1; j < parts.Length; j++)
                        {
                            string[] indexParts = parts[j].Split('/');
                            int vIndex = int.Parse(indexParts[0]) - 1;
                            indices.Add(vIndex);
                        }
                        faces.Add(parts.Length - 1);
                        break;

                }
                
                
            }
            
            
            Mesh mesh = new Mesh(vertices.ToArray(), new VertexAttributes(), new ShadingAttributes(), faceNormals.ToArray(), faces.ToArray(), indices.ToArray());
            
            return mesh;
            
        }
    }
}