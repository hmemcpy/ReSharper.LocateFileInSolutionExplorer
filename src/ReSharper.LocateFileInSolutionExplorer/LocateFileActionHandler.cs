using System;
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using DataConstants = JetBrains.TextControl.DataContext.DataConstants;

namespace ReSharper.LocateFileInSolutionExplorer
{
    [ShellComponent]
    public class LocateFileActionHandler : IActionHandler
    {
        private const string LocateInSolutionExplorerId = "LocateInSolutionExplorer";

        [NotNull] private readonly IActionManager actionManager;
        [NotNull] private readonly object syncLock = new object();
        private DateTime lastLocateInvocation = DateTime.MinValue;
        private bool lastUnderlyingActionUpdate;

        public LocateFileActionHandler([NotNull] Lifetime lifetime, [NotNull] IActionManager manager)
        {
            actionManager = manager;

            var locateInSolutionAction = manager.TryGetAction(LocateInSolutionExplorerId) as IUpdatableAction;
            if (locateInSolutionAction != null)
            {
                locateInSolutionAction.AddHandler(lifetime, this);
            }
        }

        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            // this allows double [Shift+Alt+L] to work even when there is no project file context
            // available (for example, when 'References' project node is focused in Solution Explorer)
            // and builtin 'LocateInSolutionExplorer' action is not normally available
            lastUnderlyingActionUpdate = nextUpdate();

            // and it's better to do this:
            var locateFileAction = actionManager.TryGetAction(LocateFileAction.Id) as IUpdatableAction;
            return locateFileAction == null || locateFileAction.Update(context);
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            var textControl = context.GetData(DataConstants.TEXT_CONTROL);
            GC.KeepAlive(textControl);

            DateTime lastInvocation, utcNow = DateTime.UtcNow;
            lock (syncLock)
            {
                lastInvocation = this.lastLocateInvocation;
                this.lastLocateInvocation = utcNow;
            }

            const int doubleKeyPressDelay = 500;
            if (utcNow.Subtract(lastInvocation).TotalMilliseconds < doubleKeyPressDelay)
            {
                var locateFileAction = actionManager.TryGetAction(LocateFileAction.Id) as IExecutableAction;
                if (locateFileAction != null)
                {
                    locateFileAction.Execute(context);
                    return;
                }
            }

            if (lastUnderlyingActionUpdate)
            {
                nextExecute();
            }
        }
    }
}