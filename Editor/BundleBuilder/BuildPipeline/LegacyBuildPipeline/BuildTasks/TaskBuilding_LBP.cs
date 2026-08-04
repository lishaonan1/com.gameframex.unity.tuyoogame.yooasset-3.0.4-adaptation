using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 旧版构建管线的资源包构建任务
    /// </summary>
    public class TaskBuilding_LBP : IBuildTask
    {
        /// <summary>
        /// 旧版构建管线的构建结果上下文
        /// </summary>
        [ContextObject]
        public class BuildResultContext
        {
            /// <summary>
            /// Unity 引擎生成的 AssetBundleManifest
            /// </summary>
            public AssetBundleManifest UnityManifest;
        }

        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var legacyBuildParameters = buildParametersContext.Parameters as LegacyBuildParameters;

            // 开始构建
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            BuildAssetBundleOptions buildOptions = legacyBuildParameters.GetBundleBuildOptions();
            TaskBuilding_RFBP.ValidateRawFileBundles(buildMapContext);
            var bundleBuilds = buildMapContext.GetPipelineBuilds(legacyBuildParameters.ReplaceAssetPathWithAddress, false);
            AssetBundleManifest unityManifest = null;
            if (bundleBuilds.Length > 0)
            {
                unityManifest = BuildPipeline.BuildAssetBundles(pipelineOutputDirectory, bundleBuilds, buildOptions, buildParametersContext.Parameters.BuildTarget);
                if (unityManifest == null)
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFailed, "UnityEngine build failed.");
                    throw new InvalidOperationException(message);
                }

                // 检测输出目录
                string unityOutputManifestFilePath = $"{pipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}";
                if (System.IO.File.Exists(unityOutputManifestFilePath) == false)
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFatal, $"Output {nameof(AssetBundleManifest)} file not found: '{unityOutputManifestFilePath}'.");
                    throw new InvalidOperationException(message);
                }

                BuildLogger.Log("UnityEngine build succeeded.");
            }

            // PackRawFile 资源包不是 Unity AssetBundle，必须在引擎构建后以原始文件形式输出。
            TaskBuilding_RFBP.CopyRawFileBundles(buildMapContext, buildParametersContext, true);
            TaskBuilding_RFBP.VerifyRawFileBundles(buildMapContext, buildParametersContext);

            BuildResultContext buildResultContext = new BuildResultContext();
            buildResultContext.UnityManifest = unityManifest;
            context.SetContextObject(buildResultContext);
        }
    }
}
