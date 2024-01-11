#!/bin/sh

if [ $# -lt 1 ] ; then
    echo "Usage: $0 projectPath"
    exit -1
fi
echo $0 $@

projectPath="$1"
echo "projectPath: $projectPath"
shift

#------------------------------检查环境------------------------------
jsonPath="$projectPath/Assets/StreamingAssets/google-services-desktop.json"
if [[ ! -f "${jsonPath}" ]]; then
    echo "Firebase配置文件不存在: ${jsonPath}" >&2
    exit 0
fi

buildPath="$projectPath/../Build"
if [[ ! -d "${buildPath}" ]]; then
    echo "生成目录不存在: ${buildPath}" >&2
    exit -1
fi

symbolPath=$(find "${buildPath}" -maxdepth 1 -type f -name "*.symbols.zip" | head -n 1 | xargs -I {} readlink -f "{}")
if [[ ! -f "${symbolPath}" ]]; then
    echo "符号表不存在: ${buildPath}" >&2
    exit -1
fi

#------------------------------上传符号表------------------------------
firebase_app_id=$(jq -r '.client[0].client_info.mobilesdk_app_id' "$jsonPath" 2>/dev/null)
echo firebase crashlytics:symbols:upload --app=$firebase_app_id "$symbolPath"
firebase crashlytics:symbols:upload --app=$firebase_app_id "$symbolPath"
