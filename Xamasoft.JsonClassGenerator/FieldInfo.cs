using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Xamasoft.JsonClassGenerator
{
	public class FieldInfo
	{
	    public IJsonClassGeneratorConfig Generator { get; }

	    public IList<object> Examples { get; }

		public string JsonMemberName { get; }

        public string MemberName { get; }

        public JsonType Type { get; }

        public FieldInfo(IJsonClassGeneratorConfig generator, string jsonMemberName, JsonType type, bool usePascalCase, IList<object> examples)
		{
			Generator = generator;
			JsonMemberName = jsonMemberName;
			MemberName = jsonMemberName;
			if (usePascalCase)
			{
				MemberName = JsonClassGenerator.ToTitleCase(MemberName);
			}
			Type = type;
			Examples = examples;
		}

		public string GetExamplesText()
		{
			var str = string.Join(", ", (
				from x in Examples.Take(5)
				select JsonConvert.SerializeObject(x)).ToArray());
			return str;
		}

		public string GetGenerationCode(string jobject)
		{
			string str;
			var fieldInfo = this;
			if (fieldInfo.Type.Type != JsonTypeEnum.Array)
			{
				str = fieldInfo.Type.Type != JsonTypeEnum.Dictionary ? string.Format("JsonClassHelper.{1}(JsonClassHelper.GetJToken<{2}>({0}, \"{3}\"))", jobject, fieldInfo.Type.GetReaderName(), fieldInfo.Type.GetJTokenType(), fieldInfo.JsonMemberName) : string.Format("({1})JsonClassHelper.ReadDictionary<{2}>(JsonClassHelper.GetJToken<JObject>({0}, \"{3}\"))", jobject, fieldInfo.Type.GetTypeName(), fieldInfo.Type.InternalType.GetTypeName(), fieldInfo.JsonMemberName, fieldInfo.Type.GetTypeName());
			}
			else
			{
				var innermostType = fieldInfo.Type.GetInnermostType();
				str = string.Format("({1})JsonClassHelper.ReadArray<{5}>(JsonClassHelper.GetJToken<JArray>({0}, \"{2}\"), JsonClassHelper.{3}, typeof({6}))", jobject, fieldInfo.Type.GetTypeName(), fieldInfo.JsonMemberName, innermostType.GetReaderName(), -1, innermostType.GetTypeName(), fieldInfo.Type.GetTypeName());
			}
			return str;
		}
	}
}