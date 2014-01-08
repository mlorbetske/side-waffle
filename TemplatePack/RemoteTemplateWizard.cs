using System.Collections.Generic;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace TemplatePack
{
    public class RemoteTemplateWizard : IWizard
    {
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            MessageBox.Show("Custom wizard called");

            var creator = new CreateTemplate();
            creator.AddToSolution(pathToVsTemplateInTempDirectory, );
            throw new WizardCancelledException();
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }
    }
}
