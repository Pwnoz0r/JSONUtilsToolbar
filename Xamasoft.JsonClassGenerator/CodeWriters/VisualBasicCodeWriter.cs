using System;
using System.IO;

namespace Xamasoft.JsonClassGenerator.CodeWriters
{
	public class VisualBasicCodeWriter : ICodeWriter
	{
	    public static string NoRenameAttribute { get; } = "<Obfuscation(Feature:=\"renaming\", Exclude:=true)>";

	    public static string NoPruneAttribute { get; } = "<Obfuscation(Feature:=\"trigger\", Exclude:=false)>";

	    public string DisplayName => "Visual Basic .NET";

	    public string FileExtension => ".vb";

	    public string GetTypeName(JsonType type, IJsonClassGeneratorConfig config)
		{
			string assignedName;
			var explicitDeserialization = config.ExplicitDeserialization;
			switch (type.Type)
			{
				case JsonTypeEnum.Anything:
				{
					assignedName = "Object";
					break;
				}
				case JsonTypeEnum.String:
				{
					assignedName = "String";
					break;
				}
				case JsonTypeEnum.Boolean:
				{
					assignedName = "Boolean";
					break;
				}
				case JsonTypeEnum.Integer:
				{
					assignedName = "Integer";
					break;
				}
				case JsonTypeEnum.Long:
				{
					assignedName = "Long";
					break;
				}
				case JsonTypeEnum.Float:
				{
					assignedName = "Double";
					break;
				}
				case JsonTypeEnum.Date:
				{
					assignedName = "DateTime";
					break;
				}
				case JsonTypeEnum.NullableInteger:
				{
					assignedName = "Integer?";
					break;
				}
				case JsonTypeEnum.NullableLong:
				{
					assignedName = "Long?";
					break;
				}
				case JsonTypeEnum.NullableFloat:
				{
					assignedName = "Double?";
					break;
				}
				case JsonTypeEnum.NullableBoolean:
				{
					assignedName = "Boolean?";
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
					assignedName = explicitDeserialization ? string.Concat("IList(Of ", GetTypeName(type.InternalType, config), ")") : string.Concat(GetTypeName(type.InternalType, config), "()");
					break;
				}
				case JsonTypeEnum.Dictionary:
				{
					assignedName = string.Concat("Dictionary(Of String, ", GetTypeName(type.InternalType, config), ")");
					break;
				}
				case JsonTypeEnum.NullableSomething:
				{
					assignedName = "Object";
					break;
				}
				case JsonTypeEnum.NonConstrained:
				{
					assignedName = "Object";
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
			var str = config.InternalVisibility ? "Friend" : "Public";
			if (!config.UseNestedClasses)
			{
				if (ShouldApplyNoRenamingAttribute(config))
				{
					sw.WriteLine("    <Obfuscation(Feature:=\"renaming\", Exclude:=true)>");
				}
				if (ShouldApplyNoPruneAttribute(config))
				{
					sw.WriteLine("    <Obfuscation(Feature:=\"trigger\", Exclude:=false)>");
				}
				sw.WriteLine("    {0} Class {1}", str, type.AssignedName);
			}
			else
			{
				sw.WriteLine("    {0} Partial Class {1}", str, config.MainClass);
				if (!type.IsRoot)
				{
					if (ShouldApplyNoRenamingAttribute(config))
					{
						sw.WriteLine("        <Obfuscation(Feature:=\"renaming\", Exclude:=true)>");
					}
					if (ShouldApplyNoPruneAttribute(config))
					{
						sw.WriteLine("        <Obfuscation(Feature:=\"trigger\", Exclude:=false)>");
					}
					sw.WriteLine("        {0} Class {1}", str, type.AssignedName);
				}
			}
			WriteClassMembers(config, sw, type, !config.UseNestedClasses || type.IsRoot ? "        " : "            ");
			if (!config.UseNestedClasses ? false : !type.IsRoot)
			{
				sw.WriteLine("        End Class");
			}
			sw.WriteLine("    End Class");
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
					sw.WriteLine(string.Concat(prefix, "''' <summary>"));
					sw.WriteLine(string.Concat(prefix, "''' Examples: ", field.GetExamplesText()));
					sw.WriteLine(string.Concat(prefix, "''' </summary>"));
				}
				if (config.UsePascalCase)
				{
					sw.WriteLine(string.Concat(prefix, "<JsonProperty(\"{0}\")>"), field.JsonMemberName);
				}
				if (!config.UseProperties)
				{
					sw.WriteLine(string.Concat(prefix, "Public {1} As {0}"), field.Type.GetTypeName(), field.MemberName);
				}
				else
				{
					sw.WriteLine(string.Concat(prefix, "Public Property {1} As {0}"), field.Type.GetTypeName(), field.MemberName);
				}
			}
		}

		public void WriteFileEnd(IJsonClassGeneratorConfig config, TextWriter sw)
		{
		}

		public void WriteFileStart(IJsonClassGeneratorConfig config, TextWriter sw)
		{
			var fileHeader = JsonClassGenerator.FileHeader;
			for (var i = 0; i < fileHeader.Length; i++)
			{
				sw.WriteLine(string.Concat("' ", fileHeader[i]));
			}
			sw.WriteLine();
			sw.WriteLine("Imports System");
			sw.WriteLine("Imports System.Collections.Generic");
			if (ShouldApplyNoRenamingAttribute(config) ? true : ShouldApplyNoPruneAttribute(config))
			{
				sw.WriteLine("Imports System.Reflection");
			}
			if (config.UsePascalCase)
			{
				sw.WriteLine("Imports Newtonsoft.Json");
			}
			sw.WriteLine("Imports Newtonsoft.Json.Linq");
			if (config.SecondaryNamespace == null || !config.HasSecondaryClasses ? false : !config.UseNestedClasses)
			{
				sw.WriteLine("Imports {0}", config.SecondaryNamespace);
			}
		}

		public void WriteNamespaceEnd(IJsonClassGeneratorConfig config, TextWriter sw, bool root)
		{
			sw.WriteLine("End Namespace");
		}

		public void WriteNamespaceStart(IJsonClassGeneratorConfig config, TextWriter sw, bool root)
		{
			sw.WriteLine();
			sw.WriteLine("Namespace Global.{0}", !root || config.UseNestedClasses ? config.SecondaryNamespace ?? config.Namespace : config.Namespace);
			sw.WriteLine();
		}
	}
}