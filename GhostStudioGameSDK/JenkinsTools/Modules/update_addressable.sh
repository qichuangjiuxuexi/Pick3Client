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

echo "------------------------------Unity调用UpdateAddressable------------------------------"
exeMethod="AddressableUtil.BuildAddressableContents"
"$currentShellPath/invoke_unity_method.sh" "$projectPath" "$platform" "$exeMethod" "" || exit $?

# echo "------------------------------上传Addressable到git------------------------------"
# "$currentShellPath/git_push.sh" "$projectPath" "Update Addressable By Jenkins" "Assets/AddressableAssetsData" "Assets/Project/AddressableRes/Common/Scripts/Definition/AAConst.cs"
# exit 0
