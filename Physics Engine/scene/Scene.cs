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
        public Config_Render render_config { get; set; }
        
        public Scene(SceneObject[] objects, Light[] lights, Config_Render render_config)
        {
            this.objects = objects;
            this.lights = lights;
            this.render_config = render_config;
        }
    }
}
