using System;
using System.IO;

namespace Xamasoft.JsonClassGenerator.CodeWriters
{
	public class JavaCodeWriter : ICodeWriter
	{
		public string DisplayName => "Java";

	    public string FileExtension => ".java";

	    public string GetTypeName(JsonType type, IJsonClassGeneratorConfig config)
		{
			throw new NotImplementedException();
		}

		public void WriteClass(IJsonClassGeneratorConfig config, TextWriter sw, JsonType type)
		{
			throw new NotImplementedException();
		}

		public void WriteFileEnd(IJsonClassGeneratorConfig config, TextWriter sw)
		{
			throw new NotImplementedException();
		}

		public void WriteFileStart(IJsonClassGeneratorConfig config, TextWriter sw)
		{
			var fileHeader = JsonClassGenerator.FileHeader;
			for (var i = 0; i < fileHeader.Length; i++)
			{
				sw.WriteLine(string.Concat("// ", fileHeader[i]));
			}
		}

		public void WriteNamespaceEnd(IJsonClassGeneratorConfig config, TextWriter sw, bool root)
		{
			throw new NotImplementedException();
		}

		public void WriteNamespaceStart(IJsonClassGeneratorConfig config, TextWriter sw, bool root)
		{
			throw new NotImplementedException();
		}
	}
}