using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamasoft.JsonClassGenerator
{
	public class JsonType
	{
		private readonly IJsonClassGeneratorConfig _generator;

		public string AssignedName
		{
			get;
			private set;
		}

		public IList<FieldInfo> Fields
		{
			get;
			internal set;
		}

		public JsonType InternalType
		{
			get;
			private set;
		}

		public bool IsRoot
		{
			get;
			internal set;
		}

		public bool MustCache
		{
			get
			{
				bool flag;
				var type = Type;
				if (type == JsonTypeEnum.Anything)
				{
					flag = true;
				}
				else
				{
					switch (type)
					{
						case JsonTypeEnum.Object:
						{
							flag = true;
							break;
						}
						case JsonTypeEnum.Array:
						{
							flag = true;
							break;
						}
						case JsonTypeEnum.Dictionary:
						{
							flag = true;
							break;
						}
						case JsonTypeEnum.NullableSomething:
						{
							flag = false;
							break;
						}
						case JsonTypeEnum.NonConstrained:
						{
							flag = true;
							break;
						}
						default:
						{
							goto case JsonTypeEnum.NullableSomething;
						}
					}
				}
				return flag;
			}
		}

		public JsonTypeEnum Type
		{
			get;
			private set;
		}

		private JsonType(IJsonClassGeneratorConfig generator)
		{
			_generator = generator;
		}

		public JsonType(IJsonClassGeneratorConfig generator, JToken token) : this(generator)
		{
			Type = GetFirstTypeEnum(token);
			if (Type == JsonTypeEnum.Array)
			{
				var jArrays = (JArray)token;
				InternalType = GetCommonType(generator, jArrays.ToArray());
			}
		}

		internal JsonType(IJsonClassGeneratorConfig generator, JsonTypeEnum type) : this(generator)
		{
			Type = type;
		}

		public void AssignName(string name)
		{
			AssignedName = name;
		}

		public static JsonType GetCommonType(IJsonClassGeneratorConfig generator, JToken[] tokens)
		{
			JsonType jsonType;
			if (tokens.Length != 0)
			{
				var commonType = new JsonType(generator, tokens[0]).MaybeMakeNullable(generator);
				for (var i = 1; i < tokens.Length; i++)
				{
					var jsonType1 = new JsonType(generator, tokens[i]);
					commonType = commonType.GetCommonType(jsonType1);
				}
				jsonType = commonType;
			}
			else
			{
				jsonType = new JsonType(generator, JsonTypeEnum.NonConstrained);
			}
			return jsonType;
		}

		public JsonType GetCommonType(JsonType type2)
		{
			JsonType jsonType;
			var commonTypeEnum = GetCommonTypeEnum(Type, type2.Type);
			if (commonTypeEnum == JsonTypeEnum.Array)
			{
			    if (type2.Type == JsonTypeEnum.NullableSomething)
				{
					jsonType = this;
					return jsonType;
				}
			    if (Type != JsonTypeEnum.NullableSomething)
			    {
			        var jsonType1 = InternalType.GetCommonType(type2.InternalType).MaybeMakeNullable(_generator);
			        if (jsonType1 != InternalType)
			        {
			            var jsonType2 = new JsonType(_generator, JsonTypeEnum.Array)
			            {
			                InternalType = jsonType1
			            };
			            jsonType = jsonType2;
			            return jsonType;
			        }
			    }
			    else
			    {
			        jsonType = type2;
			        return jsonType;
			    }
			}
		    jsonType = Type != commonTypeEnum ? new JsonType(_generator, commonTypeEnum).MaybeMakeNullable(_generator) : this;
			return jsonType;
		}

		private JsonTypeEnum GetCommonTypeEnum(JsonTypeEnum type1, JsonTypeEnum type2)
		{
			JsonTypeEnum jsonTypeEnum;
			if (type1 == JsonTypeEnum.NonConstrained)
			{
				jsonTypeEnum = type2;
			}
			else if (type2 != JsonTypeEnum.NonConstrained)
			{
				switch (type1)
				{
					case JsonTypeEnum.String:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = type1;
							return jsonTypeEnum;
						}
				        if (type2 != JsonTypeEnum.String)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.Boolean:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = JsonTypeEnum.NullableBoolean;
							return jsonTypeEnum;
						}
				        if (type2 != JsonTypeEnum.Boolean)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.Integer:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = JsonTypeEnum.NullableInteger;
							return jsonTypeEnum;
						}
				        switch (type2)
				        {
				            case JsonTypeEnum.Float:
				                jsonTypeEnum = JsonTypeEnum.Float;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Long:
				                jsonTypeEnum = JsonTypeEnum.Long;
				                return jsonTypeEnum;
				        }
				        if (type2 != JsonTypeEnum.Integer)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.Long:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = JsonTypeEnum.NullableLong;
							return jsonTypeEnum;
						}
				        if (type2 == JsonTypeEnum.Float)
				        {
				            jsonTypeEnum = JsonTypeEnum.Float;
				            return jsonTypeEnum;
				        }
				        if (type2 != JsonTypeEnum.Integer)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.Float:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = JsonTypeEnum.NullableFloat;
							return jsonTypeEnum;
						}
				        switch (type2)
				        {
				            case JsonTypeEnum.Float:
				                jsonTypeEnum = type1;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Integer:
				                jsonTypeEnum = type1;
				                return jsonTypeEnum;
				        }
				        if (type2 != JsonTypeEnum.Long)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.Date:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = JsonTypeEnum.NullableDate;
							return jsonTypeEnum;
						}
				        if (type2 != JsonTypeEnum.Date)
				        {
				            break;
				        }
				        jsonTypeEnum = JsonTypeEnum.Date;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.NullableInteger:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = type1;
							return jsonTypeEnum;
						}
				        switch (type2)
				        {
				            case JsonTypeEnum.Float:
				                jsonTypeEnum = JsonTypeEnum.NullableFloat;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Long:
				                jsonTypeEnum = JsonTypeEnum.NullableLong;
				                return jsonTypeEnum;
				        }
				        if (type2 != JsonTypeEnum.Integer)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.NullableLong:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = type1;
							return jsonTypeEnum;
						}
				        switch (type2)
				        {
				            case JsonTypeEnum.Float:
				                jsonTypeEnum = JsonTypeEnum.NullableFloat;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Integer:
				                jsonTypeEnum = type1;
				                return jsonTypeEnum;
				        }
				        if (type2 != JsonTypeEnum.Long)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.NullableFloat:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = type1;
							return jsonTypeEnum;
						}
				        switch (type2)
				        {
				            case JsonTypeEnum.Float:
				                jsonTypeEnum = type1;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Integer:
				                jsonTypeEnum = type1;
				                return jsonTypeEnum;
				        }
				        if (type2 != JsonTypeEnum.Long)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.NullableBoolean:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = type1;
							return jsonTypeEnum;
						}
				        if (type2 != JsonTypeEnum.Boolean)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.NullableDate:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = type1;
							return jsonTypeEnum;
						}
				        if (type2 != JsonTypeEnum.Date)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.Object:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = type1;
							return jsonTypeEnum;
						}
				        if (type2 != JsonTypeEnum.Object)
				        {
				            if (type2 == JsonTypeEnum.Dictionary)
				            {
				                throw new ArgumentException();
				            }
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.Array:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = type1;
							return jsonTypeEnum;
						}
				        if (type2 != JsonTypeEnum.Array)
				        {
				            break;
				        }
				        jsonTypeEnum = type1;
				        return jsonTypeEnum;
				    }
				    case JsonTypeEnum.Dictionary:
					{
						throw new ArgumentException();
					}
					case JsonTypeEnum.NullableSomething:
				    {
				        if (IsNull(type2))
						{
							jsonTypeEnum = type1;
							return jsonTypeEnum;
						}
				        switch (type2)
				        {
				            case JsonTypeEnum.String:
				                jsonTypeEnum = JsonTypeEnum.String;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Integer:
				                jsonTypeEnum = JsonTypeEnum.NullableInteger;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Float:
				                jsonTypeEnum = JsonTypeEnum.NullableFloat;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Long:
				                jsonTypeEnum = JsonTypeEnum.NullableLong;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Boolean:
				                jsonTypeEnum = JsonTypeEnum.NullableBoolean;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Date:
				                jsonTypeEnum = JsonTypeEnum.NullableDate;
				                return jsonTypeEnum;
				            case JsonTypeEnum.Array:
				                jsonTypeEnum = JsonTypeEnum.Array;
				                return jsonTypeEnum;
				        }
				        if (type2 != JsonTypeEnum.Object)
				        {
				            break;
				        }
				        jsonTypeEnum = JsonTypeEnum.Object;
				        return jsonTypeEnum;
				    }
				}
				jsonTypeEnum = JsonTypeEnum.Anything;
			}
			else
			{
				jsonTypeEnum = type1;
			}
			return jsonTypeEnum;
		}

		private static JsonTypeEnum GetFirstTypeEnum(JToken token)
		{
			JsonTypeEnum jsonTypeEnum;
			var type = token.Type;
			if (type != JTokenType.Integer)
			{
				switch (type)
				{
					case JTokenType.Object:
					{
						jsonTypeEnum = JsonTypeEnum.Object;
						break;
					}
					case JTokenType.Array:
					{
						jsonTypeEnum = JsonTypeEnum.Array;
						break;
					}
					case JTokenType.Constructor:
					case JTokenType.Property:
					case JTokenType.Comment:
					case JTokenType.Integer:
					{
						jsonTypeEnum = JsonTypeEnum.Anything;
						break;
					}
					case JTokenType.Float:
					{
						jsonTypeEnum = JsonTypeEnum.Float;
						break;
					}
					case JTokenType.String:
					{
						jsonTypeEnum = JsonTypeEnum.String;
						break;
					}
					case JTokenType.Boolean:
					{
						jsonTypeEnum = JsonTypeEnum.Boolean;
						break;
					}
					case JTokenType.Null:
					{
						jsonTypeEnum = JsonTypeEnum.NullableSomething;
						break;
					}
					case JTokenType.Undefined:
					{
						jsonTypeEnum = JsonTypeEnum.NullableSomething;
						break;
					}
					case JTokenType.Date:
					{
						jsonTypeEnum = JsonTypeEnum.Date;
						break;
					}
					default:
					{
						goto case JTokenType.Integer;
					}
				}
			}
			else
			{
				jsonTypeEnum = (long)((JValue)token).Value >= (long)2147483647 ? JsonTypeEnum.Long : JsonTypeEnum.Integer;
			}
			return jsonTypeEnum;
		}

		public JsonType GetInnermostType()
		{
		    if (Type != JsonTypeEnum.Array)
			{
				throw new InvalidOperationException();
			}
			var jsonType = InternalType.Type == JsonTypeEnum.Array ? InternalType.GetInnermostType() : InternalType;
			return jsonType;
		}

		public string GetJTokenType()
		{
			string str;
			switch (Type)
			{
				case JsonTypeEnum.String:
				case JsonTypeEnum.Boolean:
				case JsonTypeEnum.Integer:
				case JsonTypeEnum.Long:
				case JsonTypeEnum.Float:
				case JsonTypeEnum.Date:
				case JsonTypeEnum.NullableInteger:
				case JsonTypeEnum.NullableLong:
				case JsonTypeEnum.NullableFloat:
				case JsonTypeEnum.NullableBoolean:
				case JsonTypeEnum.NullableDate:
				{
					str = "JValue";
					break;
				}
				case JsonTypeEnum.Object:
				{
					str = "JObject";
					break;
				}
				case JsonTypeEnum.Array:
				{
					str = "JArray";
					break;
				}
				case JsonTypeEnum.Dictionary:
				{
					str = "JObject";
					break;
				}
				default:
				{
					str = "JToken";
					break;
				}
			}
			return str;
		}

		internal static JsonType GetNull(IJsonClassGeneratorConfig generator)
		{
			return new JsonType(generator, JsonTypeEnum.NullableSomething);
		}

		public string GetReaderName()
		{
			string str;
			if (Type == JsonTypeEnum.Anything || Type == JsonTypeEnum.NullableSomething || Type == JsonTypeEnum.NonConstrained)
			{
				str = "ReadObject";
			}
			else if (Type != JsonTypeEnum.Object)
			{
				str = Type != JsonTypeEnum.Array ? $"Read{Enum.GetName(typeof(JsonTypeEnum), Type)}"
				    : $"ReadArray<{InternalType.GetTypeName()}>";
			}
			else
			{
				str = $"ReadStronglyTypedObject<{AssignedName}>";
			}
			return str;
		}

		public string GetTypeName()
		{
			return _generator.CodeWriter.GetTypeName(this, _generator);
		}

		private static bool IsNull(JsonTypeEnum type)
		{
			return type == JsonTypeEnum.NullableSomething;
		}

	    internal JsonType MaybeMakeNullable(IJsonClassGeneratorConfig generator)
	    {
	        var jsonType = generator.AlwaysUseNullableValues ? GetCommonType(GetNull(generator)) : this;
	        return jsonType;
	    }
	}
}