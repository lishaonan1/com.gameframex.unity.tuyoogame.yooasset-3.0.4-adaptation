using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 旧版构建管线的清单文件创建任务
    /// </summary>
    public class TaskCreateManifest_LBP : TaskCreateManifest, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var legacyBuildParameters = buildParametersContext.Parameters as LegacyBuildParameters;
            bool replaceAssetPathWithAddress = legacyBuildParameters.ReplaceAssetPathWithAddress;
            CreateManifestFile(true, true, replaceAssetPathWithAddress, context);
        }

        protected override string[] GetBundleDepends(BuildContext context, string bundleName)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            if (buildMapContext.GetBundleInfo(bundleName).IsRawFileBundle)
                return Array.Empty<string>();

            var buildResultContext = context.GetContextObject<TaskBuilding_LBP.BuildResultContext>();
            if (buildResultContext.UnityManifest == null)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleInBuildResult, $"Unity 资源包构建结果不存在：'{bundleName}'。");
                throw new InvalidOperationException(message);
            }
            return buildResultContext.UnityManifest.GetAllDependencies(bundleName);
        }
    }
}
