using System.Xml.Linq;

namespace Realize
{
    public class ProjectProcessor
    {
        private bool _isDryRun;
        private bool _replaceExisting;
        private DirectoryInfo _currentDirectory;
        private FileInfo _inputProj;
        private FileInfo _outputProj;

        public ProjectProcessor(bool isDryRun, bool replaceExisting, DirectoryInfo currentDirectory, FileInfo inputProj, FileInfo outputProj)
        {
            _isDryRun = isDryRun;
            _replaceExisting = replaceExisting;
            _currentDirectory = currentDirectory;
            _inputProj = inputProj;
            _outputProj = outputProj;
        }

        public void Process()
        {
            XElement projectDocument;
            XDocument outputDocument = new(new XDeclaration("1.0", "utf-8", "yes"));

            try
            {
                projectDocument = XElement.Load(_inputProj.FullName);
            }
            catch
            {
                throw;
            }
            if (projectDocument == null)
            {
                throw new NullReferenceException($"Unable to load {_inputProj.FullName}");
            }

            var itemGroupXname = XName.Get("ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003");
            var compileXname = XName.Get("Compile", "http://schemas.microsoft.com/developer/msbuild/2003");
            var linkXname = XName.Get("Link", "http://schemas.microsoft.com/developer/msbuild/2003");
            var includeXname = XName.Get("Include", "http://schemas.microsoft.com/developer/msbuild/2003");

            var listOfElements = new List<CompileItem>();

            foreach (var descendent in projectDocument.Descendants(compileXname))
            {
                if ((descendent.Attributes().FirstOrDefault()?.Name ?? string.Empty) == "Include" &&
                    (descendent.Descendants(linkXname).Count() > 0))
                {
                    var filePath = descendent.Attributes().First().Value;
                    var linkPath = descendent.Descendants(linkXname).First().Value;

                    var source = Path.Combine(_inputProj.DirectoryName, filePath);
                    var destination = Path.Combine(_inputProj.DirectoryName, linkPath);

                    if (_isDryRun)
                    {
                        Console.WriteLine($"DRYRUN: Copying {filePath} to {linkPath}");
                    }
                    else
                    {
                        Console.WriteLine($"Copying {filePath} to {linkPath}");
                        FileUtils.CreateDirectoryPath(destination);
                        File.Copy(source, destination, overwrite: true);
                        listOfElements.Add(new CompileItem { ActualElement = descendent, IncludePath= filePath, LinkPath = linkPath});
                    }

                }
            }

            if ((!_isDryRun && _outputProj != null) || (!_isDryRun && _replaceExisting))
            {
                foreach (var item in listOfElements)
                {
                    // removes the value
                    Console.WriteLine($"Rewriting {item.IncludePath} to {item.LinkPath}");
                    item.ActualElement.SetElementValue(linkXname, null);
                    item.ActualElement.SetAttributeValue("Include", item.LinkPath);
                }

                if (_replaceExisting)
                {
                    var backupCopyFilename = FileUtils.NextAvailableFilename(_inputProj.FullName);
                    File.Copy(_inputProj.FullName, backupCopyFilename);
                    projectDocument.Save(_inputProj.FullName);
                }
                else
                {
                    projectDocument.Save(_outputProj.FullName);
                }
            }
            else
            {
                Console.WriteLine(outputDocument);
            }
        }

        private void WriteToOutput(XDocument outputDocument, XElement element)
        {
            //outputDocument.Add(element);
            Console.WriteLine(element);
        }
    }
}
