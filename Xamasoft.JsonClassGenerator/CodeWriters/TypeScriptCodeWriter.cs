using System;
using System.IO;

namespace Xamasoft.JsonClassGenerator.CodeWriters
{
	public class TypeScriptCodeWriter : ICodeWriter
	{
		public string DisplayName => "TypeScript";

	    public string FileExtension => ".ts";

	    private string GetNamespace(IJsonClassGeneratorConfig config, bool root)
		{
			return root ? config.Namespace : config.SecondaryNamespace ?? config.Namespace;
		}

		public string GetTypeName(JsonType type, IJsonClassGeneratorConfig config)
		{
			string assignedName;
			switch (type.Type)
			{
				case JsonTypeEnum.Anything:
				{
					assignedName = "any";
					break;
				}
				case JsonTypeEnum.String:
				{
					assignedName = "string";
					break;
				}
				case JsonTypeEnum.Boolean:
				{
					assignedName = "bool";
					break;
				}
				case JsonTypeEnum.Integer:
				case JsonTypeEnum.Long:
				case JsonTypeEnum.Float:
				{
					assignedName = "number";
					break;
				}
				case JsonTypeEnum.Date:
				{
					assignedName = "Date";
					break;
				}
				case JsonTypeEnum.NullableInteger:
				case JsonTypeEnum.NullableLong:
				case JsonTypeEnum.NullableFloat:
				{
					assignedName = "number";
					break;
				}
				case JsonTypeEnum.NullableBoolean:
				{
					assignedName = "bool";
					break;
				}
				case JsonTypeEnum.NullableDate:
				{
					assignedName = "Date";
					break;
				}
				case JsonTypeEnum.Object:
				{
					assignedName = type.AssignedName;
					break;
				}
				case JsonTypeEnum.Array:
				{
					assignedName = string.Concat(GetTypeName(type.InternalType, config), "[]");
					break;
				}
				case JsonTypeEnum.Dictionary:
				{
					assignedName = string.Concat("{ [key: string]: ", GetTypeName(type.InternalType, config), "; }");
					break;
				}
				case JsonTypeEnum.NullableSomething:
				{
					assignedName = "any";
					break;
				}
				case JsonTypeEnum.NonConstrained:
				{
					assignedName = "any";
					break;
				}
				default:
				{
					throw new NotSupportedException("Unsupported type");
				}
			}
			return assignedName;
		}

		private bool IsNullable(JsonTypeEnum type)
		{
			return type == JsonTypeEnum.NullableBoolean || type == JsonTypeEnum.NullableDate || type == JsonTypeEnum.NullableFloat || type == JsonTypeEnum.NullableInteger || type == JsonTypeEnum.NullableLong ? true : type == JsonTypeEnum.NullableSomething;
		}

		public void WriteClass(IJsonClassGeneratorConfig config, TextWriter sw, JsonType type)
		{
			bool flag;
			var str = GetNamespace(config, type.IsRoot) != null ? "    " : "";
			var flag1 = !config.InternalVisibility ? true : config.SecondaryNamespace != null;
			var textWriter = sw;
			var assignedName = new[] { str, null, null, null, null };
			assignedName[1] = flag1 ? "export " : string.Empty;
			assignedName[2] = "interface ";
			assignedName[3] = type.AssignedName;
			assignedName[4] = " {";
			textWriter.WriteLine(string.Concat(assignedName));
			foreach (var field in type.Fields)
			{
				if (!type.IsRoot || config.SecondaryNamespace == null || config.Namespace == null)
				{
					flag = false;
				}
				else if (field.Type.Type == JsonTypeEnum.Object)
				{
					flag = true;
				}
				else
				{
					flag = field.Type.InternalType == null ? false : field.Type.InternalType.Type == JsonTypeEnum.Object;
				}
				var flag2 = flag;
				if (config.ExamplesInDocumentation)
				{
					sw.WriteLine();
					sw.WriteLine(string.Concat(str, "    /**"));
					sw.WriteLine(string.Concat(str, "      * Examples: ", field.GetExamplesText()));
					sw.WriteLine(string.Concat(str, "      */"));
				}
				var textWriter1 = sw;
				var jsonMemberName = new[] { str, "    ", field.JsonMemberName, null, null, null, null, null };
				jsonMemberName[3] = IsNullable(field.Type.Type) ? "?" : "";
				jsonMemberName[4] = ": ";
				jsonMemberName[5] = flag2 ? string.Concat(config.SecondaryNamespace, ".") : string.Empty;
				jsonMemberName[6] = GetTypeName(field.Type, config);
				jsonMemberName[7] = ";";
				textWriter1.WriteLine(string.Concat(jsonMemberName));
			}
			sw.WriteLine(string.Concat(str, "}"));
			sw.WriteLine();
		}

		public void WriteFileEnd(IJsonClassGeneratorConfig config, TextWriter sw)
		{
		}

		public void WriteFileStart(IJsonClassGeneratorConfig config, TextWriter sw)
		{
			var fileHeader = JsonClassGenerator.FileHeader;
			for (var i = 0; i < fileHeader.Length; i++)
			{
				sw.WriteLine(string.Concat("// ", fileHeader[i]));
			}
			sw.WriteLine();
		}

		public void WriteNamespaceEnd(IJsonClassGeneratorConfig config, TextWriter sw, bool root)
		{
			if (GetNamespace(config, root) != null)
			{
				sw.WriteLine("}");
				sw.WriteLine();
			}
		}

		public void WriteNamespaceStart(IJsonClassGeneratorConfig config, TextWriter sw, bool root)
		{
			if (GetNamespace(config, root) != null)
			{
				sw.WriteLine(string.Concat("module ", GetNamespace(config, root), " {"));
				sw.WriteLine();
			}
		}
	}
}