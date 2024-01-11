#!/bin/sh

if [ $# -lt 6 ] ; then
    echo "Usage: $0 projectPath packageDir branchName platform releaseBuild buildVersion updateConfig isApk"
    exit -1
fi

#Unity工程目录
projectPath="$1"
echo "projectPath: $projectPath"
shift

#打包输出目录
packageDir="$1"
echo "packageDir: $packageDir"
shift

#分支名称
branchName="${1:-main}"
echo "branchName: $branchName"
shift

#打包平台：Android/iOS
platform="${1:-Android}"
echo "platform: $platform"
shift

#是否是正式包
releaseBuild="${1:-false}"
echo "releaseBuild: $releaseBuild"
shift

#打包序号
buildVersion="${1:-1}"
echo "buildVersion: $buildVersion"
shift

#是否更新配置
updateConfig="${1:-false}"
echo "updateConfig: $updateConfig"
shift

#正式包是否是APK
isApk="${1:-false}"
echo "isApk: $isApk"
shift

echo "------------------------------检查环境------------------------------"
currentShellPath=$(cd "$(dirname "$0")"; pwd)
if [[ ! -d "${projectPath}" ]]; then
    echo "Unity 工程不存在: ${projectPath}" >&2
    exit -1
fi

if [[ "$platform" == "iOS" ]] && [[ -z "$keychainPassword" ]]; then
    echo "keychainPassword 环境变量未设置" >&2
    exit -1
fi

cd "$projectPath"
if [[ -f editor.pid ]]; then
    /bin/echo -n "--- 杀死上次残留的Unity进程: "
    cat editor.pid
    pkill -F editor.pid
fi

echo "------------------------------清理输出目录------------------------------"
buildPath="$projectPath/../Build"
rm -rf "$buildPath"

echo "------------------------------压缩ASTC------------------------------"
"$currentShellPath/Modules/astc_sprite_importer.sh" "$projectPath" "$platform" || exit $?

if [[ "$updateConfig" == "true" ]]; then
    echo "------------------------------拉取最新配置------------------------------"
    "$currentShellPath/Modules/update_configs.sh" "$projectPath" "$platform" || exit $?
fi

echo "------------------------------更新Addressable------------------------------"
"$currentShellPath/Modules/update_addressable.sh" "$projectPath" "$platform" || exit $?

echo "------------------------------Unity构建------------------------------"
if [[ "$platform" == "Android" ]]; then
    if [[ "$releaseBuild" == "true" ]]; then
        if [[ "$isApk" == "true" ]]; then
            exeMethod="BuildNative.BuildAndroidReleaseApk"
            extention="apk"
        else
            exeMethod="BuildNative.BuildAndroidReleaseAab"
            extention="aab"
        fi
    else
        exeMethod="BuildNative.BuildAndroidDebug"
        extention="apk"
    fi
else
    if [[ "$releaseBuild" == "true" ]]; then
        exeMethod="BuildNative.BuildIOSRelease"
    else
        exeMethod="BuildNative.BuildIOSDebug"
    fi
    extention="ipa"
fi
"$currentShellPath/Modules/invoke_unity_method.sh" "$projectPath" "$platform" "$exeMethod" || exit $?

echo "------------------------------生成包------------------------------"
if [[ "$platform" == "iOS" ]]; then
    "$currentShellPath/Modules/xcode_build.sh" "$projectPath" "$releaseBuild" || exit $?
fi
if ! ls "${buildPath}"/*.$extention; then
    echo "打包生成失败: ${buildPath}" >&2
    exit -1
fi

echo "------------------------------拷贝包------------------------------"
"$currentShellPath/Modules/copy_package.sh" "$projectPath" "$packageDir" "$releaseBuild" "$buildVersion" || exit $?

echo "------------------------------上传符号表------------------------------"
if [[ "$platform" == "Android" ]] && [[ "$releaseBuild" == "true" ]] && [[ "$isApk" == "false" ]]; then
    "$currentShellPath/Modules/firebase_upload_symbols.sh" "$projectPath" || exit $?
fi
