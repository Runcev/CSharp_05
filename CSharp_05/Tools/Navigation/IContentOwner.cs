using System.Windows.Controls;

namespace CSharp_05.Tools.Navigation
{
    internal interface IContentOwner
    {
        ContentControl ContentControl { get; }
    }
}