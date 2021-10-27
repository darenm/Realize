using System.Xml.Linq;

namespace Realize
{
    internal class CompileItem
    {
        public XElement ActualElement { get; set; }
        public string IncludePath { get; set; }
        public string LinkPath { get; set; }
    }
}
