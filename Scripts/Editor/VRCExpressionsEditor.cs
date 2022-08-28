using System.Linq;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Collections.Generic;

namespace KLabs.VRC.Extentions.Editor
{
	[CustomEditor(typeof(VRCExpressions))]
	class ExpressionsEditor : UnityEditor.Editor
	{
		public VRCExpressionsMenu AddSubAssetSource { get; set; }

		public override void OnInspectorGUI()
		{
			var target = (VRCExpressions)this.target;
			EditorGUILayout.ObjectField("Parameters", target.Parameters, typeof(VRCExpressionParameters), false);

			for (int i = 0; i < target.Menu.Length; i++)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.ObjectField($"Menu[{i}]", target.Menu[i], typeof(VRCExpressionsMenu), false);
				}
			}
		}

		private static VRCExpressionsMenuComparer comparer = new VRCExpressionsMenuComparer();

		private sealed class VRCExpressionsMenuComparer : IComparer<VRCExpressionsMenu>
		{
			public int Compare(VRCExpressionsMenu x, VRCExpressionsMenu y)
			{
				return x.name.CompareTo(y.name);
			}
		}
	}
}
