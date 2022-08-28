using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Linq;
using System.Collections.Generic;

namespace KLabs.VRC.Extentions
{
	[CustomEditor(typeof(VRCExpressionsJsonImporter))]
	public class VRCExpressionsJsonImporterEditor : ScriptedImporterEditor
	{
		private KeyValuePair<string, VRCExpressionsMenu>[] subAssets = new KeyValuePair<string, VRCExpressionsMenu>[0];


		public override void OnInspectorGUI()
		{
			var target = (VRCExpressionsJsonImporter)this.target;
			for (int i = 0; i < subAssets.Length; i++)
			{
				var item = subAssets[i];
				var changeValue = EditorGUILayout.ObjectField(item.Key, item.Value, typeof(VRCExpressionsMenu), false) as VRCExpressionsMenu;
				if (changeValue != item.Value)
				{
					var pair = new VRCExpressionsJsonImporter.NameMenuPair() { Name = item.Key, Menu = changeValue };
					var index = target.XrefsMenu.FindIndex(e => e.Name == item.Key);
					if (index == -1)
					{
						target.XrefsMenu.Add(pair);
					}
					else
					{
						target.XrefsMenu[index] = pair;
					}
					subAssets[i] = new KeyValuePair<string, VRCExpressionsMenu>(item.Key, changeValue);
				}
			}
			ApplyRevertGUI();
		}

		protected override void Apply()
		{
			var target = (VRCExpressionsJsonImporter)this.target;
			foreach (var item in target.XrefsMenu)
			{
				if (item.Menu == null)
				{
					target.RemoveRemap(new AssetImporter.SourceAssetIdentifier(typeof(VRCExpressionsMenu), item.Name));
				}
				else
				{
					target.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(VRCExpressionsMenu), item.Name), item.Menu);
				}
			}
			target.XrefsMenu = target.XrefsMenu.Where(e => e.Menu != null).ToList();
			base.Apply();

			AssetDatabase.WriteImportSettingsIfDirty(target.assetPath);
			AssetDatabase.ImportAsset(target.assetPath, ImportAssetOptions.ForceUpdate);
		}

		public override void OnEnable()
		{
			base.OnEnable();
			var target = (VRCExpressionsJsonImporter)this.target;
			var tempAssets = AssetDatabase.LoadAllAssetsAtPath(target.assetPath);
			var view = new Dictionary<string, VRCExpressionsMenu>();
			foreach (var item in target.MenuLabels)
			{
				view[item] = null;
			}
			foreach (var item in tempAssets)
			{
				if (item is VRCExpressionsMenu && AssetDatabase.IsSubAsset(item))
				{
					view[item.name] = null;
				}
			}
			var keys = view.Keys.ToArray();
			foreach (var key in keys)
			{
				var p = target.XrefsMenu.Find(e => e.Name == key);
				view[key] = p.Name != null ? p.Menu : null;
			}
			subAssets = view.ToArray();
		}

		public override void OnDisable()
		{
			base.OnDisable();
		}
	}
}
