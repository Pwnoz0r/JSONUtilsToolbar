using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamasoft.JsonClassGenerator.CodeWriters;
using Xamasoft.JsonClassGenerator.Properties;

namespace Xamasoft.JsonClassGenerator
{
	public class JsonClassGenerator : IJsonClassGeneratorConfig
	{
		private readonly PluralizationService _pluralizationService = PluralizationService.CreateService(new CultureInfo("en-us"));

		private bool _used;

		private readonly HashSet<string> _names = new HashSet<string>();

		public static readonly string[] FileHeader;
	    public string GetDirectoryName { get; private set; }

	    public bool AlwaysUseNullableValues
		{
			get;
			set;
		}

		public bool ApplyObfuscationAttributes
		{
			get;
			set;
		}

		public ICodeWriter CodeWriter
		{
			get;
			set;
		}

		public string Example
		{
			get;
			set;
		}

		public bool ExamplesInDocumentation
		{
			get;
			set;
		}

		public bool ExplicitDeserialization
		{
			get;
			set;
		}

		public bool HasSecondaryClasses => Types.Count > 1;

	    public bool InternalVisibility
		{
			get;
			set;
		}

		public string MainClass
		{
			get;
			set;
		}

		public string Namespace
		{
			get;
			set;
		}

		public bool NoHelperClass
		{
			get;
			set;
		}

		public TextWriter OutputStream
		{
			get;
			set;
		}

		public string SecondaryNamespace
		{
			get;
			set;
		}

		public bool SingleFile
		{
			get;
			set;
		}

		public string TargetFolder
		{
			get;
			set;
		}

		public IList<JsonType> Types
		{
			get;
			private set;
		}

		public bool UseNamespaces => Namespace != null;

	    public bool UseNestedClasses
		{
			get;
			set;
		}

		public bool UsePascalCase
		{
			get;
			set;
		}

		public bool UseProperties
		{
			get;
			set;
		}

		static JsonClassGenerator()
		{
			FileHeader = new[] { $"Visual Studio Extension \"JsonUtilsToolbar\" created by Jonathan \"Pwnoz0r\" Rainier - Initial Servers LLC. - {DateTime.Now.Year}", "Generated using Xamasoft JSON Class Generator Lib - http://www.xamasoft.com/json-class-generator" };
		}

	    private string CreateUniqueClassName(string name)
		{
			name = ToTitleCase(name);
			return name;
		}

		private string CreateUniqueClassNameFromPlural(string plural)
		{
			plural = ToTitleCase(plural);
			return CreateUniqueClassName(_pluralizationService.Singularize(plural));
		}

		private void GenerateClass(JObject[] examples, JsonType type)
		{
		    JToken jTokens1;
			object obj;
			var strs = new Dictionary<string, JsonType>();
			var objs = new Dictionary<string, IList<object>>();
			var flag = true;
			var jObjectArrays = examples;
			for (var i = 0; i < jObjectArrays.Length; i++)
			{
				foreach (var jProperty in jObjectArrays[i].Properties())
				{
					var jsonType1 = new JsonType(this, jProperty.Value);
					var name = jProperty.Name;
				    JsonType jsonType;
				    if (!strs.TryGetValue(name, out jsonType))
					{
						var jsonType2 = jsonType1;
						jsonType2 = !flag ? jsonType2.GetCommonType(JsonType.GetNull(this)) : jsonType2.MaybeMakeNullable(this);
						strs.Add(name, jsonType2);
						objs[name] = new List<object>();
					}
					else
					{
						strs[name] = jsonType.GetCommonType(jsonType1);
					}
					var item = objs[name];
					var value = jProperty.Value;
					if (value.Type == JTokenType.Null ? false : value.Type != JTokenType.Undefined)
					{
						if (value.Type == JTokenType.Array || value.Type == JTokenType.Object)
						{
							obj = value;
						}
						else
						{
							obj = value.Value<object>();
						}
						var obj1 = obj;
						if (!item.Any(x => obj1.Equals(x)))
						{
							item.Add(obj1);
						}
					}
					else if (!item.Contains(null))
					{
						item.Insert(0, null);
					}
				}
				flag = false;
			}
			if (UseNestedClasses)
			{
				foreach (var str in strs)
				{
					_names.Add(str.Key.ToLower());
				}
			}
			foreach (var keyValuePair in strs)
			{
				var value1 = keyValuePair.Value;
				if (value1.Type == JsonTypeEnum.Object)
				{
					var jObjects = new List<JObject>(examples.Length);
					var jObjectArrays1 = examples;
					for (var j = 0; j < jObjectArrays1.Length; j++)
					{
					    JToken jTokens;
					    if (jObjectArrays1[j].TryGetValue(keyValuePair.Key, out jTokens))
						{
							if (jTokens.Type == JTokenType.Object)
							{
								jObjects.Add((JObject)jTokens);
							}
						}
					}
				    value1.AssignName(CreateUniqueClassName(keyValuePair.Key));
					GenerateClass(jObjects.ToArray(), value1);
				}
				if (value1.InternalType != null && value1.InternalType.Type == JsonTypeEnum.Object)
				{
					var jObjects1 = new List<JObject>(examples.Length);
					var jObjectArrays2 = examples;
					for (var k = 0; k < jObjectArrays2.Length; k++)
					{
						if (jObjectArrays2[k].TryGetValue(keyValuePair.Key, out jTokens1))
						{
							if (jTokens1.Type == JTokenType.Array)
							{
								foreach (var jTokens2 in (JArray)jTokens1)
								{
									if (!(jTokens2 is JObject))
									{
										throw new NotSupportedException("Arrays of non-objects are not supported yet.");
									}
									jObjects1.Add((JObject)jTokens2);
								}
							}
							else if (jTokens1.Type == JTokenType.Object)
							{
								foreach (var keyValuePair1 in (JObject)jTokens1)
								{
									if (!(keyValuePair1.Value is JObject))
									{
										throw new NotSupportedException("Arrays of non-objects are not supported yet.");
									}
									jObjects1.Add((JObject)keyValuePair1.Value);
								}
							}
						}
					}
					keyValuePair.Value.InternalType.AssignName(CreateUniqueClassNameFromPlural(keyValuePair.Key));
					GenerateClass(jObjects1.ToArray(), keyValuePair.Value.InternalType);
				}
			}
			type.Fields = (
				from x in strs
				select new FieldInfo(this, x.Key, x.Value, UsePascalCase, objs[x.Key])).ToArray();
			Types.Add(type);
		}

		public void GenerateClasses()
		{
			JObject[] array;
			if (CodeWriter == null)
			{
				CodeWriter = new CSharpCodeWriter();
			}
			if (ExplicitDeserialization && !(CodeWriter is CSharpCodeWriter))
			{
				throw new ArgumentException("Explicit deserialization is obsolete and is only supported by the C# provider.");
			}
			if (_used)
			{
				throw new InvalidOperationException("This instance of JsonClassGenerator has already been used. Please create a new instance.");
			}
			_used = true;
			var targetFolder = TargetFolder != null;
			if (targetFolder && !Directory.Exists(TargetFolder))
			{
				Directory.CreateDirectory(TargetFolder);
			}
			using (var stringReader = new StringReader(Example.StartsWith("HTTP/") ? Example.Substring(Example.IndexOf("\r\n\r\n", StringComparison.Ordinal)) : Example))
			{
				using (var jsonTextReader = new JsonTextReader(stringReader))
				{
					var jTokens = JToken.ReadFrom(jsonTextReader);
					if (!(jTokens is JArray))
					{
						if (!(jTokens is JObject))
						{
							throw new Exception("Sample JSON must be either a JSON array, or a JSON object.");
						}
						array = new[] { (JObject)jTokens };
					}
					else
					{
						array = ((JArray)jTokens).Cast<JObject>().ToArray();
					}
				}
			}
			Types = new List<JsonType>();
			_names.Add(MainClass);
			var jsonType = new JsonType(this, array[0])
			{
				IsRoot = false
			};
			jsonType.AssignName(MainClass);
			GenerateClass(array, jsonType);
			if (targetFolder)
			{
			    GetDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			    if (!NoHelperClass && ExplicitDeserialization)
				{
					File.WriteAllBytes(Path.Combine(TargetFolder, "JsonClassHelper.cs"), Resources.JsonClassHelper);
				}
				if (!SingleFile)
				{
					foreach (var type in Types)
					{
						var str = TargetFolder;
						if (!UseNestedClasses && !type.IsRoot && SecondaryNamespace != null)
						{
							var secondaryNamespace = SecondaryNamespace;
							if (secondaryNamespace.StartsWith(string.Concat(Namespace, ".")))
							{
								secondaryNamespace = secondaryNamespace.Substring(Namespace.Length + 1);
							}
							str = Path.Combine(str, secondaryNamespace);
							Directory.CreateDirectory(str);
						}
						WriteClassesToFile(Path.Combine(str, string.Concat(!UseNestedClasses || type.IsRoot ? string.Empty : string.Concat(MainClass, "."), type.AssignedName, CodeWriter.FileExtension)), new[] { type });
					}
				}
				else
				{
					WriteClassesToFile(Path.Combine(TargetFolder, string.Concat(MainClass, CodeWriter.FileExtension)), Types);
				}
			}
			else if (OutputStream != null)
			{
				WriteClassesToFile(OutputStream, Types);
			}
		}

		internal static string ToTitleCase(string str)
		{
			var stringBuilder = new StringBuilder(str.Length);
			var flag = true;
			for (var i = 0; i < str.Length; i++)
			{
				var chr = str[i];
				if (!char.IsLetterOrDigit(chr))
				{
					flag = true;
				}
				else
				{
					stringBuilder.Append(flag ? char.ToUpper(chr) : chr);
					flag = false;
				}
			}
			return stringBuilder.ToString();
		}

		private void WriteClassesToFile(string path, IEnumerable<JsonType> types)
		{
			using (var streamWriter = new StreamWriter(path, false, Encoding.UTF8))
			{
				WriteClassesToFile(streamWriter, types);
			}
		}

		private void WriteClassesToFile(TextWriter sw, IEnumerable<JsonType> types)
		{
			var flag = false;
			var isRoot = false;
			CodeWriter.WriteFileStart(this, sw);
			foreach (var type in types)
			{
				if (!(UseNamespaces & flag) || isRoot == type.IsRoot ? false : SecondaryNamespace != null)
				{
					CodeWriter.WriteNamespaceEnd(this, sw, isRoot);
					flag = false;
				}
				if (!UseNamespaces ? false : !flag)
				{
					CodeWriter.WriteNamespaceStart(this, sw, type.IsRoot);
					flag = true;
					isRoot = type.IsRoot;
				}
				CodeWriter.WriteClass(this, sw, type);
			}
			if (UseNamespaces & flag)
			{
				CodeWriter.WriteNamespaceEnd(this, sw, isRoot);
			}
			CodeWriter.WriteFileEnd(this, sw);
		}
	}
}