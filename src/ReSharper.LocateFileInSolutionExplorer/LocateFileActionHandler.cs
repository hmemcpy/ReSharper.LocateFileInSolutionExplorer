using System;
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.VsIntegration.ActionManagement;

namespace ReSharper.LocateFileInSolutionExplorer
{
    [ShellComponent]
    public class LocateFileActionHandler : IActionHandler
    {
        private const string locateInSolutionExplorerId = "LocateInSolutionExplorer";

        [NotNull]
        private readonly IActionManager actionManager;

        public LocateFileActionHandler([NotNull] IActionManager manager, Lifetime lifetime)
        {
            actionManager = manager;

            var gotoTypeAction = manager.TryGetAction(locateInSolutionExplorerId) as IUpdatableAction;
            if (gotoTypeAction != null)
            {
                gotoTypeAction.AddHandler(lifetime, this);
            }
        }

        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            return nextUpdate();
        }

        private bool isDoubleCompletion;

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            var vsActionManager = actionManager as VsActionManager;
            if (vsActionManager == null || 
                vsActionManager.LastActionToExecute == null)
                return;

            if (vsActionManager.LastActionToExecute.Id == locateInSolutionExplorerId && !isDoubleCompletion)
            {
                try
                {
                    nextExecute();
                }
                finally
                {
                    isDoubleCompletion = true;
                }

                return;
            }
            
            var locateFileAction = actionManager.TryGetAction(LocateFileAction.Id) as IExecutableAction;
            if (locateFileAction != null)
            {
                try
                {
                    locateFileAction.Execute(context);
                }
                finally
                {
                    isDoubleCompletion = false;
                }
            }
        }
    }
}