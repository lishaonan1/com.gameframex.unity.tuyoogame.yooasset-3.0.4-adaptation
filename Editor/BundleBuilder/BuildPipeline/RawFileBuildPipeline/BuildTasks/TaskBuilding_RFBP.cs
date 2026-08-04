using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset.Editor
{
    /// <summary>
    /// 原生文件构建管线的文件拷贝任务
    /// </summary>
    public class TaskBuilding_RFBP : IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            CopyRawFileBundles(buildMapContext, buildParametersContext, false);
        }

        /// <summary>
        /// 拷贝原生文件
        /// </summary>
        internal static void ValidateRawFileBundles(BuildMapContext buildMapContext)
        {
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                if (bundleInfo.IsRawFileBundle == false)
                    continue;

                if (bundleInfo.AllPackAssets.Count != 1)
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.NotSupportMultipleRawAsset,
                        $"原生文件资源包不支持包含多个原生文件：'{bundleInfo.BundleName}'。");
                    throw new InvalidOperationException(message);
                }
            }
        }

        /// <summary>
        /// 拷贝原生文件资源包
        /// </summary>
        /// <param name="buildMapContext">构建映射上下文</param>
        /// <param name="buildParametersContext">构建参数上下文</param>
        /// <param name="onlyRawFileBundles">是否只处理名称带有 rawfile 后缀的资源包</param>
        internal static void CopyRawFileBundles(BuildMapContext buildMapContext, BuildParametersContext buildParametersContext, bool onlyRawFileBundles)
        {
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            ValidateRawFileBundles(buildMapContext);
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                if (onlyRawFileBundles && bundleInfo.IsRawFileBundle == false)
                    continue;

                if (bundleInfo.AllPackAssets.Count != 1)
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.NotSupportMultipleRawAsset,
                        $"原生文件资源包不支持包含多个原生文件：'{bundleInfo.BundleName}'。");
                    throw new InvalidOperationException(message);
                }

                string dest = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                var buildAsset = bundleInfo.AllPackAssets[0];
                EditorFileUtility.CopyFile(buildAsset.AssetInfo.AssetPath, dest, true);
            }
        }

        /// <summary>
        /// 验证原生文件资源包已生成构建产物
        /// </summary>
        internal static void VerifyRawFileBundles(BuildMapContext buildMapContext, BuildParametersContext buildParametersContext)
        {
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                if (bundleInfo.IsRawFileBundle == false)
                    continue;

                string filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                if (File.Exists(filePath) == false)
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.MissingExpectedBundle,
                        $"原生文件资源包构建产物不存在：'{filePath}'。");
                    throw new InvalidOperationException(message);
                }
            }
        }
    }
}
