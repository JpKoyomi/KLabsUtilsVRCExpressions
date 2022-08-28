using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using VRC.SDK3.Avatars.ScriptableObjects;
using Newtonsoft.Json;

using ValueType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;

namespace KLabs.VRC.Extentions
{
	[ScriptedImporter(1, "vrcexpjson")]
	public class VRCExpressionsJsonImporter : ScriptedImporter
	{
		[field: SerializeField]
		public List<NameMenuPair> XrefsMenu { get; set; } = new List<NameMenuPair>();

		[field: SerializeField]
		public string[] MenuLabels { get; private set; } = new string[0];

		public override void OnImportAsset(AssetImportContext ctx)
		{
			var vrcExps = ScriptableObject.CreateInstance<VRCExpressions>();
			ctx.AddObjectToAsset(nameof(VRCExpressions), vrcExps);
			ctx.SetMainObject(vrcExps);

			var vrcExpParams = ScriptableObject.CreateInstance<VRCExpressionParameters>();
			vrcExpParams.name = "Parameters";
			ctx.AddObjectToAsset("Parameters", vrcExpParams);
			vrcExps.Parameters = vrcExpParams;

			try
			{
				var json = Deserialize(ctx.assetPath);
				var paramDic = GenerateParameters(json.parameters);
				vrcExpParams.parameters = paramDic.Values.ToArray();

				if (json.menu != null)
				{
					var menuDic = GenerateMenu(json.menu, paramDic, out var subMenuCtrls);
					var menuLabels = new HashSet<string>();
					foreach (var item in subMenuCtrls)
					{
						menuLabels.Add(item.Value);
						if (GetExternalObjectMap().TryGetValue(new SourceAssetIdentifier(typeof(VRCExpressionsMenu), item.Value), out var map))
						{
							item.Key.subMenu = (VRCExpressionsMenu)map;
						}
						else if (menuDic.TryGetValue(item.Value, out var m))
						{
							item.Key.subMenu = m;
						}
					}
					MenuLabels = menuLabels.ToArray();
					foreach (var item in menuDic)
					{
						ctx.AddObjectToAsset(item.Key, item.Value);
					}
					vrcExps.Menu = menuDic.Values.ToArray();
				}


			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message, vrcExpParams);
			}
		}

		private static VRCExpressionJson Deserialize(string assetPath)
		{
			return JsonConvert.DeserializeObject<VRCExpressionJson>(System.IO.File.ReadAllText(assetPath));
		}

		private static VRCExpParamDic GenerateParameters(VRCExpressionJson.SerializedParameters parameters)
		{
			var paramsDic = new VRCExpParamDic();
			foreach (var item in parameters)
			{
				if (item.Value != null)
				{
					var p = item.Value;
					var valueType = (ValueType)System.Enum.Parse(typeof(ValueType), p.valueType);
					var type = p.valueType;
					var parameter = new VRCExpressionParameters.Parameter()
					{
						name = item.Key,
						valueType = valueType,
						defaultValue = p.defaultValue,
						saved = p.saved
					};
					paramsDic.Add(item.Key, parameter);
				}
			}
			return paramsDic;
		}

		private VRCExpMenuDic GenerateMenu(Dictionary<string, VRCExpressionJson.SerializedControl[]> src, VRCExpParamDic paramDic, out List<KeyValuePair<VRCExpressionsMenu.Control, string>> subMenuControls)
		{
			var dic = new VRCExpMenuDic();
			var subMenuCtrls = new List<KeyValuePair<VRCExpressionsMenu.Control, string>>();
			foreach (var item in src)
			{
				var menu = GenerateMenuItem(item.Key, item.Value, subMenuCtrls, paramDic);
				dic.Add(item.Key, menu);
			}
			subMenuControls = subMenuCtrls;
			return dic;
		}

		private VRCExpressionsMenu GenerateMenuItem(string name, VRCExpressionJson.SerializedControl[] controls, List<KeyValuePair<VRCExpressionsMenu.Control, string>> subMenuCtrls, VRCExpParamDic types)
		{
			var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
			menu.name = name;
			foreach (var item in controls)
			{
				var type = (ControlType)System.Enum.Parse(typeof(ControlType), item.type);
				var parameter = new VRCExpressionsMenu.Control.Parameter() { name = item.parameter ?? string.Empty };
				var value = item.value;
				if (types.TryGetValue(parameter.name, out var p))
				{
					value = p.valueType == ValueType.Bool ? 1.0f : value;
				}
				var control = new VRCExpressionsMenu.Control()
				{
					name = item.name,
					type = type,
					parameter = parameter,
					value = value
				};
				menu.controls.Add(control);
				if (control.type == ControlType.SubMenu)
				{
					subMenuCtrls.Add(new KeyValuePair<VRCExpressionsMenu.Control, string>(control, item.subMenu));
				}
			}
			return menu;
		}

		[System.Serializable]
		public sealed class VRCExpressionJson
		{
			public SerializedParameters parameters;
			public Dictionary<string, SerializedControl[]> menu;

			[System.Serializable]
			public class SerializedParameter
			{
				public string valueType;
				public float defaultValue;
				public bool saved;
			}

			[System.Serializable]
			public struct SerializedControl
			{
				public string name;
				//public Texture2D icon;
				public string type;
				public string parameter;
				public float value;
				// public Style style;
				public string subMenu;
				public string[] subParameters;
				// public Label[] labels; // Label[]
			}

			[System.Serializable]
			public sealed class SerializedParameters : Dictionary<string, SerializedParameter> { }
		}

		public sealed class VRCExpParamDic : Dictionary<string, VRCExpressionParameters.Parameter> { }

		public sealed class VRCExpMenuDic : Dictionary<string, VRCExpressionsMenu> { }

		[System.Serializable]
		public struct NameMenuPair
		{
			[field: SerializeField]
			public string Name { get; set; }

			[field: SerializeField]
			public VRCExpressionsMenu Menu { get; set; }
		}
	}
}
