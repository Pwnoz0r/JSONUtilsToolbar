//------------------------------------------------------------------------------
// <copyright file="JsonCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace JsonUtilsToolbar
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class JsonCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        private readonly DTE2 _dte2;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("cc79efcc-e893-431b-bb8a-d67f436aa2d1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private JsonCommand(Package package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            _package = package;
            _dte2 = ServiceProvider.GetService(typeof(DTE)) as DTE2;
            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (commandService == null) return;

            var menuCommandId = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(ParseJson, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static JsonCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => _package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new JsonCommand(package);
        }

        private static IEnumerable<Project> GetProjects(IVsSolution solution)
        {
            return GetProjectsInSolution(solution).Select(GetDteProject).Where(project => project != null);
        }

        private static IEnumerable<IVsHierarchy> GetProjectsInSolution(IVsSolution solution, __VSENUMPROJFLAGS flags = __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION)
        {
            if (solution == null)
                yield break;

            IEnumHierarchies enumHierarchies;
            var guid = Guid.Empty;
            solution.GetProjectEnum((uint)flags, ref guid, out enumHierarchies);
            if (enumHierarchies == null)
                yield break;

            var hierarchy = new IVsHierarchy[1];
            uint fetched;
            while (enumHierarchies.Next(1, hierarchy, out fetched) == VSConstants.S_OK && fetched == 1)
            {
                if (hierarchy.Length > 0 && hierarchy[0] != null)
                    yield return hierarchy[0];
            }
        }

        private static Project GetDteProject(IVsHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            object obj;
            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out obj);
            return obj as Project;
        }

        private static void ParseJson(object sender, EventArgs e)
        {
            var solution = (IVsSolution)Package.GetGlobalService(typeof(IVsSolution));
            var project = GetProjects(solution).First();

            if (project == null) return;

            var projectFile = project.FileName;
            var projectDirectory = Path.GetDirectoryName(projectFile);
            var projectName = project.Name;

            var dialog = new JsonDialog(projectName);
            dialog.ShowDialog();

            if (!dialog.DoNewFile) return;

            var fileName = dialog.ModelClassname;
            if (!fileName.Contains("."))
                fileName = $"{fileName}.cs";
            else if (!fileName.Split('.').Last().Contains("cs"))
                fileName = $"{fileName}.cs";

            var extendedNamespacePath = "";
            if (dialog.ProjectNamespace.Contains("."))
            {
                var projectStructure = dialog.ProjectNamespace.Split('.');
                var strArrays = projectStructure;
                var tempPath = strArrays.Where(s => projectName != s).Aggregate("", Path.Combine);

                if (projectDirectory != null)
                {
                    Directory.CreateDirectory(Path.Combine(projectDirectory, tempPath));
                    extendedNamespacePath = Path.Combine(projectDirectory, tempPath, fileName);
                }
            }

            if (projectDirectory == null) return;

            var finalFileName = extendedNamespacePath == "" ? Path.Combine(projectDirectory, fileName) : extendedNamespacePath;
            try
            {
                using (var streamWriter = new StreamWriter(finalFileName))
                {
                    streamWriter.Write(dialog.FormattedJsonModel);
                }
            }
            catch (Exception)
            {
                // ignored
            }
            if (!fileName.StartsWith(projectDirectory, StringComparison.OrdinalIgnoreCase))
            {
                project.ProjectItems.AddFromFile(finalFileName);
            }
        }
    }
}
