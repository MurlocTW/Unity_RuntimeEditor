using System; 
using System.Collections.Generic; 
namespace Battlehub.RTEditor
{
	public partial class EditorsMap
	{
		partial void InitEditorsMap()
		{
			m_map = new Dictionary<Type, EditorDescriptor>
			{
				{ typeof(UnityEngine.GameObject), new EditorDescriptor(0, true, false) },
				{ typeof(System.Object), new EditorDescriptor(1, true, true) },
				{ typeof(UnityEngine.Object), new EditorDescriptor(2, true, true) },
				{ typeof(System.Boolean), new EditorDescriptor(3, true, true) },
				{ typeof(System.Enum), new EditorDescriptor(4, true, true) },
				{ typeof(System.Collections.Generic.List<>), new EditorDescriptor(5, true, true) },
				{ typeof(System.Array), new EditorDescriptor(6, true, true) },
				{ typeof(System.String), new EditorDescriptor(7, true, true) },
				{ typeof(System.Int32), new EditorDescriptor(8, true, true) },
				{ typeof(System.Single), new EditorDescriptor(9, true, true) },
				{ typeof(Range), new EditorDescriptor(10, true, true) },
				{ typeof(UnityEngine.Vector2), new EditorDescriptor(11, true, true) },
				{ typeof(UnityEngine.Vector3), new EditorDescriptor(12, true, true) },
				{ typeof(UnityEngine.Vector4), new EditorDescriptor(13, true, true) },
				{ typeof(UnityEngine.Quaternion), new EditorDescriptor(14, true, true) },
				{ typeof(UnityEngine.Color), new EditorDescriptor(15, true, true) },
				{ typeof(UnityEngine.Bounds), new EditorDescriptor(16, true, true) },
				{ typeof(RangeInt), new EditorDescriptor(17, true, true) },
				{ typeof(RangeOptions), new EditorDescriptor(18, true, true) },
				{ typeof(HeaderText), new EditorDescriptor(19, true, true) },
				{ typeof(System.Reflection.MethodInfo), new EditorDescriptor(20, true, true) },
				{ typeof(UnityEngine.Component), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.BoxCollider), new EditorDescriptor(22, true, false) },
				{ typeof(UnityEngine.Camera), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.CapsuleCollider), new EditorDescriptor(22, true, false) },
				{ typeof(UnityEngine.FixedJoint), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.HingeJoint), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.Light), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.MeshCollider), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.MeshFilter), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.MeshRenderer), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.MonoBehaviour), new EditorDescriptor(21, false, false) },
				{ typeof(UnityEngine.Rigidbody), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.SkinnedMeshRenderer), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.Skybox), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.SphereCollider), new EditorDescriptor(22, true, false) },
				{ typeof(UnityEngine.SpringJoint), new EditorDescriptor(21, true, false) },
				{ typeof(UnityEngine.Transform), new EditorDescriptor(23, true, false) },
				{ typeof(RuntimeAnimation), new EditorDescriptor(21, true, false) },
			};
		}
	}
}
