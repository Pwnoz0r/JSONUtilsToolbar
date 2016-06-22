using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Xamasoft.JsonClassGenerator;
using Xamasoft.JsonClassGenerator.CodeWriters;

namespace JsonUtilsToolbar
{
    /// <summary>
    /// Interaction logic for JsonDialog.xaml
    /// </summary>
    public partial class JsonDialog
    {
        public JsonDialog(string currentNamespace)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(currentNamespace)) return;

            ProjectNamespace = currentNamespace;
            TextBoxNamespace.Text = currentNamespace;
        }

        public bool DoNewFile { get; set; }

        public string FormattedJsonModel { get; set; }

        public string ModelClassname { get; set; }

        public string ProjectNamespace { get; set; }

        private void ButtonGenerateJson_OnClickButtonGenerateJson_Click(object sender, RoutedEventArgs e)
        {
            var codeWriter = new CSharpCodeWriter();
            var rawJson = new TextRange(RichTextBoxJsonInput.Document.ContentStart, RichTextBoxJsonInput.Document.ContentEnd);

            if (CheckBoxToNewFile.IsChecked.HasValue)
                DoNewFile = CheckBoxToNewFile.IsChecked.Value;

            if (!string.IsNullOrEmpty(TextBoxClassname.Text))
                ModelClassname = TextBoxClassname.Text;

            var mainClass = "";
            if (!ModelClassname.Contains("."))
                mainClass = ModelClassname;
            else
            {
                var classnameSplit = ModelClassname.Split('.');
                if (classnameSplit.Last().Contains("cs"))
                {
                    ModelClassname = classnameSplit[0];
                }
            }

            ProjectNamespace = TextBoxNamespace.Text;

            var jsonClassGenerator = new JsonClassGenerator
            {
                Example = rawJson.Text,
                InternalVisibility = false,
                CodeWriter = codeWriter,
                ExplicitDeserialization = false,
                Namespace = ProjectNamespace,
                MainClass = mainClass,
                SecondaryNamespace = null,
                NoHelperClass = false,
                UseProperties = false,
                UsePascalCase = true,
                UseNestedClasses = false,
                ApplyObfuscationAttributes = false,
                SingleFile = true,
                ExamplesInDocumentation = false,
                TargetFolder = null
            };

            var classGenerator = jsonClassGenerator;
            using (var sw = new StringWriter())
            {
                classGenerator.OutputStream = sw;
                classGenerator.GenerateClasses();
                sw.Flush();
                FormattedJsonModel = sw.ToString();
            }
            DialogResult = true;
            Close();
        }
    }
}
