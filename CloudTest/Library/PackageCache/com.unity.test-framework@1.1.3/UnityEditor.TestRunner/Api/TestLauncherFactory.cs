using System;
using System.Linq;
using UnityEngine.TestTools;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal class TestLauncherFactory : ITestLauncherFactory
    {
        public TestLauncherBase GetLauncher(ExecutionSettings executionSettings)
        {
            var filters = GetFilters(executionSettings);
            if (filters[0].testMode == TestMode.EditMode || filters[0].testMode == 0)
            {
                return GetEditModeLauncher(GetFilters(executionSettings), executionSettings.runSynchronously);
            }
            else
            {
                if (executionSettings.runSynchronously)
                    throw new NotSupportedException("Playmode tests cannot be run synchronously.");
                
                var settings = PlaymodeTestsControllerSettings.CreateRunnerSettings(filters.Select(filter => filter.ToTestRunnerFilter()).ToArray());
                return GetPlayModeLauncher(settings, executionSettings);
            }
        }

        static Filter[] GetFilters(ExecutionSettings executionSettings)
        {
            if (executionSettings.filters != null && executionSettings.filters.Length > 0)
            {
                return executionSettings.filters;
            }

            return new[] {executionSettings.filter ?? new Filter()};
        }

        static TestLauncherBase GetEditModeLauncher(Filter[] filters, bool runSynchronously)
        {
            return GetEditModeLauncherForProvidedAssemblies(filters, TestPlatform.EditMode, runSynchronously);
        }

        static TestLauncherBase GetPlayModeLauncher(PlaymodeTestsControllerSettings settings, ExecutionSettings executionSettings)
        {
            if (executionSettings.targetPlatform != null)
            {
                return GetPlayerLauncher(settings, executionSettings);
            }

            if (PlayerSettings.runPlayModeTestAsEditModeTest)
            {
                return GetEditModeLauncherForProvidedAssemblies(executionSettings.filters, TestPlatform.PlayMode, false);
            }

            return GetPlayModeLauncher(settings);
        }

        static TestLauncherBase GetEditModeLauncherForProvidedAssemblies(Filter[] filters, TestPlatform testPlatform, bool runSynchronously)
        {
            return new EditModeLauncher(filters, testPlatform, runSynchronously);
        }

        static TestLauncherBase GetPlayModeLauncher(PlaymodeTestsControllerSettings settings)
        {
            return new PlaymodeLauncher(settings);
        }

        static TestLauncherBase GetPlayerLauncher(PlaymodeTestsControllerSettings settings, ExecutionSettings executionSettings)
        {
            return new PlayerLauncher(
                settings,
                executionSettings.targetPlatform.Value,
                executionSettings.overloadTestRunSettings);
        }
    }
}
