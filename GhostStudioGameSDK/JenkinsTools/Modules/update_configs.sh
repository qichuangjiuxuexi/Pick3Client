#!/bin/sh

if [ $# -lt 1 ] ; then
    echo "Usage: $0 projectPath platform"
    exit -1
fi
echo $0 $@

projectPath="$1"
echo "projectPath: $projectPath"
shift

platform="${1:-Android}"
echo "platform: $platform"
shift

echo "------------------------------检查环境------------------------------"
currentShellPath=$(cd "$(dirname "$0")"; pwd)
if [[ ! -d "${projectPath}" ]]; then
    echo "Unity 工程不存在: ${projectPath}" >&2
    exit -1
fi

configPath="${projectPath/%Client/Config}"
if [[ ! -d "${configPath}" ]]; then
    echo "Config 工程不存在: ${configPath}" >&2
    exit -1
fi

echo "------------------------------拉取最新配置------------------------------"
"$currentShellPath/switch_branch.sh" "$configPath" "main" || exit $?

echo "------------------------------Unity调用UpdateConfigs------------------------------"
exeMethod="UpdateConfigUtil.UpdateConfigScripts"
"$currentShellPath/invoke_unity_method.sh" "$projectPath" "$platform" "$exeMethod" "" || exit $?
exeMethod="UpdateConfigUtil.UpdateConfigData"
"$currentShellPath/invoke_unity_method.sh" "$projectPath" "$platform" "$exeMethod" "" || exit $?

echo "------------------------------上传配置到git------------------------------"
"$currentShellPath/git_push.sh" "$projectPath" "Update Config By Jenkins" "Assets/Project/AddressableRes/Configs"
exit 0
