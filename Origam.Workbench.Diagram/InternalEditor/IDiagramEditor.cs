using System;

namespace Origam.Workbench.Diagram.InternalEditor
{
    interface IDiagramEditor: IDisposable
    {
        void ReDrawAndReselect();
    }
}