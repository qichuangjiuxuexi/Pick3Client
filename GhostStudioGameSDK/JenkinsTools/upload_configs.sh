#!/bin/sh

if [ $# -lt 4 ] ; then
    echo "Usage: $0 projectPath branch uploadKeys deleteKeys"
    exit -1
fi
echo $0 $@

projectPath="$1"
echo "projectPath: $projectPath"
shift

branch="${1:-main}"
echo "branch: $branch"
shift

uploadKeys="${1}"
echo "uploadKeys: $uploadKeys"
shift

deleteKeys="${1}"
echo "deleteKeys: $deleteKeys"
shift

echo "------------------------------检查环境------------------------------"
currentShellPath=$(cd "$(dirname "$0")"; pwd)
projectPath="$currentShellPath/../../../$projectPath"
platform="Android"

if [[ ! -d "${projectPath}" ]]; then
    echo "Unity 工程不存在: ${projectPath}" >&2
    exit -1
fi

configPath="${projectPath/%Client/Config}"
if [[ ! -d "${configPath}" ]]; then
    echo "Config 工程不存在: ${configPath}" >&2
    exit -1
fi

echo "------------------------------清理输出目录------------------------------"
buildPath="$projectPath/../Exports"
rm -rf "$buildPath"

echo "------------------------------拉取最新配置------------------------------"
"$currentShellPath/Modules/switch_branch.sh" "$projectPath" "$branch" || exit $?
"$currentShellPath/Modules/switch_branch.sh" "$configPath" "main" || exit $?

echo "------------------------------压缩ASTC------------------------------"
"$currentShellPath/Modules/astc_sprite_importer.sh" "$projectPath" "$platform" || exit $?

echo "------------------------------Unity调用UpdateConfigs------------------------------"
exeMethod="UpdateConfigUtil.UpdateConfigScripts"
"$currentShellPath/Modules/invoke_unity_method.sh" "$projectPath" "$platform" "$exeMethod" "" || exit $?
exeMethod="UpdateConfigUtil.UpdateConfigData"
"$currentShellPath/Modules/invoke_unity_method.sh" "$projectPath" "$platform" "$exeMethod" "" || exit $?
exeMethod="ExportConfigUtil.ExportConfigs"
"$currentShellPath/Modules/invoke_unity_method.sh" "$projectPath" "$platform" "$exeMethod" "" || exit $?

echo "------------------------------上传配置到firebase------------------------------"
"$currentShellPath/Modules/firebase_upload_config.sh" "$uploadKeys" "$deleteKeys" || exit $?
