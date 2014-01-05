using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.Application.Threading.Tasks;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation.Search;
using JetBrains.ReSharper.Feature.Services.Occurences.Presentation;
using JetBrains.ReSharper.Feature.Services.Tips;
using JetBrains.ReSharper.Features.Common.GoToByName.Controllers;
using JetBrains.ReSharper.Features.Finding.GoToFile;
using JetBrains.UI.Application;
using JetBrains.UI.Application.Progress;
using JetBrains.UI.Controls.GotoByName;
using JetBrains.UI.GotoByName;
using JetBrains.Util;
using ProjectModelConstants = JetBrains.ProjectModel.DataContext.DataConstants;

namespace ReSharper.LocateFileInSolutionExplorer
{
    [ActionHandler(Id)]
    public class LocateFileAction : IActionHandler
    {
        public const string Id = "LocateFileInSolutionExplorer";

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            var solution = context.GetData(ProjectModelConstants.SOLUTION);
            if (solution == null)
            {
                MessageBox.ShowError("Cannot execute the Go To action because there's no solution open.");
                return;
            }

            var definition = Lifetimes.Define(solution.GetLifetime());

            var locks = Shell.Instance.GetComponent<IShellLocks>();
            var tasks = Shell.Instance.GetComponent<ITaskHost>();

            TipsManager.Instance.FeatureIsUsed("LocateFileInSolutionExplorer", (string)null);

            var controller = new LocateFileController(definition.Lifetime, solution, locks, tasks);
            EnableShowInFindResults(controller, definition);
            new GotoByNameMenu(context.GetComponent<GotoByNameMenuComponent>(),
                               definition,
                               controller.Model,
                               context.GetComponent<UIApplication>().MainWindow,
                               context.GetData(GotoByNameDataConstants.CurrentSearchText));
        }

        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            return context.CheckAllNotNull(ProjectModelConstants.SOLUTION);
        }

        private void EnableShowInFindResults(LocateFileController controller, LifetimeDefinition definition)
        {
            var itemsToPresent = new List<PresentableGotoItem>();
            controller.FuncEtcItemExecute.Value = delegate
            {
                Shell.Instance.Locks.ExecuteOrQueueReadLock("ShowInFindResults", delegate
                {
                    string filterString = controller.Model.FilterText.Value;
                    if (string.IsNullOrEmpty(filterString))
                    {
                        return;
                    }
                    filterString = filterString.Replace("*.", ".").Replace(".", "*.");
                    definition.Terminate();
                    GotoFileBrowserDescriptor descriptor = null;
                    if (!Shell.Instance.GetComponent<UITaskExecutor>().FreeThreaded.ExecuteTask("Show Files In Find Results", TaskCancelable.Yes, delegate(IProgressIndicator indicator)
                    {
                        indicator.TaskName = string.Format("Collecting symbols matching '{0}'", filterString);
                        indicator.Start(1);
                        ConsumeOccurences(controller, filterString, itemsToPresent);
                        if (itemsToPresent.Any() && !indicator.IsCanceled)
                        {
                            descriptor = CreateGotoFileBrowserDescriptor(controller.Solution, filterString,
                                from item in itemsToPresent
                                select item.Occurence, delegate
                                {
                                    itemsToPresent.Clear();
                                    ConsumeOccurences(controller, filterString, itemsToPresent);
                                    return itemsToPresent.Select(item => item.Occurence);
                                });
                        }
                        indicator.Stop();
                    }))
                    {
                        if (descriptor != null)
                        {
                            descriptor.LifetimeDefinition.Terminate();
                        }
                        return;
                    }
                    if (descriptor != null)
                    {
                        FindResultsBrowser.ShowResults(descriptor);
                    }
                });
            };
        }

        private static void ConsumeOccurences(LocateFileController controller, string filterString,
            List<PresentableGotoItem> pairsToPresent)
        {
            using (ReadLockCookie.Create())
                controller.ConsumePresentableItems(filterString, -1, (items, behavior) => pairsToPresent.AddRange(items));
        }

        protected virtual GotoFileBrowserDescriptor CreateGotoFileBrowserDescriptor(ISolution solution, string pattern, 
            [NotNull] IEnumerable<IOccurence> occurences, Func<IEnumerable<IOccurence>> restoreFunc)
        {
            return new GotoFileBrowserDescriptor(solution, "file", pattern, occurences, restoreFunc);
        }
    }
}