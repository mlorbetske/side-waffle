using System.Collections.Generic;

namespace RemoteTemplates
{
    public interface ICreateTemplate
    {
        /// <summary>
        /// Adds an instance of a template to the solution
        /// </summary>
        /// <param name="pathToVsTemplate">The path on disk to the VSTEMPLATE (or VSTEMPLATE.XML) file to create the template from</param>
        /// <param name="relativePathFromVsTemplateToContentRoot">The relative path from the <paramref name="pathToVsTemplate"/> to the root of the content listed in the template file</param>
        /// <param name="replacementsDictionary">The set of replacements to make in the resulting template, keys are the tokens to replace, values are the values to replace them with (should be passed directly from the wizard's RunStarted method)</param>
        /// <example>
        /// If the template file is located at c:\temp\definitions\_project.vstemplate.xml, but refers to content in c:\temp, the method would be called with these parameters
        /// AddToSolution(@"c:\temp\definitions\_project.vstemplate.xml", @"..\");
        /// 
        /// Remember to throw a WizardCancelledException just after calling this method
        /// </example>
        void AddToSolution(string pathToVsTemplate, string relativePathFromVsTemplateToContentRoot, Dictionary<string, string> replacementsDictionary);

        /// <summary>
        /// Demands that a particular assembly containing a wizard be made available
        /// </summary>
        /// <param name="location">The location of the wizard</param>
        bool DemandWizardAssembly(string location);
    }
}
