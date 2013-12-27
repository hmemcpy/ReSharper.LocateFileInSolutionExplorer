using System;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Threading.Tasks;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Occurences;
using JetBrains.ReSharper.Features.Common.ComponentsAPI;
using JetBrains.ReSharper.Features.Common.GoToByName;
using JetBrains.ReSharper.Features.Common.GoToByName.Controllers;
using JetBrains.ReSharper.Features.Common.GoToByName.ModelInitializers;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.UI.GotoByName;
using JetBrains.UI.PopupMenu.Impl;

namespace ReSharper.LocateFileInSolutionExplorer
{
    public class LocateFileController : GotoFileController
    {
        public LocateFileController([NotNull] Lifetime lifetime, [NotNull] ISolution solution, [NotNull] IShellLocks locks, ITaskHost tasks, bool enableMulticore = false)
            : base(lifetime, solution, locks, tasks, enableMulticore)
        {
            Model.CaptionText.Value = "Enter file or folder name to locate:";
        }

        protected override bool ExecuteItem(JetPopupMenuItem item, ISignal<bool> closeBeforeExecute)
        {
            closeBeforeExecute.Fire(true);
            var occurence = item.Key as ProjectItemOccurence;
            if (occurence == null || occurence.ProjectItem == null)
            {
                return false;
            }

            using (CommitCookie.Commit(Solution))
            {
                return Solution.GetComponent<ISolutionExplorer>().ShowInSolutionExplorer(occurence.ProjectItem, false);
            }
        }
    }
}