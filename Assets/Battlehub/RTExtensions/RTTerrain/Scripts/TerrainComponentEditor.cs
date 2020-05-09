using Battlehub.RTEditor;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainComponentEditor : ComponentEditor
    {
        [SerializeField]
        private TerrainEditor m_terrainEditor = null;

        public override Component Component
        {
            get { return base.Component; }
            set
            {
                base.Component = value;
                if(m_terrainEditor != value)
                {
                    m_terrainEditor.Terrain = value as Terrain;
                }
            }
        }

        protected override void DestroyEditor()
        {
            DestroyGizmo();
        }

        protected override void BuildEditor(IComponentDescriptor componentDescriptor, PropertyDescriptor[] descriptors)
        {
            
        }
    }
}
