using System.IO;

namespace YooAsset.Editor
{
    /// <summary>
    /// 可编程构建管线的补丁包创建任务
    /// </summary>
    public class TaskCreatePackage_SBP : TaskCreatePackage, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            CreatePackagePatch(buildParametersContext, buildMapContext);
        }

        /// <summary>
        /// 拷贝补丁文件到补丁包目录
        /// </summary>
        private void CreatePackagePatch(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
        {
            var scriptableBuildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            BuildLogger.Log($"Start making patch package: '{packageOutputDirectory}'.");

            // 纯 RawFile 包没有 SBP 引擎构建日志。
            string buildLogPath = $"{pipelineOutputDirectory}/buildlogtep.json";
            if (File.Exists(buildLogPath))
                CopyPipelineFile(pipelineOutputDirectory, packageOutputDirectory, "buildlogtep.json");

            // 拷贝代码防裁剪配置
            string linkXmlPath = $"{pipelineOutputDirectory}/link.xml";
            if (scriptableBuildParameters.WriteLinkXML && File.Exists(linkXmlPath))
            {
                CopyPipelineFile(pipelineOutputDirectory, packageOutputDirectory, "link.xml");
            }

            // 拷贝所有补丁文件
            CopyPackageBundles(buildMapContext);
        }
    }
}
