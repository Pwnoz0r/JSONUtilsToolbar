using System;
using System.IO;

namespace Xamasoft.JsonClassGenerator.CodeWriters
{
	public class CSharpCodeWriter : ICodeWriter
	{
	    public static string NoRenameAttribute { get; } = "[Obfuscation(Feature = \"renaming\", Exclude = true)]";

	    public static string NoPruneAttribute { get; } = "[Obfuscation(Feature = \"trigger\", Exclude = false)]";

	    public string DisplayName => "C#";

	    public string FileExtension => ".cs";

	    public string GetTypeName(JsonType type, IJsonClassGeneratorConfig config)
		{
			string assignedName;
			var explicitDeserialization = !config.ExplicitDeserialization;
			switch (type.Type)
			{
				case JsonTypeEnum.Anything:
				{
					assignedName = "object";
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
				{
					assignedName = "int";
					break;
				}
				case JsonTypeEnum.Long:
				{
					assignedName = "long";
					break;
				}
				case JsonTypeEnum.Float:
				{
					assignedName = "double";
					break;
				}
				case JsonTypeEnum.Date:
				{
					assignedName = "DateTime";
					break;
				}
				case JsonTypeEnum.NullableInteger:
				{
					assignedName = "int?";
					break;
				}
				case JsonTypeEnum.NullableLong:
				{
					assignedName = "long?";
					break;
				}
				case JsonTypeEnum.NullableFloat:
				{
					assignedName = "double?";
					break;
				}
				case JsonTypeEnum.NullableBoolean:
				{
					assignedName = "bool?";
					break;
				}
				case JsonTypeEnum.NullableDate:
				{
					assignedName = "DateTime?";
					break;
				}
				case JsonTypeEnum.Object:
				{
					assignedName = type.AssignedName;
					break;
				}
				case JsonTypeEnum.Array:
				{
					assignedName = explicitDeserialization ? string.Concat("IList<", GetTypeName(type.InternalType, config), ">") : string.Concat(GetTypeName(type.InternalType, config), "[]");
					break;
				}
				case JsonTypeEnum.Dictionary:
				{
					assignedName = string.Concat("Dictionary<string, ", GetTypeName(type.InternalType, config), ">");
					break;
				}
				case JsonTypeEnum.NullableSomething:
				{
					assignedName = "object";
					break;
				}
				case JsonTypeEnum.NonConstrained:
				{
					assignedName = "object";
					break;
				}
				default:
				{
					throw new NotSupportedException("Unsupported json type");
				}
			}
			return assignedName;
		}

		private bool ShouldApplyNoPruneAttribute(IJsonClassGeneratorConfig config)
		{
			return !config.ApplyObfuscationAttributes || config.ExplicitDeserialization ? false : config.UseProperties;
		}

		private bool ShouldApplyNoRenamingAttribute(IJsonClassGeneratorConfig config)
		{
			return !config.ApplyObfuscationAttributes || config.ExplicitDeserialization ? false : !config.UsePascalCase;
		}

		public void WriteClass(IJsonClassGeneratorConfig config, TextWriter sw, JsonType type)
		{
			var str = config.InternalVisibility ? "internal" : "public";
			if (!config.UseNestedClasses)
			{
				if (ShouldApplyNoRenamingAttribute(config))
				{
					sw.WriteLine("    [Obfuscation(Feature = \"renaming\", Exclude = true)]");
				}
				if (ShouldApplyNoPruneAttribute(config))
				{
					sw.WriteLine("    [Obfuscation(Feature = \"trigger\", Exclude = false)]");
				}
				sw.WriteLine("    {0} class {1}", str, type.AssignedName);
				sw.WriteLine("    {");
			}
			else if (!type.IsRoot)
			{
				if (ShouldApplyNoRenamingAttribute(config))
				{
					sw.WriteLine("        [Obfuscation(Feature = \"renaming\", Exclude = true)]");
				}
				if (ShouldApplyNoPruneAttribute(config))
				{
					sw.WriteLine("        [Obfuscation(Feature = \"trigger\", Exclude = false)]");
				}
				sw.WriteLine("        {0} class {1}", str, type.AssignedName);
				sw.WriteLine("        {");
			}
			var str1 = !config.UseNestedClasses || type.IsRoot ? "        " : "            ";
			var flag = !config.InternalVisibility || config.UseProperties ? false : !config.ExplicitDeserialization;
			if (flag)
			{
				sw.WriteLine("#pragma warning disable 0649");
				if (!config.UsePascalCase)
				{
					sw.WriteLine();
				}
			}
			if (!type.IsRoot ? false : config.ExplicitDeserialization)
			{
				WriteStringConstructorExplicitDeserialization(config, sw, type, str1);
			}
			if (!config.ExplicitDeserialization)
			{
				WriteClassMembers(config, sw, type, str1);
			}
			else if (!config.UseProperties)
			{
				WriteClassWithFieldsExplicitDeserialization(sw, type, str1);
			}
			else
			{
				WriteClassWithPropertiesExplicitDeserialization(sw, type, str1);
			}
			if (flag)
			{
				sw.WriteLine();
				sw.WriteLine("#pragma warning restore 0649");
				sw.WriteLine();
			}
			if (!config.UseNestedClasses ? false : !type.IsRoot)
			{
				sw.WriteLine("        }");
			}
			if (!config.UseNestedClasses)
			{
				sw.WriteLine("    }");
			}
			sw.WriteLine();
		}

		private void WriteClassMembers(IJsonClassGeneratorConfig config, TextWriter sw, JsonType type, string prefix)
		{
			foreach (var field in type.Fields)
			{
				if (config.UsePascalCase ? true : config.ExamplesInDocumentation)
				{
					sw.WriteLine();
				}
				if (config.ExamplesInDocumentation)
				{
					sw.WriteLine(string.Concat(prefix, "/// <summary>"));
					sw.WriteLine(string.Concat(prefix, "/// Examples: ", field.GetExamplesText()));
					sw.WriteLine(string.Concat(prefix, "/// </summary>"));
				}
				if (config.UsePascalCase)
				{
					sw.WriteLine(string.Concat(prefix, "[JsonProperty(\"{0}\")]"), field.JsonMemberName);
				}
				if (!config.UseProperties)
				{
					sw.WriteLine(string.Concat(prefix, "public {0} {1};"), field.Type.GetTypeName(), field.MemberName);
				}
				else
				{
					sw.WriteLine(string.Concat(prefix, "public {0} {1} {{ get; set; }}"), field.Type.GetTypeName(), field.MemberName);
				}
			}
		}

		private void WriteClassWithFieldsExplicitDeserialization(TextWriter sw, JsonType type, string prefix)
		{
			sw.WriteLine(string.Concat(prefix, "public {0}(JObject obj)"), type.AssignedName);
			sw.WriteLine(string.Concat(prefix, "{"));
			foreach (var field in type.Fields)
			{
				sw.WriteLine(string.Concat(prefix, "    this.{0} = {1};"), field.MemberName, field.GetGenerationCode("obj"));
			}
			sw.WriteLine(string.Concat(prefix, "}"));
			sw.WriteLine();
			foreach (var fieldInfo in type.Fields)
			{
				sw.WriteLine(string.Concat(prefix, "public readonly {0} {1};"), fieldInfo.Type.GetTypeName(), fieldInfo.MemberName);
			}
		}

		private void WriteClassWithPropertiesExplicitDeserialization(TextWriter sw, JsonType type, string prefix)
		{
			sw.WriteLine(string.Concat(prefix, "private JObject __jobject;"));
			sw.WriteLine(string.Concat(prefix, "public {0}(JObject obj)"), type.AssignedName);
			sw.WriteLine(string.Concat(prefix, "{"));
			sw.WriteLine(string.Concat(prefix, "    this.__jobject = obj;"));
			sw.WriteLine(string.Concat(prefix, "}"));
			sw.WriteLine();
			foreach (var field in type.Fields)
			{
				string str = null;
				if (field.Type.MustCache)
				{
					var lower = char.ToLower(field.MemberName[0]);
					str = string.Concat("_", lower.ToString(), field.MemberName.Substring(1));
					sw.WriteLine(string.Concat(prefix, "[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]"));
					sw.WriteLine(string.Concat(prefix, "private {0} {1};"), field.Type.GetTypeName(), str);
				}
				sw.WriteLine(string.Concat(prefix, "public {0} {1}"), field.Type.GetTypeName(), field.MemberName);
				sw.WriteLine(string.Concat(prefix, "{"));
				sw.WriteLine(string.Concat(prefix, "    get"));
				sw.WriteLine(string.Concat(prefix, "    {"));
				if (!field.Type.MustCache)
				{
					sw.WriteLine(string.Concat(prefix, "        return {0};"), field.GetGenerationCode("__jobject"));
				}
				else
				{
					sw.WriteLine(string.Concat(prefix, "        if ({0} == null)"), str);
					sw.WriteLine(string.Concat(prefix, "            {0} = {1};"), str, field.GetGenerationCode("__jobject"));
					sw.WriteLine(string.Concat(prefix, "        return {0};"), str);
				}
				sw.WriteLine(string.Concat(prefix, "    }"));
				sw.WriteLine(string.Concat(prefix, "}"));
				sw.WriteLine();
			}
		}

		public void WriteFileEnd(IJsonClassGeneratorConfig config, TextWriter sw)
		{
			if (config.UseNestedClasses)
			{
				sw.WriteLine("    }");
			}
		}

		public void WriteFileStart(IJsonClassGeneratorConfig config, TextWriter sw)
		{
			if (config.UseNamespaces)
			{
				var fileHeader = JsonClassGenerator.FileHeader;
				for (var i = 0; i < fileHeader.Length; i++)
				{
					sw.WriteLine(string.Concat("// ", fileHeader[i]));
				}
				sw.WriteLine();
				sw.WriteLine("using System;");
				sw.WriteLine("using System.Collections.Generic;");
				if (ShouldApplyNoPruneAttribute(config) ? true : ShouldApplyNoRenamingAttribute(config))
				{
					sw.WriteLine("using System.Reflection;");
				}
				if (config.ExplicitDeserialization ? false : config.UsePascalCase)
				{
					sw.WriteLine("using Newtonsoft.Json;");
				}
				sw.WriteLine("using Newtonsoft.Json.Linq;");
				if (config.ExplicitDeserialization)
				{
					sw.WriteLine("using JsonCSharpClassGenerator;");
				}
				if (config.SecondaryNamespace == null || !config.HasSecondaryClasses ? false : !config.UseNestedClasses)
				{
					sw.WriteLine("using {0};", config.SecondaryNamespace);
				}
			}
			if (config.UseNestedClasses)
			{
				sw.WriteLine("    {0} class {1}", config.InternalVisibility ? "internal" : "public", config.MainClass);
				sw.WriteLine("    {");
			}
		}

		public void WriteNamespaceEnd(IJsonClassGeneratorConfig config, TextWriter sw, bool root)
		{
			sw.WriteLine("}");
		}

		public void WriteNamespaceStart(IJsonClassGeneratorConfig config, TextWriter sw, bool root)
		{
			sw.WriteLine();
			sw.WriteLine("namespace {0}", !root || config.UseNestedClasses ? config.SecondaryNamespace ?? config.Namespace : config.Namespace);
			sw.WriteLine("{");
			sw.WriteLine();
		}

		private void WriteStringConstructorExplicitDeserialization(IJsonClassGeneratorConfig config, TextWriter sw, JsonType type, string prefix)
		{
			sw.WriteLine();
			sw.WriteLine(string.Concat(prefix, "public {1}(string json)"), config.InternalVisibility ? "internal" : "public", type.AssignedName);
			sw.WriteLine(string.Concat(prefix, "    : this(JObject.Parse(json))"));
			sw.WriteLine(string.Concat(prefix, "{"));
			sw.WriteLine(string.Concat(prefix, "}"));
			sw.WriteLine();
		}
	}
}