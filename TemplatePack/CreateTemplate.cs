using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using LigerShark.TemplateBuilder.Tasks;
using Microsoft.VisualStudio.Shell;
using RemoteTemplates;

namespace TemplatePack
{
    [Export(typeof(ICreateTemplate))]
    public class CreateTemplate : ICreateTemplate
    {
        public void AddToSolution(string pathToVsTemplate, string relativePathFromVsTemplateToContentRoot, Dictionary<string, string> replacementsDictionary)
        {
            var dte = ServiceProvider.GlobalProvider.GetService(typeof (DTE)) as DTE2;
            const string vstemplateXmlns = "http://schemas.microsoft.com/developer/vstemplate/2005";
            var templatePath = pathToVsTemplate;
            var dirName = Path.Combine(Path.GetDirectoryName(templatePath), relativePathFromVsTemplateToContentRoot);

            //Build a full VSTEMPLATE from a fragment if we're given one
            if (templatePath.EndsWith(".vstemplate.xml", StringComparison.InvariantCultureIgnoreCase))
            {
                //Set our target template path to the original template path but with the '.xml' stripped off
                var targetTemplatePath = templatePath.Substring(0, templatePath.Length - 4);
                //Get the project file so we can pass it to the method that explores the project to pull in all the included files
                var projFile = XDocument.Load(templatePath).Root.Element(XName.Get("TemplateContent", vstemplateXmlns)).Element(XName.Get("Project", vstemplateXmlns)).Attribute("File").Value;

                //Create the full template
                //TODO: Same principle as the next TODO, we shouldn't be calling the task, we should be calling non-MSBUILD specific logic & the task should call into the same method
                new CreateTemplateTask
                {
                    ProjectFile = Path.Combine(dirName, projFile),
                    VsTemplateShell = templatePath,
                    DestinationTemplateLocation = targetTemplatePath
                }.Execute();

                //Update the variable holding the location of our template file, point it to the generated full template
                templatePath = targetTemplatePath;
            }

            //Hedge our bets, if we were given a project file, say our source is the project file, other factors may change it later
            var templateFileProjectName = Path.GetFileName(templatePath);

            //If we've got a full VSTEMPLATE now, look for a _preprocess.xml and do replacements
            if (templatePath.EndsWith("vstemplate", StringComparison.InvariantCultureIgnoreCase))
            {
                templateFileProjectName = XDocument.Load(templatePath).Root.Element(XName.Get("TemplateContent", vstemplateXmlns)).Element(XName.Get("Project", vstemplateXmlns)).Attribute("File").Value;

                var preprocessXmlLocation = Path.Combine(dirName, "_preprocess.xml");

                if (File.Exists(preprocessXmlLocation))
                {
                    var task = new ReplaceInFiles
                    {
                        TemplateInfoFile = new Microsoft.Build.Utilities.TaskItem
                        {
                            ItemSpec = preprocessXmlLocation
                        },
                        RootDirectory = dirName
                    };

                    try
                    {
                        //Exception is always thrown here since the task calls the logger... Since MSBUILD didn't provide one (because MSBUILD didn't call the task - we did) an exception is thrown
                        //  about initialization not being complete
                        //TODO: Move the logic out of the task itself and into something we can call without this nastyness
                        task.Execute();
                    }
                    catch
                    {
                    }
                }
            }

            var targetPath = Path.Combine(dirName, Path.GetFileName(templatePath));

            //Replace task replaces the reference to the source project file we need, put it back
            if (templatePath.EndsWith("vstemplate", StringComparison.InvariantCultureIgnoreCase))
            {
                //Since we're dealing with a VSTEMPLATE, we'll have to move it out of the definitions folder so that the relative paths are correct... this should be evaluated more closely
                File.Move(templatePath, targetPath);
                var doc = XDocument.Load(targetPath);
                var proj = doc.Root.Element(XName.Get("TemplateContent", vstemplateXmlns)).Element(XName.Get("Project", vstemplateXmlns));
                proj.Attribute("File").Value = templateFileProjectName;
                doc.Save(targetPath);
            }

            //Invoke the normal template creation process, passing in the location the user selected to create the project in the initial dialog and the name of the project
            dte.Solution.AddFromTemplate(targetPath, replacementsDictionary["$destinationdirectory$"], replacementsDictionary["$projectname$"]);
        }

        public bool DemandWizardAssembly(string location)
        {
            if (MessageBox.Show("The selected template needs to download the wizard assembly located at " + location + ". Proceed?", "Wizard Download Required", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return false;
            }

            try
            {
                var request = WebRequest.CreateHttp(location);
                using (var stream = request.GetRequestStream())
                {
                    var assemblyRoot = Path.GetDirectoryName(new Uri(typeof (CreateTemplate).Assembly.CodeBase, UriKind.Absolute).LocalPath);
                    var wizards = Path.Combine(assemblyRoot, "DownloadedWizards");

                    if (!Directory.Exists(wizards))
                    {
                        Directory.CreateDirectory(wizards);
                    }

                    var tempLocation = Path.Combine(wizards, location.Split('/').Last());
                    using (var fs = File.Create(tempLocation))
                    {
                        stream.CopyTo(fs);
                        fs.Flush();
                    }

                    Assembly.LoadFrom(tempLocation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A problem occurred while attempting to download the wizard assembly at: " + location + Environment.NewLine + ex);
                return false;
            }

            return true;
        }
    }
}
