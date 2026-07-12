using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Physics_Engine
{
    

    public class Scene
    {
        public SceneObject[] objects;
        
        public Light[] lights;
        public Config config { get; set; }
        
        public Scene(SceneObject[] objects, Light[] lights, Config config)
        {
            this.objects = objects;
            this.lights = lights;
            this.config = config;
        }
    }
}
